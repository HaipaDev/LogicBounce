using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class CoreSetup : MonoBehaviour{   public static CoreSetup instance;
    [ColorPalette("Abandon-Hyper")]
    public Color[] colorPalette;
    [Header("Main managers")]
    [AssetsOnly][SerializeField] GameObject saveSerialPrefab;
    [AssetsOnly][SerializeField] GameObject easySavePrefab;
    [AssetsOnly][SerializeField] GameObject gsceneManagerPrefab;
    [AssetsOnly][SerializeField] GameObject gameManagerPrefab;
    
    [Header("Assets managers")]
    [AssetsOnly][SerializeField] GameObject assetsManagerPrefab;
    [AssetsOnly][SerializeField] GameObject audioManagerPrefab;
    [AssetsOnly][SerializeField] GameObject jukeboxPrefab;
    [Header("GameSpecific")]
    [AssetsOnly][SerializeField] GameObject levelMapManagerPrefab;
    [AssetsOnly][SerializeField] GameObject stepsManagerPrefab;
    [AssetsOnly][SerializeField] Player playerPrefab;
    
    //[Header("Networking, Advancements etc")]
    //[AssetsOnly][SerializeField] GameObject dbaccessPrefab;
    //[AssetsOnly][SerializeField] GameObject discordRPCPrefab;
    void Awake(){
        instance=this;
        if(SceneManager.GetActiveScene().name=="Loading")LoadPre();
        else Load();
    }
    void LoadPre(){
        if(FindObjectOfType<SaveSerial>()==null){Instantiate(saveSerialPrefab);}
        if(FindObjectOfType<ES3ReferenceMgr>()==null){Instantiate(easySavePrefab);}
        if(FindObjectOfType<GSceneManager>()==null){var go=Instantiate(gsceneManagerPrefab);go.GetComponent<GSceneManager>().enabled=true;}/*Idk it disables itself? so I guess Ill turn it on manually*/
        //if(FindObjectOfType<DBAccess>()==null){Instantiate(dbaccessPrefab);}
        if(FindObjectOfType<AudioManager>()==null){Instantiate(audioManagerPrefab);}
    }
    void Load(){
        LoadPre();
        if(FindObjectOfType<GameManager>()==null){Instantiate(gameManagerPrefab);}

        if(FindObjectOfType<AssetsManager>()==null){Instantiate(assetsManagerPrefab);}

        //if(FindObjectOfType<DBAccess>()==null){Instantiate(dbaccessPrefab);}
        //#if (!UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_ANDROID && UNITY_EDITOR)
        //if(FindObjectOfType<DiscordPresence.PresenceManager>()==null){Instantiate(discordRPCPrefab);}
        
        if(FindObjectOfType<PostProcessVolume>()!=null&& FindObjectOfType<SaveSerial>().settingsData.pprocessing!=true){FindObjectOfType<PostProcessVolume>().enabled=false;}//Destroy(FindObjectOfType<PostProcessVolume>());}
        //if(FindObjectOfType<EventSystem>()!=null){if(FindObjectOfType<EventSystem>().GetComponent<UIInputSystem>()==null)FindObjectOfType<EventSystem>().gameObject.AddComponent<UIInputSystem>();}
        if(FindObjectOfType<Jukebox>()==null&&SceneManager.GetActiveScene().name=="Menu"){Instantiate(jukeboxPrefab);}
        
        if(FindObjectOfType<LevelMapManager>()==null&&LevelMapManager.InContextScene()){Instantiate(levelMapManagerPrefab);}
        if(FindObjectOfType<StepsManager>()==null&&SceneManager.GetActiveScene().name=="Game"){Instantiate(stepsManagerPrefab);}
    }

    public GameObject _getJukeboxPrefab(){return jukeboxPrefab;}
    public GameObject _levelMapManagerPrefab(){return levelMapManagerPrefab;}
    public Player _getPlayerPrefab(){return playerPrefab;}
}
