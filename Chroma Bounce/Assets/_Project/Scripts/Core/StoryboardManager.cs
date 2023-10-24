using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

// Part of this script responsible for having a typewriter effect for UI was Prepared by Nick Hwang (https://www.youtube.com/nickhwang), modified by me

public class StoryboardManager : MonoBehaviour{     public static StoryboardManager instance;
    public static bool IsOpen;
    [ChildGameObjectsOnly][SerializeField]GameObject blurChild;
    [ChildGameObjectsOnly][SerializeField]GameObject storyboardUI;
    [SerializeField] float storyboardUIAnchoredPosHidden=-1925f;
    [ChildGameObjectsOnly][SerializeField]GameObject sbTextParent;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sbText;
    [ChildGameObjectsOnly][SerializeField]GameObject sbNarratorParent;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sbNarratorText;
    [DisableInEditorMode]public int currentTextId;
    [SerializeField] float delayBeforeStart = 0f;
	[SerializeField] float timeBtwCharsDef = 0.1f;
    [DisableInEditorMode][SerializeField]float timeBtwChars = 0.1f;
	[SerializeField] string leadingChar = "";
	[SerializeField] bool leadingCharBeforeDelay = false;
	[SerializeField] string typewriterSoundAsset = "Typing";

    [DisableInEditorMode]public bool finishedTyping;
    Vector2 targetUIPos=Vector2.zero;
    string writer,writerFinal;TextMeshProUGUI textComponent;
    void Start(){
        instance=this;
        targetUIPos=Vector2.zero;storyboardUI.GetComponent<RectTransform>().anchoredPosition=targetUIPos;
        if(LevelMapManager.instance.GetCurrentLevelMap().storyboardText==null||(LevelMapManager.instance.GetCurrentLevelMap().storyboardText!=null&&LevelMapManager.instance.GetCurrentLevelMap().storyboardText.Count==0)){
            storyboardUI.SetActive(false);StepsManager.instance.OpenStepsUI();
        }else{StepsManager.instance.CloseStepsUI(true);}
        sbTextParent.gameObject.SetActive(false);sbNarratorParent.SetActive(false);
        textComponent=sbText;
        if(!LevelMapManager.instance._testing&&LevelMapManager.instance.GetCurrentLevelMap().storyboardText.Count>0){Talk();}else{Close();}
    }
    void Update(){
        ///Interpolate movement
        float _stepUIPos=2120f*Time.unscaledDeltaTime;
        storyboardUI.GetComponent<RectTransform>().anchoredPosition=Vector2.MoveTowards(storyboardUI.GetComponent<RectTransform>().anchoredPosition,targetUIPos,_stepUIPos);
        if(storyboardUI.GetComponent<RectTransform>().anchoredPosition.x<storyboardUIAnchoredPosHidden+30&&finishedTyping){storyboardUI.SetActive(false);}

        if(textComponent!=null){
            finishedTyping=(textComponent.text==writerFinal);
        }else{
            if(sbText!=null){
                textComponent=sbText;
            }
        }
    }
    void Talk(){
        IsOpen=true;
        storyboardUI.SetActive(true);
        targetUIPos=Vector2.zero;
        if(LevelMapManager.instance.GetCurrentLevelMap().storyboardText.Count>currentTextId){
            var lsb=LevelMapManager.instance.GetCurrentLevelMap().storyboardText[currentTextId];
            SetTypewriterWithType(lsb.text,lsb.storyboardTextType,lsb.speed,typewriterSoundAsset);
        }else{Debug.LogWarning("No storyboard by id: "+" for level "+LevelMapManager.instance.levelCurrent);}
    }
    public void TalkManual(string text,StoryboardTextType sbTextType,float speed=0.1f,string typewriterSoundAssetOverwrite="Typing"){
        IsOpen=true;
        storyboardUI.SetActive(true);
        targetUIPos=Vector2.zero;
        currentTextId=100;
        SetTypewriterWithType(text,sbTextType,speed,typewriterSoundAssetOverwrite);
    }
    public void Close(){
        if(typeWriterCoroutine!=null){StopCoroutine(typeWriterCoroutine);typeWriterCoroutine=null;}
        IsOpen=false;
        //storyboardUI.SetActive(false);
        targetUIPos=new Vector2(storyboardUIAnchoredPosHidden,0);
        if(!VictoryCanvas.Won)StepsManager.instance.OpenStepsUI(true,false);
    }
    public void Skip(){
        if(GameManager.instance._webglFullscreenRequested){
            if(finishedTyping){
                if(LevelMapManager.instance.GetCurrentLevelMap().storyboardText.Count>currentTextId+1){
                    currentTextId++;
                    Talk();
                }else{Close();}
            }else{textComponent.text=writerFinal;if(typeWriterCoroutine!=null){StopCoroutine(typeWriterCoroutine);typeWriterCoroutine=null;}}//Debug.Log("Stopping typewriter.");}}
        }
    }
    Coroutine typeWriterCoroutine;
    public void SetTypewriterWithType(string text,StoryboardTextType sbTextType,float speed=0.1f,string typewriterSoundAssetOverwrite="Typing"){
        textComponent=sbText;sbTextParent.gameObject.SetActive(true);sbNarratorParent.SetActive(false);
        if(sbTextType==StoryboardTextType.narrator){
            textComponent=sbNarratorText;sbNarratorParent.SetActive(true);sbTextParent.gameObject.SetActive(false);
            typewriterSoundAssetOverwrite="Typing2";
        }
        SetTypewriter(text,textComponent,speed,typewriterSoundAssetOverwrite);
    }
    public void SetTypewriter(string text,TextMeshProUGUI textComponent,float speed=0.1f,string typewriterSoundAssetOverwrite="Typing"){
        if(typeWriterCoroutine!=null){StopCoroutine(typeWriterCoroutine);typeWriterCoroutine=null;}//Debug.Log("Stopping typewriter.");}
        writerFinal=text;
        writer=text;
        textComponent.text="";
        timeBtwChars=timeBtwCharsDef;
        if(!string.IsNullOrEmpty(typewriterSoundAssetOverwrite)){typewriterSoundAsset=typewriterSoundAssetOverwrite;}
        if(speed!=0.1f){timeBtwChars=speed;}
        //timeBtwChars=timeBtwCharsDef/speed;

        if(typeWriterCoroutine==null)typeWriterCoroutine=StartCoroutine(TypeWriterTMP(textComponent));
        Debug.Log("Setting typewriter...");
    }
	IEnumerator TypeWriterTMP(TextMeshProUGUI textComponent){
        textComponent.text = leadingCharBeforeDelay ? leadingChar : "";

        //Debug.Log("Starting typing, waiting for delay");
        yield return new WaitForSeconds(delayBeforeStart);
        //Debug.Log("Starting typing.");

		foreach(char c in writer){
			if(textComponent.text.Length>0){
				textComponent.text=textComponent.text.Substring(0, textComponent.text.Length - leadingChar.Length);
			}
			textComponent.text += c;
			textComponent.text += leadingChar;
			yield return new WaitForSeconds(timeBtwChars);
            if(!string.IsNullOrEmpty(typewriterSoundAsset))AudioManager.instance.Play(typewriterSoundAsset);
            //Debug.Log("Typing: "+c);
		}

		if(leadingChar!=""){
			textComponent.text=textComponent.text.Substring(0, textComponent.text.Length - leadingChar.Length);
		}
	}
}
[System.Serializable]
public class StoryboardText{
    public StoryboardTextType storyboardTextType;
    public float speed=0.07f;
    [MultiLineProperty(10)]public string text;
}
public enum StoryboardTextType{character,narrator}