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
    [DisableInEditorMode][SerializeField]public List<Laser> laserListSorted;
    [DisableInEditorMode][SerializeField]public List<Mirror> mirrorListSorted;
    [DisableInEditorMode][SerializeField]public List<LogicGate> logicGateListSorted;

    void Awake(){if(LevelMapManager.instance!=null/*||OutOfContextScene()*/){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}}
    void Start(){if(GSceneManager.CheckScene("Game"))Restart();}//So that lists get populated
    public static bool OutOfContextScene(){return (!GSceneManager.CheckScene("Game")&&!GSceneManager.CheckScene("LevelSelect"));}
    public static bool InContextScene(){return (GSceneManager.CheckScene("Game")||GSceneManager.CheckScene("LevelSelect"));}
    void Update(){
        if(GSceneManager.CheckScene("Game")&&StepsManager.StepsUIOpen){
            if(Input.GetKeyDown(KeyCode.R)){if(restartDelay<=0)CallRestart();}
            if(restartDelay>0){restartDelay-=Time.unscaledDeltaTime;}
        }
        if(!GSceneManager.CheckScene("Game")){ResetLists();if(levelParent!=null){Destroy(levelParent);}}
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
    public void CallRestartQuiet(){Restart();}
    void Restart(){
        if(levelParent!=null&&levelMaps[levelCurrent].parent!=null){
            ///Cleanup
            foreach(Bullet b in FindObjectsOfType<Bullet>()){Destroy(b.gameObject);}

            ///Reset
            for(int i=0;i<levelParent.transform.childCount;i++){
                var _parentChild=levelParent.transform.GetChild(i);
                var _prefabChild=levelMaps[levelCurrent].parent.transform.GetChild(i);
                _parentChild.gameObject.SetActive(_prefabChild.gameObject.activeSelf);
                _parentChild.transform.position=_prefabChild.transform.position;

                ///Laser
                if(_parentChild.GetComponent<Laser>()!=null&&_prefabChild.GetComponent<Laser>()!=null){
                    var _parentC=_parentChild.GetComponent<Laser>();
                    var _prefabC=_prefabChild.GetComponent<Laser>();
                    if(_parentC.clonedLaser)Destroy(_parentChild);
                    _parentC.SetPolarity(_prefabC.positive);
                    if(!laserListSorted.Contains(_parentC))laserListSorted.Add(_parentC);
                }else if((_parentChild.GetComponent<Laser>()==null&&_prefabChild.GetComponent<Laser>()!=null)
                ||(_parentChild.GetComponent<Laser>()!=null&&_prefabChild.GetComponent<Laser>()==null)){
                    Debug.LogWarning("Components 'Laser' do not match on child: "+i+" | on level: "+levelCurrent);}
                ///Logic Gates
                if(_parentChild.GetComponent<LogicGate>()!=null&&_prefabChild.GetComponent<LogicGate>()!=null){
                    var _parentC=_parentChild.GetComponent<LogicGate>();
                    var _prefabC=_prefabChild.GetComponent<LogicGate>();
                    _parentC.charged=_prefabC.charged;
                    _parentC.active=_prefabC.active;
                    if(!logicGateListSorted.Contains(_parentC))logicGateListSorted.Add(_parentC);
                }///Mirrors
                if(_parentChild.GetComponent<Mirror>()!=null&&_prefabChild.GetComponent<Mirror>()!=null){
                    var _parentC=_parentChild.GetComponent<Mirror>();
                    var _prefabC=_prefabChild.GetComponent<Mirror>();
                    _parentC.opposite=_prefabC.opposite;
                    _parentC.ninetydegree=_prefabC.ninetydegree;
                    if(!mirrorListSorted.Contains(_parentC))mirrorListSorted.Add(_parentC);
                }
                ///Spawnpoint
                if(_parentChild.GetComponent<Spawnpoint>()!=null&&_prefabChild.GetComponent<Spawnpoint>()!=null){
                    var _parentC=_parentChild.GetComponent<Spawnpoint>();
                    var _prefabC=_prefabChild.GetComponent<Spawnpoint>();
                    _parentC.playerDir=_prefabC.playerDir;
                }
            }
            ///Player
            StartCoroutine(RestartPlayerI());
        }else{
            if(GSceneManager.CheckScene("Game")){
                if(levelParent==null){Debug.LogWarning("levelParent is null!");
                    if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));}
                    else{InstantiateLevelParent();Restart();}
                }
                if(levelMaps.Length<levelCurrent){Debug.LogWarning("Level list shorter than "+levelCurrent);}
                else{if(levelMaps[levelCurrent].parent==null){Debug.LogWarning("Level "+levelCurrent+" not assigned null!");}}
            }else{Debug.LogWarning("Trying to restart outside of 'Game' scene!");return;}
        }
    }
    IEnumerator RestartPlayerI(){
        var spawnpoint=FindObjectOfType<Spawnpoint>();
        Destroy(Player.instance.gameObject);
        yield return new WaitForSecondsRealtime(0.05f);
        Instantiate(CoreSetup.instance._getPlayerPrefab().gameObject,(Vector2)spawnpoint.transform.position+spawnpoint.playerOffset,Quaternion.identity);
        Player.instance.SetGunRotation(levelMaps[levelCurrent].defaultGunRotation);
        Player.instance.SetPolarity(levelMaps[levelCurrent].startingChargePositive,true);
        Player.instance.bulletBounceLimit=levelMaps[levelCurrent].bulletBounceLimit;
        Player.instance.bulletSpeed=levelMaps[levelCurrent].bulletSpeed;
    }
    void InstantiateLevelParent(){
        //levelParent=Instantiate(levelMaps[levelCurrent].parent,Vector2.zero,Quaternion.identity);levelParent.transform.position=Vector2.zero;
        levelParent=Instantiate(levelMaps[levelCurrent].parent,transform);levelParent.transform.position=Vector2.zero;
        levelParent.gameObject.name="Level";
    }
    void ResetLists(){
        laserListSorted.Clear();
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
            StepsManager.instance.currentSteps=levelMaps[levelCurrent].defaultSteps;StepsManager.instance.SumUpEnergy();
        }}
        CallRestart(0,true);
        ResetLists();
    }
    public void LoadNextLevel(){
        if(levelMaps.Length>(levelCurrent+1)){LoadLevel(levelCurrent+1);CallRestart(0.35f,false);}
        else{Debug.LogWarning("No more levels :(");}
    }
    public LevelMap GetCurrentLevelMap(){return levelMaps[levelCurrent];}
}
/*[System.Serializable]
public class LevelMap{
    public GameObject parent;
    public int stepEnergy=6;
    public bool startingChargePositive=true;
    public float defaultGunRotation=0;
    public int bulletBounceLimit=10;
    public float bulletSpeed=6f;
    public dir playerDir=dir.down;
    public List<StepProperties> defaultSteps;
    public List<StepPropertiesType> allowedStepTypes=new List<StepPropertiesType>(){
        StepPropertiesType.delay
        ,StepPropertiesType.gunShoot
        ,StepPropertiesType.gunPolarity
        ,StepPropertiesType.gunRotation
        //,StepPropertiesType.mirrorPos
    };
    [DictionaryDrawerSettings(KeyLabel = "Type", ValueLabel = "Cost")]
    public Dictionary<StepPropertiesType,int> stepTypesCosts=new Dictionary<StepPropertiesType,int>();
}*/