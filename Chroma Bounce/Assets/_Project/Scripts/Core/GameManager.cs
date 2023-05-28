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

    void Awake(){
        SetUpSingleton();
        instance=this;
        #if UNITY_EDITOR
        cheatmode=true;
        #else
        cheatmode=false;
        #endif
    }
    void SetUpSingleton(){int numberOfObj=FindObjectsOfType<GameManager>().Length;if(numberOfObj>1){Destroy(gameObject);}else{DontDestroyOnLoad(gameObject);}}
    void Start(){}
    void Update(){
        if(gameSpeed>=0){Time.timeScale=gameSpeed;}if(gameSpeed<=0){gameSpeed=0;}
        if(GSceneManager.CheckScene("Game")){
            if(Time.timeScale<=0.001f||PauseMenu.GameIsPaused||StepsManager.StepsUIOpen||VictoryCanvas.Won){GlobalTimeIsPaused=true;}else{GlobalTimeIsPaused=false;}
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

        CheckCodes(0,0);
    }
    public Volume PostProcessVolume(){return postProcessVolume;}
    public void SaveSettings(){SaveSerial.instance.SaveSettings();}
    public void Save(){ SaveSerial.instance.Save(); SaveSerial.instance.SaveSettings(); }
    public void Load(){ SaveSerial.instance.Load(); SaveSerial.instance.LoadSettings(); }
    public void DeleteAll(){ SaveSerial.instance.Delete(); /*ResetSettings();*/ GSceneManager.instance.LoadStartMenu();}
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
        else if(SceneManager.GetActiveScene().name=="Game"&&PauseMenu.GameIsPaused){if(FindObjectOfType<SettingsMenu>()!=null)FindObjectOfType<SettingsMenu>().Close();if(FindObjectOfType<PauseMenu>()!=null&&goToPause)FindObjectOfType<PauseMenu>().Pause();}
    }}
    public void ResetMusicPitch(){if(FindObjectOfType<Jukebox>()!=null)if(FindObjectOfType<Jukebox>().GetComponent<AudioSource>()!=null)FindObjectOfType<Jukebox>().GetComponent<AudioSource>().pitch=1;}

    public void CheckCodes(int fkey, int nkey){
        if(fkey==0&&nkey==0){}
        if(Input.GetKey(KeyCode.Delete) || fkey==-1){
            if(Input.GetKeyDown(KeyCode.Alpha0) || nkey==0){
                cheatmode=true;
            }if(Input.GetKeyDown(KeyCode.Alpha9) || nkey==9){
                cheatmode=false;
            }
        }
        if(cheatmode==true){
            if(Input.GetKey(KeyCode.F1) || fkey==1){
                if(Input.GetKeyDown(KeyCode.Alpha1) || nkey==1){}
            }
            if(Input.GetKey(KeyCode.F2) || fkey==2){
                if(Input.GetKeyDown(KeyCode.Alpha1) || nkey==1){}
            }
            if(Input.GetKey(KeyCode.F3) || fkey==3){
                if(Input.GetKeyDown(KeyCode.Alpha1) || nkey==1){}
            }
        }
    }
    public string FormatTime(float time){
        int minutes = (int) time / 60 ;
        int seconds = (int) time - 60 * minutes;
        //int milliseconds = (int) (1000 * (time - minutes * 60 - seconds));
    return string.Format("{0:00}:{1:00}"/*:{2:000}"*/, minutes, seconds/*, milliseconds*/ );
    }
    public string GetGameManagerTimeFormat(){
        return FormatTime(GameManagerTime);
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