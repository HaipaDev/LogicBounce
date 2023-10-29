using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
public class GameManager : MonoBehaviour{   public static GameManager instance;
    public static bool GlobalTimeIsPaused;
    public static bool GlobalTimeIsPausedNotStepped;
    [Header("Global")]
    public bool smthOn=true;
    [Header("Current Player Values")]
    public int score = 0;
    [Header("Settings")]
    [Range(0.0f, 10.0f)] public float gameSpeed=1f;
    public float defaultGameSpeed=1f;
    public bool speedChanged;
    [Header("Other")]
    public string gameVersion="1.0";
    public float buildVersion=1f;
    public bool cheatmode;
    public bool dmgPopups=true;
    public bool analyticsOn=true;
    [SerializeField]float restartTimer=-4;
    Volume postProcessVolume;
    bool setValues;
    public float GameManagerTime=0;
    [Range(0,2)]public static int maskMode=1;
    public bool _webglFullscreenRequested=false;

    void Awake(){
        SetUpSingleton();
        #if UNITY_EDITOR
        cheatmode=true;
        #else
        cheatmode=false;
        #endif
    }
    void SetUpSingleton(){if(GameManager.instance!=null){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}}
    void Start(){}
    void Update(){
        if(gameSpeed>=0){Time.timeScale=gameSpeed;}if(gameSpeed<=0){gameSpeed=0;}
        if(GSceneManager.CheckScene("Game")){
            if(Time.timeScale<=0.001f||PauseMenu.GameIsPaused||StepsManager.StepsUIOpen||StoryboardManager.IsOpen||VictoryCanvas.Won){GlobalTimeIsPaused=true;}else{GlobalTimeIsPaused=false;}
            if((GlobalTimeIsPaused)&&!StepsManager.StepsUIOpen){GlobalTimeIsPausedNotStepped=true;}else{GlobalTimeIsPausedNotStepped=false;}
            //Debug.Log(GlobalTimeIsPaused+" | "+GlobalTimeIsPausedNotStepped);
        }else{GlobalTimeIsPaused=false;}

        if(SceneManager.GetActiveScene().name=="Game"&&!GlobalTimeIsPausedNotStepped&&!VictoryCanvas.Won){GameManagerTime+=Time.unscaledDeltaTime;}
        //Set speed to normal
        //if(!GlobalTimeIsPausedNotStepped&&(FindObjectOfType<Player>()!=null)&&speedChanged!=true){gameSpeed=defaultGameSpeed;}
        if(SceneManager.GetActiveScene().name!="Game"){gameSpeed=1;}
        //if(FindObjectOfType<Player>()==null){gameSpeed=defaultGameSpeed;}
        
        //Restart with R or Space/Resume with Space
        if(SceneManager.GetActiveScene().name=="Game"){
            if(GlobalTimeIsPausedNotStepped){if(restartTimer==-4)restartTimer=0.5f;}
            if(restartTimer>0)restartTimer-=Time.unscaledDeltaTime;
        }

        /*if(GlobalTimeIsPausedNotStepped){
            foreach(AudioSource sound in FindObjectsOfType<AudioSource>()){
                if(sound!=null){
                    //if(sound.gameObject!=Jukebox){
                    if(sound.gameObject.GetComponent<Jukebox>()==null){
                        //sound.pitch=1;
                        sound.Stop();
                    }
                }
            }
        }*/

        //Postprocessing
        if(SaveSerial.instance!=null){FindPostProcess();
        if(SaveSerial.instance.settingsData.pprocessing==true&&postProcessVolume!=null){postProcessVolume.GetComponent<Volume>().enabled=true;}
        if(SaveSerial.instance.settingsData.pprocessing==false&&FindObjectOfType<Volume>()!=null){FindPostProcess();postProcessVolume.GetComponent<Volume>().enabled=false;}
        void FindPostProcess(){if(postProcessVolume==null){postProcessVolume=Camera.main.gameObject.GetComponentInChildren<Volume>();}if(postProcessVolume==null){postProcessVolume=FindObjectOfType<Volume>();}}
        }

        if(Application.platform==RuntimePlatform.WebGLPlayer){
            if(!_webglFullscreenRequested){if(Input.GetMouseButtonDown(0)){StartCoroutine(TryForceFulscreen());_webglFullscreenRequested=true;return;}}
        }else{_webglFullscreenRequested=true;}
        /*if(Application.platform==RuntimePlatform.WebGLPlayer){
            if(!_webglFullscreenRequested){if(Input.GetMouseButtonDown(0)){_webglFullscreenRequested=true;_webglCanFullscreen=true;return;}}
            if(_webglCanFullscreen&&!_webglDidFullscreen){if(Input.GetMouseButtonDown(0)){Screen.SetResolution(Screen.width,Screen.height,SettingsMenu.GetFullScreenMode(0));_webglDidFullscreen=true;return;}}
            if(Input.GetKeyDown(KeyCode.F)){Screen.SetResolution(Screen.width,Screen.height,SettingsMenu.GetFullScreenMode(0));return;}
        }*/
    }
    IEnumerator TryForceFulscreen(){
        yield return new WaitForSeconds(0.2f);
        Screen.SetResolution(Screen.width,Screen.height,SettingsMenu.GetFullScreenMode(0));
    }
    public Volume PostProcessVolume(){return postProcessVolume;}
    public void SaveSettings(){SaveSerial.instance.SaveSettings();}
    public void Save(){ SaveSerial.instance.Save(); SaveSerial.instance.SaveSettings(); }
    public void Load(){ SaveSerial.instance.Load(); SaveSerial.instance.LoadSettings(); }
    public void DeleteAll(){
        SaveSerial.instance.Delete(); SaveSerial.instance.DeleteSettings();/*ResetSettings();*/ GSceneManager.instance.LoadStartMenu();
        if(Application.platform==RuntimePlatform.WebGLPlayer){PlayerPrefs.DeleteAll();SendMessage("ReloadWindow");}
    }
    public void ResetSettings(){
        SaveSerial.instance.DeleteSettings();
        GSceneManager.instance.ReloadScene();
        SaveSerial.instance.SaveSettings();
        var s=FindObjectOfType<SettingsMenu>();
    }
    float settingsOpenTimer;
    public void CloseSettings(bool goToPause){
    if(GameManager.instance!=null){
        if(SceneManager.GetActiveScene().name=="Options"){if(GSceneManager.instance!=null)GSceneManager.instance.LoadStartMenu();}
        else if(SceneManager.GetActiveScene().name=="Game"&&PauseMenu.GameIsPaused){if(FindObjectOfType<SettingsMenu>()!=null)FindObjectOfType<SettingsMenu>().Close();if(PauseMenu.instance!=null&&goToPause)PauseMenu.instance.Pause();}
    }}
    public void ResetMusicPitch(){if(Jukebox.instance!=null)if(Jukebox.instance.GetComponent<AudioSource>()!=null)Jukebox.instance.GetComponent<AudioSource>().pitch=1;}

