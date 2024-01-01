using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AssetsManager : MonoBehaviour{	public static AssetsManager instance;
	[Header("Main")]
	[AssetsOnly,Searchable]public List<GObject> objects;
	[AssetsOnly,Searchable]public List<GObject> vfx;
	[AssetsOnly,Searchable]public List<GSprite> sprites;
	[AssetsOnly,Searchable]public List<GMaterial> materials;
	[AssetsOnly,Searchable]public TMPro.TMP_InputValidator decimalInputValidator;
	[AssetsOnly,Searchable]public TMPro.TMP_InputValidator angleInputValidator;
	[AssetsOnly,Searchable]public TMPro.TMP_InputValidator delayInputValidator;
	// [DisableInEditorMode][SerializeField]public Sprite[] generatedLevelPreviews;
	[DisableInEditorMode][SerializeField]public Texture2D[] generatedLevelPreviews;
    
    void Awake(){if(instance!=null){Destroy(gameObject);}else{DontDestroyOnLoad(gameObject);instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
	void Start(){}

#region//Main
    public GameObject Make(string str, Vector2 pos){
		GObject o=objects.Find(x=>x.name==str);
		if(o==null){
			Debug.LogWarning("Object: " + str + " not found!");
			return null;
		}
        GameObject objref=Instantiate(o.gobj,pos,Quaternion.identity);
        return objref;
	}
    public GameObject MakeSpread(string str, Vector2 pos, int amnt, float rangeX=0.5f, float rangeY=0.5f){
		GObject o=objects.Find(x=>x.name==str);
		if(o==null){
			Debug.LogWarning("Object: " + str + " not found!");
			return null;
		}
		GameObject objref=Instantiate(o.gobj,pos,Quaternion.identity);
		for(var i=0;i<amnt-1;i++){
			var rndX=UnityEngine.Random.Range(-rangeX,rangeX);
			var rndY=UnityEngine.Random.Range(-rangeY,rangeY);
			var poss=pos+new Vector2(rndX,rndY);
			Instantiate(o.gobj,poss,Quaternion.identity);
		}
        return objref;
	}
    public GameObject VFX(string str, Vector2 pos, float duration=0){
		GObject o=vfx.Find(x=>x.name==str);
		if(o==null){
			Debug.LogWarning("Object: " + str + " not found!");
			return null;
		}
		GameObject gobj=o.gobj;
        GameObject objref;
		//if(SaveSerial.instance.settingsData.particles){
			objref=Instantiate(gobj,pos,Quaternion.identity);
			objref.transform.position=pos;
			if(duration!=0)Destroy(objref,duration);
			return objref;
		//}else return null;
	}
    public GameObject Get(string str,bool ignoreWarning=false){
		GObject o=objects.Find(x=>x.name==str);
		if(o==null){
			if(!String.IsNullOrEmpty(str)&&!ignoreWarning)Debug.LogWarning("Object: " + str + " not found!");
			return null;
		}
		GameObject gobj=o.gobj;
        return gobj;
	}
	public Sprite GetObjSpr(string str,bool ignoreWarning=false){
		GObject o=objects.Find(x=>x.name==str);
		if(o==null){
			if(!String.IsNullOrEmpty(str)&&!ignoreWarning)Debug.LogWarning("Object: " + str + " not found!");
			return null;
		}
		Sprite spr=null;
		if(o.gobj.GetComponent<SpriteRenderer>()!=null)spr=o.gobj.GetComponent<SpriteRenderer>().sprite;
		if(o.gobj.GetComponent<UnityEngine.UI.Image>()!=null)spr=o.gobj.GetComponent<UnityEngine.UI.Image>().sprite;
        return spr;
	}
	public GameObject GetVFX(string str){
		GObject o=vfx.Find(x=>x.name==str);
		if(o==null){
			if(!String.IsNullOrEmpty(str))Debug.LogWarning("VFX: " + str + " not found!");
			return null;
		}
		GameObject gobj=o.gobj;
		return gobj;
        //if(SaveSerial.instance.settingsData.particles)return gobj; else return null;
	}
    public Sprite Spr(string str,bool ignoreWarning=false){
		GSprite gs=sprites.Find(x=>x.name==str);
		if(gs==null){
			if(!String.IsNullOrEmpty(name)&&!ignoreWarning)Debug.LogWarning("Sprite: " + str + " not found!");
			return null;
		}
		Sprite s=gs.spr;
        return s;
	}
    public Sprite SprAny(string str){Sprite _spr;
		_spr=Spr(str,true);
		if(_spr==null)_spr=GetObjSpr(str,true);
		if(_spr==null)Debug.LogWarning("Sprite not found in the library of sprites nor for the object by name: "+str);
        return _spr;
	}
    public Sprite SprAnyReverse(string str){Sprite _spr;
		_spr=GetObjSpr(str,true);
		if(_spr==null)_spr=Spr(str,true);
		if(_spr==null)Debug.LogWarning("Sprite not found in the library of sprites nor for the object by name: "+str);
        return _spr;
	}
	public delegate void OnSnapshotCoroutineComplete(Texture2D result);
	float volumeBeforeSnapshot;
	public IEnumerator CaptureScreenshotCoroutine(GameObject prefabToSnapshot,OnSnapshotCoroutineComplete onComplete,int offset=0,float cameraOffset=0f,float waitingTime=0.5f){
		string sceneName="SnapshotScene_"+prefabToSnapshot.name;
		UnityEngine.SceneManagement.Scene existingScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
    
		if(existingScene.IsValid()){
			if(existingScene.isLoaded){
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(existingScene);
				yield return new WaitForSeconds(0.05f);
			}
		}
		UnityEngine.SceneManagement.Scene snapshotScene = UnityEngine.SceneManagement.SceneManager.CreateScene(sceneName);

		string cameraName="Snapshot Camera_"+prefabToSnapshot.name;
		if(GameObject.Find(cameraName)!=null){Destroy(GameObject.Find(cameraName));}
		GameObject snapshotCameraObject = new GameObject(cameraName);
		Camera snapshotCamera = snapshotCameraObject.AddComponent<Camera>();
		snapshotCameraObject.transform.SetParent(null);
		snapshotCamera.CopyFrom(Camera.main);
		snapshotCamera.orthographicSize = Camera.main.orthographicSize+cameraOffset;
		snapshotCamera.aspect = 16f / 9f;
		snapshotCameraObject.transform.position=new Vector3(offset,0,Camera.main.transform.position.z);
		snapshotCameraObject.SetActive(false); // Deactivate it
		UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(snapshotCameraObject, snapshotScene);

		// Create an empty game object to hold the prefabs
		string prefabholderName=prefabToSnapshot.name+"_Holder";
		if(GameObject.Find(prefabholderName)!=null){Destroy(GameObject.Find(prefabholderName));}
		GameObject prefabHolder = new GameObject(prefabholderName);

		// Instantiate the prefab in the new scene
		UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(prefabHolder, snapshotScene);
		prefabHolder.transform.position=new Vector3(offset,0,0);
		GameObject prefabInstance = Instantiate(prefabToSnapshot, prefabHolder.transform);
		prefabInstance.name=prefabToSnapshot.name;
		// UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(prefabInstance, snapshotScene);
		// GameObject worldCanvasInstance;
		if(prefabToSnapshot.name.Contains("Level")){
			GameObject wallsInstance = Instantiate(Get("Walls"),prefabHolder.transform);
			wallsInstance.transform.localPosition=Vector2.zero;
			GameObject bgsInstance = Instantiate(Get("bgs"),prefabHolder.transform);
			bgsInstance.transform.localPosition=new Vector3(0,0,10f);
			Instantiate(CoreSetup.instance.worldCanvasPrefab);
		}

		if(waitingTime>0){
			if(SaveSerial.instance.settingsData.soundVolume!=0){volumeBeforeSnapshot=SaveSerial.instance.settingsData.soundVolume;}
			SaveSerial.instance.settingsData.soundVolume=0f;
			// AudioManager.instance.gameObject.SetActive(false);
			GameManager.instance.gameSpeed=10f;
			// float tempFixedDelta=Time.fixedDeltaTime;
			// Time.fixedDeltaTime=0.2f;
			// Debug.Log(tempFixedDelta+" | "+(float)Time.fixedDeltaTime);
			yield return new WaitForSecondsRealtime(waitingTime);
			GameManager.instance.gameSpeed=1f;
			// Time.fixedDeltaTime=tempFixedDelta;
			SaveSerial.instance.settingsData.soundVolume=volumeBeforeSnapshot;
			// AudioManager.instance.gameObject.SetActive(true);
		}

		// Render the prefab to a texture
		RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
		snapshotCamera.targetTexture = renderTexture;
		snapshotCamera.Render();
		RenderTexture.active = null;
		Texture2D texture=ConvertRenderTextureToTexture2D(renderTexture);
		
		snapshotCamera.targetTexture=null;
		Destroy(renderTexture);
		// if(prefabToSnapshot.name.Contains("Level")){Destroy(WorldCanvas.instance.gameObject);}
		Destroy(prefabHolder);
		UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

		onComplete(texture);
	}
	public static Sprite PrefabSnapshotSpr(GameObject prefabToSnapshot){
		UnityEngine.SceneManagement.Scene existingScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("SnapshotScene");
    
		if(existingScene.IsValid()){
			if(existingScene.isLoaded){
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(existingScene);
			}
		}
		UnityEngine.SceneManagement.Scene snapshotScene = UnityEngine.SceneManagement.SceneManager.CreateScene("SnapshotScene");

		GameObject snapshotCameraObject = new GameObject("Snapshot Camera");
		Camera snapshotCamera = snapshotCameraObject.AddComponent<Camera>();
		snapshotCameraObject.transform.position=Camera.main.transform.position;
		snapshotCamera.CopyFrom(Camera.main);
		snapshotCamera.orthographicSize = Camera.main.orthographicSize+1.25f;
		snapshotCamera.aspect = 16f / 9f;
		snapshotCameraObject.transform.SetParent(null); // Make it a top-level object in the hierarchy
		snapshotCameraObject.SetActive(false); // Deactivate it
		UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(snapshotCameraObject, snapshotScene);

		// Create an empty game object to hold the prefab
		// GameObject prefabHolder = new GameObject("PrefabHolder");

		// Instantiate the prefab in the new scene
		// GameObject prefabInstance = Instantiate(prefabToSnapshot, prefabHolder.transform);
		GameObject prefabInstance = Instantiate(prefabToSnapshot);
		UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(prefabInstance, snapshotScene);

		// Render the prefab to a texture
		RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
		snapshotCamera.targetTexture = renderTexture;
		snapshotCamera.Render();
		RenderTexture.active = null;
		Texture2D texture=ConvertRenderTextureToTexture2D(renderTexture);

		// Create a sprite
		Sprite previewSprite = Sprite.Create(
			texture,
			new Rect(0, 0, renderTexture.width, renderTexture.height),
			new Vector2(0.5f, 0.5f)
		);
		// snapshotCamera.targetTexture=null;
		// Destroy(texture);
		// Destroy(renderTexture);
		// Destroy(prefabHolder);
		// Destroy(prefabInstance);

		return previewSprite;
	}
	public static Sprite TextureToSprite(Texture2D texture){
		return Sprite.Create(
			texture,
			new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f)
		);
	}
	public static Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture){
        if(renderTexture == null){
            return null;
        }

        // Create a new Texture2D and read the render texture data
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        return texture;
    }

	public Material GetMat(string mat,bool instantiate=false){
		GMaterial gm=materials.Find(x=>x.name==mat);
		if(gm==null){
			if(!String.IsNullOrEmpty(mat))Debug.LogWarning("Material: " + mat + " not found!");
			return null;
		}
		Material m=gm.mat;
		if(instantiate){m=Instantiate(m);}
        return m;
	}
	public Material UpdateShaderMatProps(Material mat,ShaderMatProps shaderMatProps,bool isUI=false){	Material _mat=mat;
		if(_mat!=null&&!_mat.shader.name.Contains("AllIn1SpriteShader")){
        	if(!isUI){if(AssetsManager.instance.GetMat("AIOShaderMat")!=null)_mat=Instantiate(AssetsManager.instance.GetMat("AIOShaderMat"));}
			else{if(AssetsManager.instance.GetMat("AIOShaderMat_UI")!=null)_mat=Instantiate(AssetsManager.instance.GetMat("AIOShaderMat_UI"));}
		}
		_mat.SetTexture("_MainTex",shaderMatProps.text);
		_mat.SetFloat("_HsvShift",shaderMatProps.hue*360);
        _mat.SetFloat("_HsvSaturation",shaderMatProps.saturation*2);
        _mat.SetFloat("_HsvBright",shaderMatProps.value*2);
        _mat.SetFloat("_NegativeAmount",shaderMatProps.negative);
        _mat.SetFloat("_PixelateSize",Mathf.Clamp(shaderMatProps.pixelate*512,4,512));
        _mat.SetFloat("_BlurIntensity",shaderMatProps.blur*100);
        _mat.SetFloat("_BlurHD",Convert.ToSingle(shaderMatProps.lowResBlur));
		return _mat;
	}
#endregion

#region//Public functions
	public static bool CaseInsStrCmpr(string str,string toComp){return str.IndexOf(toComp, StringComparison.OrdinalIgnoreCase) >= 0;}
	public static int BoolToInt(bool b){if(b){return 1;}else{return 0;}}
	public static bool IntToBool(int i){if(i==1){return true;}else{return false;}}
	/*public void TransformIntoUIParticle(GameObject go,float mult=0,float dur=-4,bool multShape=false,int type=0){
		if(go.GetComponent<UnityEngine.UI.Extensions.UIParticleSystem>()==null){
			var ps=go.GetComponent<ParticleSystem>();var psMain=ps.main;
			if(mult==0){
				if(psMain.startSize.constantMin<=1){mult=100;}
				if(psMain.startSize.constantMin<=10&&psMain.startSize.constantMin>1){mult=10;}
			}
			if(dur>0){Destroy(go,dur);}
			else if(dur==0){Destroy(go,psMain.startLifetime.constantMax+psMain.duration);}
			else if(dur==-1){Destroy(go,psMain.startLifetime.constantMax+psMain.duration*2);}
			var startSize=psMain.startSize;
			var sizeMin=startSize.constantMin;var sizeMax=startSize.constantMax;if(sizeMin==0){sizeMin=sizeMax;}
			var startSpeed=psMain.startSpeed;
			var speedMin=startSpeed.constantMin;var speedMax=startSpeed.constantMax;if(speedMin==0){speedMin=speedMax;}
			var startColor=new ParticleSystem.MinMaxGradient(psMain.startColor.colorMin,psMain.startColor.colorMax);
			var _color=startColor.colorMin;if(startColor.colorMin.r<0.15f&&startColor.colorMin.g<0.15f&&startColor.colorMin.b<0.15f){_color=startColor.colorMax;}
			if(_color==Color.clear){_color=Color.white;}
			//psMain.startColor=new ParticleSystem.MinMaxGradient(_color,psMain.startColor.colorMax);
			var colorBySpeed=ps.colorBySpeed;
			var colorMin=colorBySpeed.range.x;var colorMax=colorBySpeed.range.y;
			psMain.startSize=new ParticleSystem.MinMaxCurve(sizeMin*mult, sizeMax*mult);
			psMain.startSpeed=new ParticleSystem.MinMaxCurve(speedMin*mult, speedMax*mult);
			//colorBySpeed.range=new Vector2(colorMin*mult, colorMax*mult);
			//colorBySpeed.range=new Vector2(colorMin*30, colorMax*30);
			var psShape=ps.shape;if(multShape){psShape.scale*=mult;}

			var psUI=go.AddComponent<UnityEngine.UI.Extensions.UIParticleSystem>();
			psUI.raycastTarget=false;
			var _tex=ps.GetComponent<Renderer>().material.GetTexture("_MainTex");
			Material mat=new Material(Shader.Find("UI Extensions/Particles/Additive"));
			/*Debug.Log(go.name+" | ColorMin: "+startColor.colorMin);
			Debug.Log(go.name+" | ColorMax: "+startColor.colorMax);
			float H,S,V;Color.RGBToHSV(_color,out H,out S,out V);
			Debug.Log(go.name+" | _color: "+_color + " | HSV("+H+", "+S+", "+V+")");*/
			/*if(_isColorDark(_color)||type==1){
				//Debug.Log(go.name+" - IsDark");
				mat=new Material(Shader.Find("UI Extensions/Particles/Alpha Blended"));
			}
			mat.SetTexture("_MainTex",_tex);
			psUI.material=mat;
		}
	}*/
	public static Quaternion QuatRotateTowards(Vector3 target, Vector3 curPos, float rotModif=90){
		Vector3 vectorToTarget = target - curPos;
		float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - rotModif;
		return Quaternion.AngleAxis(angle, Vector3.forward);
	}
	public static bool _isColorDark(Color color){bool b=false;float H,S,V;Color.RGBToHSV(color,out H,out S,out V);if(V<=0.3f){b=true;}return b;}
	public static void MakeParticleLooping(ParticleSystem ps){var psMain=ps.main;psMain.loop=true;psMain.stopAction=ParticleSystemStopAction.None;}
	public static void ReplaceShaderMat(ref ShaderMatProps shader1, ShaderMatProps shader2){
		shader1.text=shader2.text;
		shader1.hue=shader2.hue;
		shader1.saturation=shader2.saturation;
		shader1.value=shader2.value;
		shader1.negative=shader2.negative;
		shader1.pixelate=shader2.pixelate;
		shader1.blur=shader2.blur;
		shader1.lowResBlur=shader2.lowResBlur;
	}

	//public static float DeltaPercentMinMax(float cur,float max){return (cur-max)/100;}
	public static float Normalize(float val,float min,float max){return (val-min)/(max-min);}
	public static float InvertNormalized(float val){return 1-val;}
	public static float InvertNormalizedAbs(float val){return Mathf.Abs(InvertNormalized(val));}
	public static float InvertNormalizedMin(float val,float min){return InvertNormalized(val)*min;}
	public static float RandomFloat(float min,float max){return (float)System.Math.Round(UnityEngine.Random.Range(min, max), 2);}
	public static bool CheckChance(float chance){return chance>=RandomFloat(0f,100f);}

	

    public static void SetActiveAllChildren(Transform transform, bool value){
        foreach (Transform child in transform){
            child.gameObject.SetActive(value);
 
            SetActiveAllChildren(child, value);
        }
    }
#endregion
}

