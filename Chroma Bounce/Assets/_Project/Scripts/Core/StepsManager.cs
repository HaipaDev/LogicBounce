using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class StepsManager : MonoBehaviour{      public static StepsManager instance;
    [Header("Current variables")]
    public StepProperties[] currentSteps;
    [DisableInEditorMode]public int currentStepId;
    [Header("References & Defaults")]
    [ChildGameObjectsOnly][SerializeField] GameObject stepsUI;
    [DisableInEditorMode][SerializeField]public static bool StepsUIOpen;
    [SerializeField] float stepsUIAnchoredPosHidden=-220f;
    [ChildGameObjectsOnly][SerializeField] RectTransform stepsUIListContent;
    [SerializeField] CameraSettings openCameraSettings;
    [SerializeField] CameraSettings defaultCameraSettings;

    Vector2 targetUIPos;
    CameraSettings targetCamSettings;
    void Awake(){if(StepsManager.instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        //defaultCameraSettings.pos=Camera.main.transform.position;defaultCameraSettings.size=Camera.main.orthographicSize;
        targetCamSettings=defaultCameraSettings;
        if(currentSteps.Length==0){Debug.LogWarning("No steps!");currentSteps=new StepProperties[1];currentSteps[0]=new StepProperties{stepType=StepPropertiesType.gunPolarity};}
        OpenStepsUI();
    }
    void Update(){
        if(!GameManager.GlobalTimeIsPausedNotStepped){
            if(Input.GetKeyDown(KeyCode.E)){if(!StepsUIOpen){OpenStepsUI(true);}else{if(StepsUIOpen){CloseStepsUI();}}}

            ///Interpolate movement
            float _stepUIPos=325f*Time.unscaledDeltaTime;
            stepsUI.GetComponent<RectTransform>().anchoredPosition=Vector3.MoveTowards(stepsUI.GetComponent<RectTransform>().anchoredPosition,targetUIPos,_stepUIPos);
            
            var cs=targetCamSettings;
            float _stepCamPos=60f*Time.unscaledDeltaTime;
            Camera.main.GetComponent<CamShake>().SetDefaultPos(Vector3.MoveTowards(Camera.main.GetComponent<CamShake>().GetDefaultPos(),new Vector3(cs.pos.x,cs.pos.y,Camera.main.transform.position.z),_stepCamPos));
            float _stepCamSize=5f*Time.unscaledDeltaTime;
            Camera.main.orthographicSize=Vector2.MoveTowards(new Vector2(0,Camera.main.orthographicSize),new Vector2(0,cs.size),_stepCamSize).y;

            if(Input.GetKeyDown(KeyCode.Space)&&!startedSteps){StartSteps();}
        }
    }
    bool startedSteps;
    public void StartSteps(){StartCoroutine(StartStepsI());}
    IEnumerator StartStepsI(){
        Debug.Log("StartStepsI()");
        startedSteps=true;
        CloseStepsUI();
        yield return new WaitForSeconds(1.5f);//Wait for end of animation
        if(stepsCoroutine==null){stepsCoroutine=StartCoroutine(ExecuteStepsI());}
    }
    public void StopSteps(){if(stepsCoroutine!=null){StopCoroutine(stepsCoroutine);stepsCoroutine=null;}currentStepId=0;startedSteps=false;}
    void ReexecuteSteps(){Debug.Log("ReexecuteSteps()");if(stepsCoroutine!=null){stepsCoroutine=null;stepsCoroutine=StartCoroutine(ExecuteStepsI());Debug.Log("ReexecutingSteps..");}}
    Coroutine stepsCoroutine;
    IEnumerator ExecuteStepsI(){
        Debug.Log("Executing Step: "+currentStepId);
        if(currentSteps.Length==0){Debug.LogWarning("No steps!");currentSteps=new StepProperties[1];currentSteps[0]=new StepProperties{stepType=StepPropertiesType.gunPolarity};}
        if(currentStepId<currentSteps.Length){
            var _s=currentSteps[currentStepId];
            switch(_s.stepType){
                case StepPropertiesType.delay:
                    yield return new WaitForSeconds(_s.delay);
                break;
                case StepPropertiesType.gunPolarity:
                    Player.instance.SetPolarity(_s.gunPolarity);
                break;
                case StepPropertiesType.gunRotation:
                    Player.instance.SetGunRotation(_s.gunRotation);
                break;
                case StepPropertiesType.gunShoot:
                    Player.instance.ShootBullet();
                break;
                case StepPropertiesType.laserPos:
                    LevelMapManager.instance.laserListSorted[_s.objectId].transform.position=_s.laserPos;
                break;
                case StepPropertiesType.mirrorPos:
                    LevelMapManager.instance.mirrorListSorted[_s.objectId].transform.position=_s.mirrorPos;
                break;
            }
            currentStepId++;
            if(currentStepId<currentSteps.Length){yield return new WaitForSeconds(0.01f);ReexecuteSteps();}else{currentStepId=0;Debug.Log("All steps done!");}
        }
    }

    void OpenStepsUI(bool reset=false,bool force=false){
        if(!StepsUIOpen){
            StopSteps();
            if(currentStepId==0||force){
                targetUIPos=Vector2.zero;
                stepsUI.SetActive(true);
                SetCameraSettings(openCameraSettings);
                StepsUIOpen=true;
                if(reset){LevelMapManager.instance.CallRestart();}
            }
        }
    }
    void CloseStepsUI(){
        if(StepsUIOpen){
            targetUIPos=new Vector2(0,stepsUIAnchoredPosHidden);
            SetCameraSettings(defaultCameraSettings);
            StepsUIOpen=false;
            AudioManager.instance.Play("StartSteps");
        }
    }
    void ForceCloseStepsUI(){
        stepsUI.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,stepsUIAnchoredPosHidden);targetUIPos=new Vector2(0,stepsUIAnchoredPosHidden);
        stepsUI.SetActive(false);
        SetCameraSettings(defaultCameraSettings);
        StepsUIOpen=false;
    }
    void SetCameraSettings(CameraSettings cs){targetCamSettings=cs;}
}
[System.Serializable]
public class CameraSettings{
    public Vector2 pos;
    public float size=5;
}
[System.Serializable]
public class StepProperties{
    public StepPropertiesType stepType;
    [ShowIf("@this.stepType==StepPropertiesType.delay")]public float delay=0.5f;
    [ShowIf("@this.stepType==StepPropertiesType.gunPolarity")]public bool gunPolarity=true;
    [ShowIf("@this.stepType==StepPropertiesType.gunRotation")][Range(0,360)]public float gunRotation;
    [ShowIf("@this.stepType==StepPropertiesType.laserPos||this.stepType==StepPropertiesType.mirrorPos")]
    public int objectId;
    [ShowIf("@this.stepType==StepPropertiesType.laserPos")]public Vector2 laserPos;
    [ShowIf("@this.stepType==StepPropertiesType.mirrorPos")]public Vector2 mirrorPos;
}
public enum StepPropertiesType{delay,gunPolarity,gunRotation,gunShoot,laserPos,mirrorPos}