    public static string FormatTimeMinSec(float time){
        int minutes = (int) time / 60 ;
        int seconds = (int) time - 60 * minutes;
        //int milliseconds = (int) (1000 * (time - minutes * 60 - seconds));
        return string.Format("{0:00}:{1:00}"/*:{2:000}"*/, minutes, seconds/*, milliseconds*/ );
    }
    public static string FormatTimeSecMs(float time,bool dot=false){
        int seconds = (int)time;
        int milliseconds = (int)((time - seconds) * 1000);
        
        if(dot){string.Format("{0}.{1:000}", seconds, milliseconds);}
        return string.Format("{0}:{1:000}", seconds, milliseconds);
    }
    public string GetGameManagerTimeFormat(){
        return FormatTimeMinSec(GameManagerTime);
    }public int GetGameManagerTime(){
        return Mathf.RoundToInt(GameManagerTime);
    }
    public void SetCheatmode(){if(!cheatmode){cheatmode=true;return;}else{cheatmode=false;return;}}
}

public enum dir{up,down,left,right}
public enum hAlign{left,right}
public enum vAlign{up,down}

public enum InputType{mouse,touch,keyboard,drag}
public enum PlaneDir{vert,horiz}

[System.Serializable]public class SpriteNColor{
    public Sprite spr;
    public Color color;
}
public enum LogicGateType{and,nand,xor,xnor,or,nor,not}