[System.Serializable]
public class GObject{
	public string name;
	[AssetsOnly]public GameObject gobj;
}
[System.Serializable]
public class GSprite{
	public string name;
	public Sprite spr;
}
[System.Serializable]
public class GTextSprite{
	public string name;
	public Texture2D text;
	public Rect rect;
	public Vector2 pivot;
}
[System.Serializable]
public class GMaterial{
	public string name;
	public Material mat;
}
[System.Serializable]
public class ShaderMatProps{
	//public string name;
	public Texture2D text;
    [Range(0,1)]public float hue=0;
    [Range(0,1)]public float saturation=0.5f;
    [Range(0,1)]public float value=0.5f;
    [Range(0,1)]public float negative=0;
    [Range((4/512),1)]public float pixelate=1;
    [Range(0,1)]public float blur=0;
    public bool lowResBlur=true;
}
[System.Serializable]
public class SimpleAnim{
	public Sprite spr;
	public float delay=0.02f;
}
[System.Serializable]
public class ListOfSimpleAnims{
	public string name;
	public List<SimpleAnim> anim;
}
[System.Serializable]
public class TransformAndPos{
	public Transform trans;
	public Vector2 pos;
}
[System.Serializable]
public class RectTransformAndPos{
	public RectTransform trans;
	public Vector2 pos;
}
[System.Serializable]
public class RectTransformAlign{
	public RectTransform trans;
	public TextAnchor align;
}
/*[System.Serializable]
public class HUDAlignment{
	public RectTransform trans;
	public bool changeAlign;
	[ShowIf("changeAlign")]public AnchorPresets align;
	public Vector2 pos;
	public Vector2 widthAndHeight;
	public float scale=1;
	[ShowIf("@this.scale!=1")]public bool multiplyPosByScale=true;
	public bool changeLayoutGroupAlign;
	[ShowIf("changeLayoutGroupAlign")]public TextAnchor layoutGroupAlign;
}*/