using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class StepsManager : MonoBehaviour{      public static StepsManager instance;
    [Header("Current variables")]
    public List<StepProperties> currentSteps;
    [DisableInEditorMode]public int currentStepId;
    [DisableInEditorMode]public Button selectedStep;
    [DisableInEditorMode]public int currentStepsEnergyUsed;
    [Header("References & Defaults")]
    [ChildGameObjectsOnly][SerializeField] GameObject stepsUI;
    [ChildGameObjectsOnly][SerializeField] RectTransform stepsUIListContent;
    [ChildGameObjectsOnly][SerializeField] GameObject otherButtons;
    [ChildGameObjectsOnly][SerializeField] GameObject addStepsUI;
    [ChildGameObjectsOnly][SerializeField] RectTransform addStepsUIListContent;
    [ChildGameObjectsOnly][SerializeField] RectTransform startingElementsTransform;
    [AssetsOnly][SerializeField] GameObject[] stepsUIPrefabs;
    [DisableInEditorMode][SerializeField]public static bool StepsUIOpen;
    [SerializeField] float stepsUIAnchoredPosHidden=-410f;
    [SerializeField] float addStepsUIAnchorHeightShown=800f;
    [SerializeField] CameraSettings openCameraSettings;
    [SerializeField] CameraSettings defaultCameraSettings;

    Vector2 targetUIPos;
    float targetAddStepsUIHeight;
    CameraSettings targetCamSettings;
    void Awake(){if(StepsManager.instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        //defaultCameraSettings.pos=Camera.main.transform.position;defaultCameraSettings.size=Camera.main.orthographicSize;
        targetCamSettings=openCameraSettings;
        if(currentSteps.Count==0){Debug.LogWarning("No starting steps!");AddStep((int)StepPropertiesType.gunShoot,true);RefreshStartingElements();}
        OpenStepsUI();addStepsUI.SetActive(false);
        //RefreshStartingElements();
        RepopulateUIFromSteps();
        //ClearAllSteps();
    }
    void Update(){
        if(!GameManager.GlobalTimeIsPausedNotStepped){
            if(!VictoryCanvas.Won){
                if(Input.GetKeyDown(KeyCode.E)){if(!StepsUIOpen){OpenStepsUI(true);}else{CloseStepsUI();}}
                if(Input.GetKeyDown(KeyCode.Space)){UIInputSystem.instance.currentSelected=null;StartSteps();}
            }

            ///Interpolate movement
            float _stepUIPos=325f*Time.unscaledDeltaTime;
            stepsUI.GetComponent<RectTransform>().anchoredPosition=Vector2.MoveTowards(stepsUI.GetComponent<RectTransform>().anchoredPosition,targetUIPos,_stepUIPos);
            float _addStepsUIHeight=1500f*Time.unscaledDeltaTime;
            addStepsUI.GetComponent<RectTransform>().sizeDelta=Vector2.MoveTowards(addStepsUI.GetComponent<RectTransform>().sizeDelta,new Vector2(addStepsUI.GetComponent<RectTransform>().sizeDelta.x,targetAddStepsUIHeight),_addStepsUIHeight);
            
            var cs=targetCamSettings;
            float _stepCamPos=60f*Time.unscaledDeltaTime;
            Camera.main.GetComponent<CamShake>().SetDefaultPos(Vector3.MoveTowards(Camera.main.GetComponent<CamShake>().GetDefaultPos(),new Vector3(cs.pos.x,cs.pos.y,Camera.main.transform.position.z),_stepCamPos));
            float _stepCamSize=5f*Time.unscaledDeltaTime;
            Camera.main.orthographicSize=Vector2.MoveTowards(new Vector2(0,Camera.main.orthographicSize),new Vector2(0,cs.size),_stepCamSize).y;

            if(allStepsDone&&reopenStepsUIDelay>0&&FindObjectsOfType<Bullet>().Length==0)reopenStepsUIDelay-=Time.unscaledDeltaTime;
            if(allStepsDone&&reopenStepsUIDelay<=0){OpenStepsUI(true);}

            /*ChangeValuesWithArrows();*/
            //if(selectedStep==null)ResetAfterPreviewingSteps();
        }
    }
    void ChangeValuesWithArrows(){
        if(selectedStep!=null&&(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow))){var _add=Input.GetKey(KeyCode.UpArrow);
            var _txtC=selectedStep.GetComponentInChildren<TMPro.TMP_InputField>();var _txt=_txtC.text;
            var _limits=selectedStep.GetComponentInChildren<LimitInputFieldNum>();
            var _val=0f;bool _isFloat=(_txt.Contains(",")||_txt.Contains("."));float _dif=1f;
            float valMin=_limits.GetLimits().x,valMax=_limits.GetLimits().y;
            if(_isFloat){_dif=0.1f;valMin=_limits.GetLimitsFloat().x;valMax=_limits.GetLimitsFloat().y;}
            if(_isFloat&&float.TryParse(_txt,out float vfloat)){_val=vfloat;}
            if(!_isFloat&&int.TryParse(_txt,out int vint)){_val=vint;}

            if(!_add){_val=Mathf.Clamp(_val+_dif,valMin,valMax);}
            else{_val=Mathf.Clamp(_val-_dif,valMin,valMax);}

            if(!_isFloat){_txtC.SetTextWithoutNotify(_val.ToString());SelectStep(selectedStep,true);}
            else{_txtC.SetTextWithoutNotify(_val.ToString("F2").Replace('.',','));SelectStep(selectedStep,true);}
        }
    }
    public void RefreshStartingElements(){
        var l=LevelMapManager.instance.GetCurrentLevelMap();
        var _gun=startingElementsTransform.GetChild(1);
        var _bounces=startingElementsTransform.GetChild(2);
        var _energy=startingElementsTransform.GetChild(3);

        string _gunRotationTxt=Mathf.RoundToInt(l.defaultGunRotation).ToString()+"°";
        foreach(Transform t in _gun){
            if(t!=transform){
                if(t.GetComponent<Image>()!=null)t.GetComponent<Image>().sprite=Player.instance.GetGunSpr(l.startingChargePositive);
                if(t.GetComponent<TMPro.TextMeshProUGUI>()!=null)t.GetComponent<TMPro.TextMeshProUGUI>().text=_gunRotationTxt;
            }
        }

        if(_bounces.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_bounces.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=l.bulletBounceLimit.ToString();}

        string _energyTxt=(currentStepsEnergyUsed.ToString()+" / "+l.stepEnergy.ToString());
        if(_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=_energyTxt;}
        if(currentEnergyLeft()<=0){_energy.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareRed");}
        else{_energy.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquare");}
    }
    bool startedSteps,allStepsDone;float reopenStepsUIDelay;
    public void StartSteps(){
        if(currentSteps.Count>0&&!VictoryCanvas.Won){
            selectedStep=null;
            if(!startedSteps&&!GameManager.GlobalTimeIsPausedNotStepped){StartCoroutine(StartStepsI());}
        }
    }
    IEnumerator StartStepsI(){
        if(!GameManager.GlobalTimeIsPausedNotStepped&&!VictoryCanvas.Won){
            Debug.Log("StartStepsI()");
            startedSteps=true;
            allStepsDone=false;
            UIInputSystem.instance.currentSelected=null;
            CloseStepsUI();
            yield return new WaitForSeconds(1.5f);//Wait for end of animation
            if(stepsCoroutine==null){stepsCoroutine=StartCoroutine(ExecuteStepsI());}
        }else{yield return null;}
    }
    public void StopSteps(){if(stepsCoroutine!=null){StopCoroutine(stepsCoroutine);stepsCoroutine=null;}currentStepId=0;startedSteps=false;}
    void ReexecuteSteps(){Debug.Log("ReexecuteSteps()");if(stepsCoroutine!=null){stepsCoroutine=null;stepsCoroutine=StartCoroutine(ExecuteStepsI());Debug.Log("ReexecutingSteps..");}}
    Coroutine stepsCoroutine;//,singleStepCoroutine;
    IEnumerator ExecuteStepsI(){
        if(startedSteps){
            Debug.Log("Executing Step: "+currentStepId);
            if(currentSteps.Count==0){Debug.LogWarning("No steps!");}
            if(currentStepId<currentSteps.Count&&startedSteps){
                ExecuteSingleStep(currentStepId);
                currentStepId++;
                if(currentStepId<currentSteps.Count){yield return new WaitForSeconds(0.01f);if(startedSteps)ReexecuteSteps();}
                else{currentStepId=0;allStepsDone=true;reopenStepsUIDelay=2f;Debug.Log("All steps done!");}
            }
        }
    }
    void ExecuteSingleStep(int id){StartCoroutine(ExecuteSingleStepI(id));}
    IEnumerator ExecuteSingleStepI(int id){
        var _s=currentSteps[id];
        switch(_s.stepType){
            case StepPropertiesType.delay:
                yield return new WaitForSeconds(_s.delay);
            break;
            case StepPropertiesType.gunShoot:
                Player.instance.ShootBullet();
            break;
            case StepPropertiesType.gunPolarity:
                //Player.instance.SetPolarity(_s.gunPolarity);
                Player.instance.SwitchPolarity();
            break;
            case StepPropertiesType.gunRotation:
                Player.instance.SetGunRotation(_s.gunRotation);//Reverse rotation
            break;
            case StepPropertiesType.mirrorPos:
                LevelMapManager.instance.mirrorListSorted[_s.objectId].transform.position=_s.mirrorPos;
            break;
            case StepPropertiesType.laserPos:
                LevelMapManager.instance.laserListSorted[_s.objectId].transform.position=_s.laserPos;
            break;
        }
    }
    void ResetAfterPreviewingSteps(){
        var l=LevelMapManager.instance.GetCurrentLevelMap();
        Player.instance.SetGunRotation(l.defaultGunRotation);
    }

    public void OpenStepsUI(bool reset=false,bool force=false){
        if(!StepsUIOpen){
            StopSteps();
            if(currentStepId==0||force){
                targetUIPos=Vector2.zero;
                stepsUI.SetActive(true);
                SetCameraSettings(openCameraSettings);
                StepsUIOpen=true;
                otherButtons.SetActive(true);
                allStepsDone=false;
                if(reset){LevelMapManager.instance.CallRestart();}
            }
        }
    }
    public void CloseStepsUI(){
        if(StepsUIOpen){
            targetUIPos=new Vector2(0,stepsUIAnchoredPosHidden);
            SetCameraSettings(defaultCameraSettings);
            StepsUIOpen=false;
            targetAddStepsUIHeight=0;
            //addStepsUI.SetActive(false);
            otherButtons.SetActive(false);
            AudioManager.instance.Play("StartSteps");
        }
    }
    void ForceCloseStepsUI(){
        stepsUI.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,stepsUIAnchoredPosHidden);targetUIPos=new Vector2(0,stepsUIAnchoredPosHidden);
        targetAddStepsUIHeight=0;addStepsUI.SetActive(false);
        stepsUI.SetActive(false);
        SetCameraSettings(defaultCameraSettings);
        StepsUIOpen=false;
    }
    public void OpenAddStepsUI(){
        if(StepsUIOpen){
            if(!addStepsUI.activeSelf||targetAddStepsUIHeight==0){addStepsUI.SetActive(true);targetAddStepsUIHeight=addStepsUIAnchorHeightShown;}
            else{targetAddStepsUIHeight=0;}
        }
        SetAllowedSteps();
    }
    public void AddStepButton(int id){AddStep(id,false);}
    public void AddStep(int id,bool quiet=false){
        if(currentEnergyLeft()>=_stepCostForTypeCurrentLvl((StepPropertiesType)id)){
            //currentSteps.Add(new StepProperties(){stepType=(StepPropertiesType)id});
            if((id!=(int)StepPropertiesType.mirrorPos&&id!=(int)StepPropertiesType.laserPos)||
            ((id==(int)StepPropertiesType.mirrorPos&&FindObjectsOfType<Mirror>().Length>0)||(id==(int)StepPropertiesType.laserPos&&FindObjectsOfType<Laser>().Length>0))){
                GameObject gos=Instantiate(stepsUIPrefabs[id],stepsUIListContent);
                SetSelectableStep(gos);
                RepopulateStepsFromUI();
                SetAllowedSteps(0.15f);
                if(!quiet)AudioManager.instance.Play("StepAdd");
            }
        }else{if(!quiet)AudioManager.instance.Play("Deny");}
    }
    public void RepopulateStepsFromUI(){StartCoroutine(RepopulateStepsFromUI_I());}
    IEnumerator RepopulateStepsFromUI_I(){
        currentSteps.Clear();currentSteps=new List<StepProperties>();currentStepsEnergyUsed=0;RefreshStartingElements();
        yield return new WaitForSecondsRealtime(0.1f);
        foreach(StepUIPrefab s in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){
            int _stepCost=_stepCostForTypeCurrentLvl(s.stepProperties.stepType);
            currentSteps.Add(s.stepProperties);currentStepsEnergyUsed+=_stepCost;
            RefreshStartingElements();
        }
        SetAllowedSteps();
    }
    public void RepopulateUIFromSteps(){
        ClearUIStepList();
        foreach(StepProperties s in currentSteps){
            GameObject go=stepsUIPrefabs[(int)s.stepType];
            GameObject gos=Instantiate(go,stepsUIListContent);
            SetSelectableStep(gos);
            go.GetComponent<StepUIPrefab>().SetProperties(s);
        }
        RefreshStartingElements();
        /*for(var i=0;i<LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes.Count;i++){
            StepPropertiesType st=LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes[i];
            addStepsUIListContent.GetChild(i).gameObject.SetActive(true);
        }*/
    }
    public void ClearUIStepList(){
        if(stepsUIListContent.childCount>0)for(var i=stepsUIListContent.childCount-1;i>=0;i--){Destroy(stepsUIListContent.GetChild(i).gameObject);}
    }
    public void ClearAllStepsButton(){if(currentSteps.Count>0)ClearAllSteps(false);}
    public void ClearAllSteps(bool quiet=true){
        ClearUIStepList();
        currentSteps=new List<StepProperties>();
        currentStepsEnergyUsed=0;RefreshStartingElements();SetAllowedSteps();
        if(!quiet){AudioManager.instance.Play("StepRemove");}
    }
    void SetSelectableStep(GameObject gos){
        var sui=gos.GetComponent<StepUIPrefab>();
        if(sui.stepProperties.stepType==StepPropertiesType.gunRotation){
            Button bt;
            if(gos.GetComponent<Button>()!=null){bt=gos.GetComponent<Button>();}
            else{bt=gos.AddComponent<Button>();}
            bt.onClick.AddListener(delegate { StepsManager.instance.SelectStep(bt);});
        }
    }
    public void SelectStep(Button bt,bool forceUpdate=false){
        if(bt!=null){
            var sui=bt.GetComponent<StepUIPrefab>();
            if(selectedStep!=bt||selectedStep==null||forceUpdate){
                selectedStep=bt;
                sui.SetSelectedSpr();
                ExecuteSingleStep(currentSteps.FindIndex(x=>x==sui.stepProperties));
                Debug.Log("Selecting Step");
            }else if(selectedStep==bt){
                selectedStep=null;
                sui.SetDefaultSpr();
                ResetAfterPreviewingSteps();
                Debug.Log("UnSelecting Steps");
            }
        }else{
            selectedStep=null;ResetAfterPreviewingSteps();
            foreach(StepUIPrefab sui in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){sui.SetDefaultSpr();}
            Debug.Log("UnSelecting Steps");
        }
    }
    public void SetAllowedSteps(float delay=0f){StartCoroutine(SetAllowedStepsI(delay));}
    IEnumerator SetAllowedStepsI(float delay){
        yield return new WaitForSecondsRealtime(delay);
        for(var i=0;i<addStepsUIListContent.childCount;i++){
            //LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes.Length>0;
            if(addStepsUIListContent.childCount>i){
                addStepsUIListContent.GetChild(i).gameObject.SetActive(LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes[(StepPropertiesType)i]);
                int _stepCost=_stepCostForTypeCurrentLvl((StepPropertiesType)i);
                var _energy=addStepsUIListContent.transform.GetChild(i).Find("Energy").gameObject;
                if(_stepCost!=0){_energy.SetActive(true);_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=_stepCost.ToString();}
                else{_energy.SetActive(false);}
                if(currentEnergyLeft()<_stepCostForTypeCurrentLvl((StepPropertiesType)i)){addStepsUIListContent.GetChild(i).GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareRed");}
                else{addStepsUIListContent.GetChild(i).GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareBlue");}
            }else{Debug.LogWarning("Children of addStepsUIListContent not setup properly!");}
        }
    }
    public int currentEnergyLeft(){return LevelMapManager.instance.GetCurrentLevelMap().stepEnergy-currentStepsEnergyUsed;}
    public void SumUpEnergy(){
        currentStepsEnergyUsed=0;
        foreach(StepProperties s in currentSteps){
            int _stepCost=0;
            currentStepsEnergyUsed+=_stepCost;
            RefreshStartingElements();
        }
    }
    public int _stepCostForTypeCurrentLvl(StepPropertiesType stepType){
        int _stepCost=0;
        if(LevelMapManager.instance.GetCurrentLevelMap().stepTypesCosts.ContainsKey(stepType)){
            _stepCost=LevelMapManager.instance.GetCurrentLevelMap().stepTypesCosts[stepType];
        }
        return _stepCost;
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
    [ShowIf("@this.stepType==StepPropertiesType.delay")]public float delay=0.1f;
    [HideInInspector][ShowIf(""/*"@this.stepType==StepPropertiesType.gunPolarity"*/)]public bool gunPolarity=true;
    [ShowIf("@this.stepType==StepPropertiesType.gunRotation")][Range(0,360)]public float gunRotation;
    [ShowIf("@this.stepType==StepPropertiesType.laserPos||this.stepType==StepPropertiesType.mirrorPos")]
    public int objectId;
    [ShowIf("@this.stepType==StepPropertiesType.mirrorPos")]public Vector2 mirrorPos;
    [ShowIf("@this.stepType==StepPropertiesType.laserPos")]public Vector2 laserPos;
}
public enum StepPropertiesType{delay,gunShoot,gunPolarity,gunRotation,mirrorPos,laserPos}
//public class StepCosts{Dictionary<StepPropertiesType,int>();}