using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class StepUIPrefab : MonoBehaviour{
    public StepProperties stepProperties;
    [SerializeField]Sprite defaultSpr;
    [SerializeField]Sprite selectedSpr;
    [ShowIf("@this.stepProperties.stepType==StepPropertiesType.mirrorPos||this.stepProperties.stepType==StepPropertiesType.laserPos")]
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI objectIdTxt;
    [ChildGameObjectsOnly][SerializeField]GameObject energyObj;

    void Start(){
        SetProperties(stepProperties);
    }
    public void SetProperties(StepProperties sp){
        stepProperties=sp;
        if(stepProperties.stepType==StepPropertiesType.delay){
            GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(stepProperties.delay.ToString("F2").Replace('.',','));
        }
        if(stepProperties.stepType==StepPropertiesType.gunRotation){
            GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(stepProperties.gunRotation.ToString());
        }
        if(stepProperties.stepType==StepPropertiesType.mirrorPos){
            for(var i=0;i<GetComponentsInChildren<TMP_InputField>().Length;i++){
                var _val=stepProperties.mirrorPos.x;if(i==1){_val=stepProperties.mirrorPos.y;}
                GetComponentsInChildren<TMP_InputField>()[i].SetTextWithoutNotify(_val.ToString());//.ToString("F2").Replace('.',','));
            }
        }
        if(energyObj!=null){
            int _stepCost=StepsManager.instance._stepCostForTypeCurrentLvl(stepProperties.stepType);
            if(_stepCost!=0){energyObj.SetActive(true);energyObj.GetComponentInChildren<TextMeshProUGUI>().text=_stepCost.ToString();}
            else{energyObj.SetActive(false);}
        }else{Debug.LogWarning("energyObj not assigned!");}

    }
    public void RemoveStep(){
        AudioManager.instance.Play("StepRemove");
        StepsManager.instance.RepopulateStepsFromUI();//At 0.1f seconds delay
        Destroy(gameObject);
    }
    public void SetSelectedSpr(){GetComponent<Image>().sprite=selectedSpr;}
    public void SetDefaultSpr(){GetComponent<Image>().sprite=defaultSpr;}
    public void SelectButton(){StepsManager.instance.SelectStep(GetComponent<Button>(),true);}
    
    public void SetDelay(string val){StartCoroutine(SetDelayI(val));}
    IEnumerator SetDelayI(string val){
        yield return new WaitForSecondsRealtime(0.05f);
        Debug.Log("Setting Delay");
        var i=float.Parse(val);
        stepProperties.delay=i;
        SelectButton();
    }
    public void SetGunRotation(string val){StartCoroutine(SetGunRotationI(val));}
    IEnumerator SetGunRotationI(string val){
        yield return new WaitForSecondsRealtime(0.05f);
        Debug.Log("Setting Gun rotation");
        var i=int.Parse(val);
        stepProperties.gunRotation=i;
        SelectButton();
    }
    public void SetObjectId(string val){StartCoroutine(SetObjectId_I(val));}
    IEnumerator SetObjectId_I(string val){
        yield return new WaitForSecondsRealtime(0.05f);
        Debug.Log("Setting Object ID");
        var i=int.Parse(val);
        stepProperties.objectId=i;
        objectIdTxt.text="ID: "+i.ToString();
        SelectButton();
    }
    public void SetMirrorPosX(string val){StartCoroutine(SetMirrorPosX_I(val));}
    IEnumerator SetMirrorPosX_I(string val){
        yield return new WaitForSecondsRealtime(0.05f);
        Debug.Log("Setting MirrorPos X");
        var i=int.Parse(val);
        stepProperties.mirrorPos=new Vector2(i,stepProperties.mirrorPos.y/10);
        SelectButton();
    }
    public void SetMirrorPosY(string val){StartCoroutine(SetMirrorPosY_I(val));}
    IEnumerator SetMirrorPosY_I(string val){
        yield return new WaitForSecondsRealtime(0.05f);
        Debug.Log("Setting MirrorPos Y");
        var i=int.Parse(val);
        stepProperties.mirrorPos=new Vector2(stepProperties.mirrorPos.x,i/10);
        SelectButton();
    }
}
