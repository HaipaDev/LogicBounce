using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelMapManager : MonoBehaviour{   public static LevelMapManager instance;
    [AssetsOnly]public LevelMap[] levelMaps;
    [SceneObjectsOnly]public GameObject levelParent;
    [AssetsOnly][SerializeField]Player playerPrefab;
    [SerializeField]float _delaySet=0.5f;
    [DisableInEditorMode][SerializeField]float _delay;
    [SerializeField]int levelCurrent=-1;
    void Awake(){if(LevelMapManager.instance!=null||OutOfContextScene()){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}}
    public static bool OutOfContextScene(){return (!GSceneManager.CheckScene("Game")&&!GSceneManager.CheckScene("LevelSelect"));}
    public static bool InContextScene(){return (GSceneManager.CheckScene("Game")||GSceneManager.CheckScene("LevelSelect"));}
    void Update(){
        if(GSceneManager.CheckScene("Game")){
            if(Input.GetKeyDown(KeyCode.R)){if(_delay<=0)StartCoroutine(RestartI());}
            if(_delay>0){_delay-=Time.unscaledDeltaTime;}
        }
    }
    IEnumerator RestartI(){
        _delay=_delaySet;
        var _vfx=AssetsManager.instance.VFX("RestartLevel",Vector2.zero,0.75f);_vfx.transform.position=new Vector3(0,0,-20);
        AudioManager.instance.Play("RestartLevel");
        Shake.instance.CamShake(0.5f,0.2f);
        if(GameManager.GlobalTimeIsPaused)yield return new WaitForSeconds(0.25f);
        else yield return new WaitForSecondsRealtime(0.25f);
        Restart();
    }
    void Restart(){
        Debug.Log("Restart");
        if(levelParent!=null&&levelMaps[levelCurrent].parent!=null){
            Debug.Log("Restarting");
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
                    _parentC.positive=_prefabC.positive;
                }else if((_parentChild.GetComponent<Laser>()==null&&_prefabChild.GetComponent<Laser>()!=null)
                ||(_parentChild.GetComponent<Laser>()!=null&&_prefabChild.GetComponent<Laser>()==null)){
                    Debug.LogWarning("Components 'Laser' do not match on child: "+i+" | on level: "+levelCurrent);}
                ///Logic Gates
                if(_parentChild.GetComponent<LogicGate>()!=null&&_prefabChild.GetComponent<LogicGate>()!=null){
                    var _parentC=_parentChild.GetComponent<LogicGate>();
                    var _prefabC=_prefabChild.GetComponent<LogicGate>();
                    _parentC.charged=_prefabC.charged;
                    _parentC.active=_prefabC.active;
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
            if(levelParent!=null){Debug.LogWarning("levelParent is null!");
                if(GameObject.Find("Level")!=null){levelParent=GameObject.Find("Level");Restart();}
                else{InstantiateLevelParent();Restart();}
            }
            if(levelMaps.Length<levelCurrent){Debug.LogWarning("Level list shorter than "+levelCurrent);}
            else{if(levelMaps[levelCurrent].parent==null){Debug.LogWarning("Level "+levelCurrent+" not assigned null!");}}
        }
    }
    IEnumerator RestartPlayerI(){
        var spawnpoint=FindObjectOfType<Spawnpoint>();
        Destroy(Player.instance.gameObject);
        yield return new WaitForSecondsRealtime(0.05f);
        Instantiate(playerPrefab.gameObject);Player.instance.transform.position=(Vector2)spawnpoint.transform.position+spawnpoint.playerOffset;
        Player.instance.positive=levelMaps[levelCurrent].startingChargePositive;
        Player.instance.bulletBounceLimit=levelMaps[levelCurrent].bulletBounceLimit;
        Player.instance.bulletSpeed=levelMaps[levelCurrent].bulletSpeed;
    }
    void InstantiateLevelParent(){
        //levelParent=Instantiate(new GameObject(),Vector2.zero,Quaternion.identity);levelParent.gameObject.name="Level";
        levelParent=Instantiate(levelMaps[levelCurrent].parent,Vector2.zero,Quaternion.identity);levelParent.gameObject.name="Level";
    }

    public void SetLevel(int i){
        levelCurrent=i;
        GSceneManager.instance.LoadGameScene();
        if(GameObject.Find("Level")!=null){Destroy(GameObject.Find("Level"));InstantiateLevelParent();}
        Restart();
    }
    public Player _getPlayerPrefab(){return playerPrefab;}
}
[System.Serializable]
public class LevelMap{
    public GameObject parent;
    //public int stepEnergy=6;
    public bool startingChargePositive=true;
    public int bulletBounceLimit=10;
    public float bulletSpeed=6f;
    public dir playerDir=dir.down;
}