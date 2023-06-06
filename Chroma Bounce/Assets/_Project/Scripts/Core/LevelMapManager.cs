using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelMapManager : MonoBehaviour{   public static LevelMapManager instance;
    [AssetsOnly]public LevelMap[] levelMaps;
    [SceneObjectsOnly]public GameObject levelParent;
    [Header("Current variables")]
    [SerializeField]float restartDelaySet=0.5f;
    [DisableInEditorMode][SerializeField]float restartDelay;
    [SerializeField]public int levelCurrent=-1;
    [DisableInEditorMode][SerializeField]public float levelTimer;
    [DisableInEditorMode][SerializeField]public List<LogicGate> logicGateListSorted;
    [DisableInEditorMode][SerializeField]public List<Laser> laserListSorted;
    [DisableInEditorMode][SerializeField]public List<Laser> laserListSortedWithCloned;
    [DisableInEditorMode][SerializeField]public List<Mirror> mirrorListSorted;

    void Awake(){if(LevelMapManager.instance!=null/*||OutOfContextScene()*/){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}}
    void Start(){if(GSceneManager.CheckScene("Game"))Restart();}
    public static bool OutOfContextScene(){return (!GSceneManager.CheckScene("Game")&&!GSceneManager.CheckScene("LevelSelect"));}
    public static bool InContextScene(){return (GSceneManager.CheckScene("Game")||GSceneManager.CheckScene("LevelSelect"));}
    void Update(){
        if(GSceneManager.CheckScene("Game")&&StepsManager.StepsUIOpen){
            if(Input.GetKeyDown(KeyCode.R)){if(restartDelay<=0)CallRestart();}
            if(restartDelay>0){restartDelay-=Time.unscaledDeltaTime;}
        }
        if(!GSceneManager.CheckScene("Game")){ResetLists();if(levelParent!=null){Destroy(levelParent);}}
        if(GSceneManager.CheckScene("Game")){if(!StepsManager.StepsUIOpen&&!VictoryCanvas.Won){if(StepsManager.instance._areStepsBeingRunOrBulletsBouncing()){levelTimer+=Time.unscaledDeltaTime;}}}else{levelTimer=0;}
    }
    public void CallRestart(float delay=0f,bool quiet=false){StartCoroutine(RestartI(delay,quiet));}
    IEnumerator RestartI(float delay=0f,bool quiet=false){
        yield return new WaitForSecondsRealtime(delay);
        restartDelay=restartDelaySet;
        if(!quiet){
            var _vfx=AssetsManager.instance.VFX("RestartLevel",Vector2.zero,0.75f);_vfx.transform.position=new Vector3(0,0,-20);
            AudioManager.instance.Play("RestartLevel");
            CamShake.instance.DoCamShake(0.5f,0.2f);
        }
        if(!GameManager.GlobalTimeIsPaused)yield return new WaitForSeconds(0.25f);
        else yield return new WaitForSecondsRealtime(0.25f);
        Restart();
    }
    void Restart(){
        levelTimer=0;
        if(levelParent!=null&&levelMaps[levelCurrent].parent!=null){
            ///Cleanup
            foreach(Bullet b in FindObjectsOfType<Bullet>()){Destroy(b.gameObject);}

            ///Reset
            for(int i=0;i<levelParent.transform.childCount;i++){
                var _parentChild=levelParent.transform.GetChild(i);
                var _prefabChild=levelMaps[levelCurrent].parent.transform.GetChild(i);
                _parentChild.gameObject.SetActive(_prefabChild.gameObject.activeSelf);
                _parentChild.transform.position=_prefabChild.transform.position;
                _parentChild.transform.eulerAngles=_prefabChild.transform.eulerAngles;

                WorldCanvas.instance.Cleanup();

                ///Logic Gates
                if(_parentChild.GetComponent<LogicGate>()!=null&&_prefabChild.GetComponent<LogicGate>()!=null){
                    var _parentC=_parentChild.GetComponent<LogicGate>();
                    var _prefabC=_prefabChild.GetComponent<LogicGate>();
                    _parentC.logicGateType=_prefabC.logicGateType;
                    _parentC.logicGateSignalsType=_prefabC.logicGateSignalsType;
                    _parentC.charged1=_prefabC.charged1;
                    _parentC.charged2=_prefabC.charged2;
                    _parentC.ForceUpdateActive(_prefabC.active);
                    _parentC.motherGate=_prefabC.motherGate;
                    /*_parentC.gatePowering1=_prefabC.gatePowering1;
                    _parentC.gatePowering2=_prefabC.gatePowering2;*/
                    _parentC.ConnectWithGatePoweringDelay();
                    if(!logicGateListSorted.Contains(_parentC))logicGateListSorted.Add(_parentC);
                }else if((_parentChild.GetComponent<LogicGate>()==null&&_prefabChild.GetComponent<LogicGate>()!=null)
                ||(_parentChild.GetComponent<LogicGate>()!=null&&_prefabChild.GetComponent<LogicGate>()==null)){
                    Debug.LogWarning("Components 'LogicGate' do not match on child: "+i+" | on level: "+levelCurrent);ReinstantiateLevel();}

                ///Laser
                if(_parentChild.GetComponent<Laser>()!=null&&_prefabChild.GetComponent<Laser>()!=null){
                    var _parentC=_parentChild.GetComponent<Laser>();
                    var _prefabC=_prefabChild.GetComponent<Laser>();
                    if(_parentC.clonedLaser)Destroy(_parentChild);
                    _parentC.SetPolarity(_prefabC.positive,true,true);
                    if(!laserListSorted.Contains(_parentC))laserListSorted.Add(_parentC);
                    if(!laserListSortedWithCloned.Contains(_parentC))laserListSortedWithCloned.Add(_parentC);
                }else if((_parentChild.GetComponent<Laser>()==null&&_prefabChild.GetComponent<Laser>()!=null)
                ||(_parentChild.GetComponent<Laser>()!=null&&_prefabChild.GetComponent<Laser>()==null)){
                    Debug.LogWarning("Components 'Laser' do not match on child: "+i+" | on level: "+levelCurrent);ReinstantiateLevel();}

                ///Mirrors
                if(_parentChild.GetComponent<Mirror>()!=null&&_prefabChild.GetComponent<Mirror>()!=null){
                    var _parentC=_parentChild.GetComponent<Mirror>();
                    var _prefabC=_prefabChild.GetComponent<Mirror>();
                    _parentC.opposite=_prefabC.opposite;
                    _parentC.ninetydegree=_prefabC.ninetydegree;
                    if(!mirrorListSorted.Contains(_parentC))mirrorListSorted.Add(_parentC);
                }else if((_parentChild.GetComponent<Mirror>()==null&&_prefabChild.GetComponent<Mirror>()!=null)
                ||(_parentChild.GetComponent<Mirror>()!=null&&_prefabChild.GetComponent<Mirror>()==null)){
                    Debug.LogWarning("Components 'Mirror' do not match on child: "+i+" | on level: "+levelCurrent);ReinstantiateLevel();}

                ///Spawnpoint
                if(_parentChild.GetComponent<Spawnpoint>()!=null&&_prefabChild.GetComponent<Spawnpoint>()!=null){
                    var _parentC=_parentChild.GetComponent<Spawnpoint>();
                    var _prefabC=_prefabChild.GetComponent<Spawnpoint>();
                    _parentC.playerOffset=_prefabC.playerOffset;
                }
            }
            ///Player
            StartCoroutine(RestartPlayerI());
        }else{
            if(GSceneManager.CheckScene("Game")){
                if(levelParent==null){Debug.LogWarning("levelParent is null!");
                    if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));}
                    InstantiateLevelParent();CallRestart(0,true);
                }
                if(levelMaps.Length<levelCurrent){Debug.LogWarning("Level list shorter than "+levelCurrent);}
                else{if(levelMaps[levelCurrent].parent==null){Debug.LogWarning("Level "+levelCurrent+" not assigned null!");}}
            }else{Debug.LogWarning("Trying to restart outside of 'Game' scene!");return;}
        }
    }
    IEnumerator RestartPlayerI(){
        var spawnpoint=FindObjectOfType<Spawnpoint>();
        if(Player.instance!=null){Destroy(Player.instance.gameObject);}
        yield return new WaitForSecondsRealtime(0.05f);
        Instantiate(CoreSetup.instance._getPlayerPrefab().gameObject,(Vector2)spawnpoint.transform.position+spawnpoint.playerOffset,Quaternion.identity);
        Player.instance.SetGunRotation(levelMaps[levelCurrent].defaultGunRotation);
        Player.instance.SetPolarity(levelMaps[levelCurrent].startingChargePositive,true);
        Player.instance.bulletBounceLimit=levelMaps[levelCurrent].bulletBounceLimit;
        Player.instance.bulletSpeed=levelMaps[levelCurrent].bulletSpeed;
    }
    void ReinstantiateLevel(){
        ResetLists();
        if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));}
        if(levelParent!=null){Destroy(levelParent);}
        InstantiateLevelParent();
    }
    void InstantiateLevelParent(){
        levelParent=Instantiate(levelMaps[levelCurrent].parent,transform);levelParent.transform.position=Vector2.zero;
        levelParent.gameObject.name="Level";
    }
    void ResetLists(){
        laserListSorted.Clear();
        laserListSortedWithCloned.Clear();
        logicGateListSorted.Clear();
        mirrorListSorted.Clear();
    }

    public void SetLevel(int i){
        levelCurrent=i;
        if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));InstantiateLevelParent();}
        Restart();
        ResetLists();
    }
    public void LoadLevel(int i){LevelMapManager.instance.StartCoroutine(LevelMapManager.instance.LoadLevelI(i));}
    IEnumerator LoadLevelI(int i){
        Debug.Log("Loaded Level "+i);
        levelCurrent=i;
        GSceneManager.instance.LoadGameScene();
        yield return new WaitForSecondsRealtime(0.1f);
        if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));InstantiateLevelParent();}
        StepsManager.instance.ClearAllSteps();
        if(levelMaps[levelCurrent].defaultSteps!=null){if(levelMaps[levelCurrent].defaultSteps.Count>0){
            foreach(StepProperties s in levelMaps[levelCurrent].defaultSteps){
                StepsManager.instance.currentSteps.Add(s);
                //StepsManager.instance.AddStep(s,true);
            }
            StepsManager.instance.RepopulateUIFromSteps();StepsManager.instance.SumUpEnergy();
        }}
        CallRestart(0,true);
        ResetLists();
    }
    public void LoadNextLevel(){
        if(_nextLevelAvailable()){LoadLevel(levelCurrent+1);CallRestart(0.35f,false);}
        else{Debug.LogWarning("No more levels :(");}
    }
    public bool _nextLevelAvailable(){return levelMaps.Length>(levelCurrent+1);}
    public LevelMap GetCurrentLevelMap(){return levelMaps[levelCurrent];}
}