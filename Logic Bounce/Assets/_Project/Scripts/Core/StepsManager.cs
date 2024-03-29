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
    [ChildGameObjectsOnly][SerializeField] GameObject pauseButton;
    [ChildGameObjectsOnly][SerializeField] GameObject topButtons;
    [ChildGameObjectsOnly][SerializeField] GameObject hideButton;
    [ChildGameObjectsOnly][SerializeField] GameObject showButton;
    [ChildGameObjectsOnly][SerializeField] GameObject addStepsUI;
    [ChildGameObjectsOnly][SerializeField] RectTransform addStepsUIListContent;
    [ChildGameObjectsOnly][SerializeField] RectTransform startingElementsTransform;
    [ChildGameObjectsOnly][SerializeField] GameObject executionBorder;
    [AssetsOnly][SerializeField] GameObject[] stepsUIPrefabs;
    [DisableInEditorMode][SerializeField]public static bool StepsUIOpen;
    [SerializeField] float stepsUIAnchoredPosHidden=-410f;
    [SerializeField] float addStepsUIAnchorHeightShown=800f;
    [SerializeField] CameraSettings openCameraSettings;
    [SerializeField] CameraSettings defaultCameraSettings;

    Vector2 _hiddenUIPos;
    Vector2 targetUIPos;
    Vector2 _topButtonsTargetPos;
    Vector2 _topButtonsOriginPos;
    Vector2 _topButtonsHiddenPos;
    Vector2 _pauseButtonTargetPos;
    Vector2 _pauseButtonOriginPos;
    Vector2 _pauseButtonHiddenPos;
    float targetAddStepsUIHeight;
    CameraSettings targetCamSettings;
    void Awake(){if(StepsManager.instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        _hiddenUIPos=new Vector2(0,stepsUIAnchoredPosHidden);
        targetUIPos=Vector2.zero;

        _topButtonsOriginPos=topButtons.GetComponent<RectTransform>().anchoredPosition;
        _topButtonsHiddenPos=topButtons.GetComponent<RectTransform>().anchoredPosition+new Vector2(0,-170);
        _topButtonsTargetPos=_topButtonsOriginPos;

        _pauseButtonOriginPos=pauseButton.GetComponent<RectTransform>().anchoredPosition;
        _pauseButtonHiddenPos=pauseButton.GetComponent<RectTransform>().anchoredPosition+new Vector2(-270,0);//new Vector2(0,200);
        _pauseButtonTargetPos=_pauseButtonOriginPos;

        targetCamSettings=defaultCameraSettings;
        //if(currentSteps.Count==0){Debug.LogWarning("No starting steps!");AddStep((int)StepPropertiesType.gunShoot,true);RefreshStartingElements();}
        CloseStepsUI(true);addStepsUI.SetActive(false);
        RepopulateUIFromSteps();
        executionBorder.SetActive(false);
    }
    void Update(){
        if(!GameManager.GlobalTimeIsPausedNotStepped||StoryboardManager.IsOpen){
            if(!VictoryCanvas.Won&&!StoryboardManager.IsOpen){
                if(Input.GetKeyDown(KeyCode.E)){if(!StepsUIOpen){OpenStepsUI();}else{CloseStepsUI();}}
                if(Input.GetKeyDown(KeyCode.Space)){UIInputSystem.instance.currentSelected=null;StartSteps();}
            }

            ///Interpolate movement
            float _stepUIPos=325f*Time.unscaledDeltaTime;
            stepsUI.GetComponent<RectTransform>().anchoredPosition=Vector2.MoveTowards(stepsUI.GetComponent<RectTransform>().anchoredPosition,targetUIPos,_stepUIPos);
            topButtons.GetComponent<RectTransform>().anchoredPosition=Vector2.MoveTowards(topButtons.GetComponent<RectTransform>().anchoredPosition,_topButtonsTargetPos,_stepUIPos);
            pauseButton.GetComponent<RectTransform>().anchoredPosition=Vector2.MoveTowards(pauseButton.GetComponent<RectTransform>().anchoredPosition,_pauseButtonTargetPos,_stepUIPos);
            float _addStepsUIHeight=1500f*Time.unscaledDeltaTime;
            addStepsUI.GetComponent<RectTransform>().sizeDelta=Vector2.MoveTowards(addStepsUI.GetComponent<RectTransform>().sizeDelta,new Vector2(addStepsUI.GetComponent<RectTransform>().sizeDelta.x,targetAddStepsUIHeight),_addStepsUIHeight);

            // if(stepsUI.GetComponent<RectTransform>().anchoredPosition==Vector2.zero){
            //     // topButtons.SetActive(true);
            //     showButton.SetActive(false);
            // }else if(stepsUI.GetComponent<RectTransform>().anchoredPosition==_hiddenUIPos){
            //     // topButtons.SetActive(false);
            //     // showButton.SetActive(true);
            // }
            if(pauseButton.GetComponent<RectTransform>().anchoredPosition==_pauseButtonHiddenPos){
                pauseButton.SetActive(false);
            }else{
                pauseButton.SetActive(true);
            }
            
            var cs=targetCamSettings;
            float _stepCamPos=60f*Time.unscaledDeltaTime;
            Camera.main.GetComponent<CamShake>().SetDefaultPos(Vector3.MoveTowards(Camera.main.GetComponent<CamShake>().GetDefaultPos(),new Vector3(cs.pos.x,cs.pos.y,Camera.main.transform.position.z),_stepCamPos));
            float _stepCamSize=5f*Time.unscaledDeltaTime;
            Camera.main.orthographicSize=Vector2.MoveTowards(new Vector2(0,Camera.main.orthographicSize),new Vector2(0,cs.size),_stepCamSize).y;

            if(!VictoryCanvas.Won){
                if(allStepsDone&&reopenStepsUIDelay>0&&FindObjectsOfType<Bullet>().Length==0)reopenStepsUIDelay-=Time.unscaledDeltaTime;
                if(allStepsDone&&reopenStepsUIDelay<=0){OpenStepsUI();}
            }

            // Debug.Log("started steps: "+startedSteps+" | stepsRunning: "+stepsRunning+" | _areStepsBeingRunOrBulletsBouncing(): "+_areStepsBeingRunOrBulletsBouncing()+" | allStepsDone: "+allStepsDone);
            if(StepsUIOpen||VictoryCanvas.Won||StoryboardManager.IsOpen){stepsRunning=false;executionBorder.SetActive(false);}//forcing it off mainly because after VictoryCanvas.Won
            else{executionBorder.SetActive(_areStepsBeingRunOrBulletsBouncing());}

            //if(selectedStep==null)ResetAfterPreviewingSteps();
        }
    }
    public void RefreshStartingElements(){
        var l=LevelMapManager.instance.GetCurrentLevelMap();
        var _gun=startingElementsTransform.GetChild(0);
        var _energy=startingElementsTransform.GetChild(1);
        var _bounces=startingElementsTransform.GetChild(2);
        var _speed=startingElementsTransform.GetChild(3);
        var _accel=startingElementsTransform.GetChild(4);
        var _maxSpeed=startingElementsTransform.GetChild(5);


        string _gunRotationTxt=Mathf.RoundToInt(l.defaultGunRotation).ToString()+"°";
        if(l.accurateGunRotation){
            _gunRotationTxt=l.defaultGunRotation.ToString()+"°";
            if(l.defaultGunRotation==Mathf.RoundToInt(l.defaultGunRotation)){_gunRotationTxt=Mathf.RoundToInt(l.defaultGunRotation).ToString()+".0°";}
        }
        foreach(Transform t in _gun){
            if(t!=this.transform){
                if(Player.instance!=null)if(t.GetComponent<Image>()!=null)t.GetComponent<Image>().sprite=Player.instance.GetGunSpr(l.startingChargePositive);
                if(t.GetComponent<TMPro.TextMeshProUGUI>()!=null)t.GetComponent<TMPro.TextMeshProUGUI>().text=_gunRotationTxt;
            }
        }


        string maxEnergyStr=l.stepEnergy.ToString();if(l.stepEnergy<0){maxEnergyStr="∞";}
        string _energyTxt=(currentStepsEnergyUsed.ToString()+" / "+maxEnergyStr);if(l.stepEnergy<0){_energyTxt="∞";}
        if(_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=_energyTxt;}
        if(currentEnergyLeft()<=0&&l.stepEnergy>0){_energy.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareRed");}
        else{_energy.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquare");}


        string bouncesLimitStr=l.bulletBounceLimit.ToString();if(l.bulletBounceLimit<0){bouncesLimitStr="∞";}
        if(_bounces.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_bounces.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=bouncesLimitStr;}


        string speedStr=l.bulletSpeed.ToString();
        if(_speed.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_speed.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=speedStr;}


        if(l.bulletAcceleration!=0){
            _accel.gameObject.SetActive(true);
        }else{
            _accel.gameObject.SetActive(false);
        }
        string accelStr="+"+l.bulletAcceleration.ToString();
        if(l.bulletAcceleration<0){accelStr="-"+l.bulletAcceleration.ToString();}
        if(l.bulletAccelerationMultiply){accelStr="*"+l.bulletAcceleration.ToString();}
        if(_accel.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_accel.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=accelStr;}


        if(!l.bulletMaxSpeedInfinite){
            _maxSpeed.gameObject.SetActive(true);
        }else{
            _maxSpeed.gameObject.SetActive(false);
        }
        string maxSpeedStr=l.bulletMaxSpeed.ToString();if(l.bulletMaxSpeedInfinite){maxSpeedStr="∞";}
        if(_maxSpeed.GetComponentInChildren<TMPro.TextMeshProUGUI>()!=null){_maxSpeed.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=maxSpeedStr;}
    }
    bool startedSteps,stepsRunning,allStepsDone;float reopenStepsUIDelay;
    public bool _areStepsBeingRun(){return (stepsRunning&&!allStepsDone);}
    // public bool _areStepsBeingRunOrBulletsBouncing(){return (stepsRunning||FindObjectsOfType<Bullet>().Length>0);}
    public bool _areStepsBeingRunOrBulletsBouncing(){return (stepsRunning||FindObjectsOfType<Bullet>().Length>0&&!VictoryCanvas.Won);}

    
    public void StartSteps(){
        if(currentSteps.Count>0&&!VictoryCanvas.Won&&!StoryboardManager.IsOpen){
            // Debug.Log("StartSteps() inside");
            // RepopulateStepsFromUI();
            // SelectStep(null,true);UIInputSystem.instance.currentSelected=null;UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);SwitchSelectables(false);
            // ResetAfterPreviewingSteps();
            if(!startedSteps&&!GameManager.GlobalTimeIsPausedNotStepped&&!VictoryCanvas.Won){
                Debug.Log("StartSteps() inside");
                RepopulateStepsFromUI();
                SelectStep(null,true);UIInputSystem.instance.currentSelected=null;UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);SwitchSelectables(false);
                ResetAfterPreviewingSteps();
                StartCoroutine(StartStepsI());
            }
        }else if(currentSteps.Count==0){AudioManager.instance.Play("Deny");}
    }
    IEnumerator StartStepsI(){
        if(!startedSteps&&!GameManager.GlobalTimeIsPausedNotStepped&&!VictoryCanvas.Won){
            Debug.Log("StartStepsI()");
            startedSteps=true;
            allStepsDone=false;
            CloseStepsUI();
            yield return new WaitForSeconds(1.5f);//Wait for end of animation
            SelectStep(null,true);UIInputSystem.instance.currentSelected=null;UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);SwitchSelectables(false);ResetAfterPreviewingSteps();
            if(stepsCoroutine==null){stepsRunning=true;stepsCoroutine=StartCoroutine(ExecuteStepsI());}
        }else{yield return null;}
    }
    public void StopSteps(){
        Debug.Log("Stopping steps");
        currentStepId=0;startedSteps=false;stepsRunning=false;SwitchSelectables(true);
        if(stepsCoroutine!=null){StopCoroutine(stepsCoroutine);stepsCoroutine=null;}
    }
    void ReexecuteSteps(){Debug.Log("ReexecuteSteps()");if(stepsCoroutine!=null){stepsCoroutine=null;stepsCoroutine=StartCoroutine(ExecuteStepsI());Debug.Log("ReexecutingSteps..");}}
    Coroutine stepsCoroutine;//,singleStepCoroutine;
    IEnumerator ExecuteStepsI(){
        if(startedSteps){
            Debug.Log("Executing Step: "+currentStepId);
            if(currentSteps.Count==0){Debug.LogWarning("No steps!");}
            if(currentStepId<currentSteps.Count&&startedSteps){
                var _s=currentSteps[currentStepId];
                if(_s.stepType==StepPropertiesType.delay){yield return new WaitForSeconds(_s.delay);}
                ExecuteSingleStep(currentStepId);
                currentStepId++;
                if(currentStepId<currentSteps.Count){yield return new WaitForSeconds(0.01f);if(startedSteps)ReexecuteSteps();}
                else{currentStepId=0;allStepsDone=true;stepsRunning=false;reopenStepsUIDelay=2f;Debug.Log("All steps done!");}
            }
        }
    }
    void ExecuteSingleStep(int id){StartCoroutine(ExecuteSingleStepI(id));}
    void PreviewStep(int id){
        if(currentSteps.Count>id&&id>=0){
            var _s=currentSteps[id];
            switch(_s.stepType){
                case StepPropertiesType.gunRotation:
                    Player.instance.SetGunRotation(_s.gunRotation);
                break;
            }
        }else{Debug.LogWarning("Trying to preview step by id: "+id+" which is not in the list?");}
    }
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
                Player.instance.SwitchPolarity();
            break;
            case StepPropertiesType.gunRotation:
                Player.instance.SetGunRotation(_s.gunRotation);
            break;
            case StepPropertiesType.mirrorPos:
                LevelMapManager.instance.mirrorListSorted[_s.objectId].transform.position=_s.mirrorPos;
            break;
            case StepPropertiesType.laserPos:
                LevelMapManager.instance.laserListSorted[_s.objectId].transform.position=_s.laserPos;
            break;
            case StepPropertiesType.switchAllLasers:
                foreach(Laser l in LevelMapManager.instance.laserListSortedWithCloned)l.SwitchPolarity();
            break;
        }
    }
    void ResetAfterPreviewingSteps(){
        var l=LevelMapManager.instance.GetCurrentLevelMap();
        Player.instance.SetGunRotation(l.defaultGunRotation);
    }

    public void OpenStepsUI_(bool reset=false){OpenStepsUI(reset);}
    public void OpenStepsUI(bool reset=false,bool force=false){
        if(!StepsUIOpen||force){
            StopSteps();
            ResetAfterPreviewingSteps();
            if(reset||_areStepsBeingRunOrBulletsBouncing()||allStepsDone){LevelMapManager.instance.CallRestart();}

            if(currentStepId==0||force){
                targetUIPos=Vector2.zero;
                _topButtonsTargetPos=_topButtonsOriginPos;
                _pauseButtonTargetPos=_pauseButtonOriginPos;
                stepsUI.SetActive(true);
                SetCameraSettings(openCameraSettings);
                StepsUIOpen=true;
                // topButtons.SetActive(true);
                hideButton.SetActive(true);
                showButton.SetActive(false);
                allStepsDone=false;
                foreach(CanvasGroup cg in GetComponentsInChildren<CanvasGroup>()){
                    cg.interactable=true;
                    cg.blocksRaycasts=true;
                }
            }
        }
    }
    public void CloseStepsUI(bool force=false){
        if(StepsUIOpen||force){
            targetUIPos=_hiddenUIPos;
            _topButtonsTargetPos=_topButtonsHiddenPos;
            _pauseButtonTargetPos=_pauseButtonHiddenPos;
            SetCameraSettings(defaultCameraSettings);
            StepsUIOpen=false;
            targetAddStepsUIHeight=0;
            //addStepsUI.SetActive(false);
            // topButtons.SetActive(false);
            hideButton.SetActive(false);
            showButton.SetActive(true);
            foreach(CanvasGroup cg in GetComponentsInChildren<CanvasGroup>()){
                cg.interactable=false;
                cg.blocksRaycasts=false;
            }
            AudioManager.instance.Play("StartSteps");
        }
    }
    void ForceCloseStepsUI(){
        stepsUI.GetComponent<RectTransform>().anchoredPosition=_hiddenUIPos;targetUIPos=_hiddenUIPos;
        topButtons.GetComponent<RectTransform>().anchoredPosition=_topButtonsHiddenPos;_topButtonsTargetPos=_topButtonsHiddenPos;
        pauseButton.GetComponent<RectTransform>().anchoredPosition=_pauseButtonHiddenPos;_pauseButtonTargetPos=_pauseButtonHiddenPos;
        targetAddStepsUIHeight=0;addStepsUI.SetActive(false);
        stepsUI.SetActive(false);
        SetCameraSettings(defaultCameraSettings);
        StepsUIOpen=false;
    }
    public void ToggleOpenCloseAddStepsUI(){
        if(StepsUIOpen){
            if(!addStepsUI.activeSelf||targetAddStepsUIHeight==0){
                addStepsUI.SetActive(true);targetAddStepsUIHeight=addStepsUIAnchorHeightShown;
                foreach(CanvasGroup cg in addStepsUI.GetComponentsInChildren<CanvasGroup>()){
                    cg.interactable=true;
                    cg.blocksRaycasts=true;
                }
                SetAllowedSteps();
                return;
            }
            else{
                targetAddStepsUIHeight=0;
                foreach(CanvasGroup cg in addStepsUI.GetComponentsInChildren<CanvasGroup>()){
                    cg.interactable=false;
                    cg.blocksRaycasts=false;
                }
                SetAllowedSteps();
                return;
            }
        }
        //SetAllowedSteps();
    }
    public void Pause(){PauseMenu.instance.Pause();}

    public void AddStepButton(int id){AddStep(id,false);}
    public void AddStep(int id,bool quiet=false){
        if(canAfford(id)){
            //currentSteps.Add(new StepProperties(){stepType=(StepPropertiesType)id});
            if((id!=(int)StepPropertiesType.mirrorPos&&id!=(int)StepPropertiesType.laserPos)||
            ((id==(int)StepPropertiesType.mirrorPos&&FindObjectsOfType<Mirror>().Length>0)||(id==(int)StepPropertiesType.laserPos&&FindObjectsOfType<Laser>().Length>0))){
                GameObject gos=Instantiate(stepsUIPrefabs[id],stepsUIListContent);
                SetSelectableStep(gos);
                RepopulateStepsFromUI();
                SetAllowedSteps(0.15f);
                if(id==(int)StepPropertiesType.gunRotation){
                    if(LevelMapManager.instance.GetCurrentLevelMap().accurateGunRotation){
                        //gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().contentType=TMPro.TMP_InputField.ContentType.DecimalNumber;
                        gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().contentType=TMPro.TMP_InputField.ContentType.Custom;
                        gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().inputValidator=AssetsManager.instance.angleInputValidator;
                        //gos.transform.GetComponentInChildren<LimitInputFieldNum>().SwitchToFloat();
                        gos.GetComponent<StepUIPrefab>().SetTextInComponentDelay("45,00",0.2f);
                        gos.GetComponent<StepUIPrefab>().SetAutoFromComponentDelay(0.21f);
                        //Debug.Log("Set GunRotation to Float");
                    }else{
                        gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().contentType=TMPro.TMP_InputField.ContentType.IntegerNumber;
                        gos.GetComponent<StepUIPrefab>().SetTextInComponentDelay("45",0.2f);
                        //gos.transform.GetComponentInChildren<LimitInputFieldNum>().SwitchToInt();
                    }
                }else if(id==(int)StepPropertiesType.delay){
                    gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().contentType=TMPro.TMP_InputField.ContentType.Custom;
                    gos.transform.GetComponentInChildren<TMPro.TMP_InputField>().inputValidator=AssetsManager.instance.delayInputValidator;
                    //gos.transform.GetComponentInChildren<LimitInputFieldNum>().SwitchToFloat();
                    gos.GetComponent<StepUIPrefab>().SetTextInComponentDelay("1,00",0.2f);
                    gos.GetComponent<StepUIPrefab>().SetAutoFromComponentDelay(0.21f);
                }

                if(!quiet)AudioManager.instance.Play("StepAdd");
            }
        }else{if(!quiet)AudioManager.instance.Play("Deny");}
    }
    public void RepopulateStepsFromUI(){StartCoroutine(RepopulateStepsFromUI_I());}
    IEnumerator RepopulateStepsFromUI_I(){
        currentSteps.Clear();currentSteps=new List<StepProperties>();currentStepsEnergyUsed=0;RefreshStartingElements();
        yield return new WaitForSecondsRealtime(0.1f);
        foreach(StepUIPrefab s in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){
            if(s.GetComponentsInChildren<TMPro.TMP_InputField>().Length>0){
                //s.GetComponentInChildren<LimitInputFieldNum>().UpdateInputFieldAutoFromComponent();
                s.SetAutoFromComponent();}
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
            Navigation n=Navigation.defaultNavigation;n.mode=Navigation.Mode.None;
            bt.navigation=n;
        }
    }
    public void SelectStep(Button bt,bool forceUpdate=false){
        if(!Input.GetKeyDown(KeyCode.Space)&&!Input.GetKey(KeyCode.Space)){
            // if(bt!=null){
            //     Debug.Log("SelectStep("+bt+", "+bt.interactable+", "+forceUpdate+")");
            // }else{
            //     Debug.Log("SelectStep("+bt+", "+", "+forceUpdate+")");
            // }
            if(bt!=null&&bt.interactable&&!startedSteps){
                var sui=bt.GetComponent<StepUIPrefab>();
                if(selectedStep!=bt||selectedStep==null||forceUpdate){
                    selectedStep=bt;
                    foreach(StepUIPrefab _sui in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){_sui.SetDefaultSpr();}
                    sui.SetSelectedSpr();
                    PreviewStep(currentSteps.FindIndex(x=>x==sui.stepProperties));
                    //ExecuteSingleStep(currentSteps.FindIndex(x=>x==sui.stepProperties));
                    // Debug.Log("Selecting Step");
                }else if(selectedStep==bt){
                    selectedStep=null;UIInputSystem.instance.currentSelected=null;//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    foreach(StepUIPrefab _sui in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){_sui.SetDefaultSpr();}
                    sui.SetDefaultSpr();
                    ResetAfterPreviewingSteps();
                    // Debug.Log("UnSelecting Step");
                }
            }else{
                if(bt==null||(bt!=null&&bt.interactable)){
                    selectedStep=null;UIInputSystem.instance.currentSelected=null;//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    if(!startedSteps){
                        ResetAfterPreviewingSteps();
                        foreach(StepUIPrefab sui in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){sui.SetDefaultSpr();}
                        // Debug.Log("UnSelecting Steps.");
                    }
                }
            }
        }
    }
    public void SwitchSelectables(bool on=true){
        foreach(StepUIPrefab sui in stepsUIListContent.GetComponentsInChildren<StepUIPrefab>()){if(sui.GetComponent<Button>()!=null)sui.GetComponent<Button>().interactable=on;}
    }
    public void SetAllowedSteps(float delay=0f){StartCoroutine(SetAllowedStepsI(delay));}
    IEnumerator SetAllowedStepsI(float delay){
        yield return new WaitForSecondsRealtime(delay);
        for(var i=0;i<addStepsUIListContent.childCount;i++){
            //LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes.Length>0;
            if(addStepsUIListContent.childCount>i&&LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes.ContainsKey((StepPropertiesType)i)){
                addStepsUIListContent.GetChild(i).gameObject.SetActive(LevelMapManager.instance.GetCurrentLevelMap().allowedStepTypes[(StepPropertiesType)i]);
                int _stepCost=_stepCostForTypeCurrentLvl((StepPropertiesType)i);
                var _energy=addStepsUIListContent.transform.GetChild(i).Find("Energy").gameObject;
                if(_stepCost!=0){_energy.SetActive(true);_energy.GetComponentInChildren<TMPro.TextMeshProUGUI>().text=_stepCost.ToString();}
                else{_energy.SetActive(false);}
                if(!canAfford(i)){addStepsUIListContent.GetChild(i).GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareRed");}
                else{addStepsUIListContent.GetChild(i).GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareBlue");}
            // }else{Debug.LogWarning("Children of addStepsUIListContent not setup properly!");if(addStepsUIListContent.childCount>i)addStepsUIListContent.GetChild(i).gameObject.SetActive(false);}
            }else{
                if(addStepsUIListContent.childCount<i){Debug.LogWarning("Children of addStepsUIListContent not setup properly!");}
                if(addStepsUIListContent.childCount>i){addStepsUIListContent.GetChild(i).gameObject.SetActive(false);}
            }
        }
    }
    public int currentEnergyLeft(){return LevelMapManager.instance.GetCurrentLevelMap().stepEnergy-currentStepsEnergyUsed;}
    public bool canAfford(int i){return (currentEnergyLeft()>=_stepCostForTypeCurrentLvl((StepPropertiesType)i)||LevelMapManager.instance.GetCurrentLevelMap().stepEnergy<0);}
    public int _stepCostForTypeCurrentLvl(StepPropertiesType stepType){
        int _stepCost=0;
        if(LevelMapManager.instance.GetCurrentLevelMap().stepTypesCosts.ContainsKey(stepType)){
            _stepCost=LevelMapManager.instance.GetCurrentLevelMap().stepTypesCosts[stepType];
        }
        return _stepCost;
    }
    public void SumUpEnergy(){
        currentStepsEnergyUsed=0;
        foreach(StepProperties s in currentSteps){
            int _stepCost=_stepCostForTypeCurrentLvl(s.stepType);
            currentStepsEnergyUsed+=_stepCost;
            RefreshStartingElements();
        }
    }
    public void SetCameraSettings(CameraSettings cs){targetCamSettings=cs;}
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
public enum StepPropertiesType{delay,gunShoot,gunPolarity,gunRotation,mirrorPos,laserPos,switchAllLasers}
//public class StepCosts{Dictionary<StepPropertiesType,int>();}