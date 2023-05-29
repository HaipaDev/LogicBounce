using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class PauseMenu : MonoBehaviour{     public static PauseMenu instance;
    public static bool GameIsPaused = false;
    [ChildGameObjectsOnly]public GameObject pauseMenuUI;
    [ChildGameObjectsOnly]public GameObject optionsUI;
    [ChildGameObjectsOnly]public GameObject blurChild;
    float unpausedTimer;
    float unpausedTimeReq=0.3f;
    //Shop shop;
    void Start(){
        instance=this;
        Resume();
        unpausedTimeReq=0;
    }
    void Update(){
        var _isEditor=false;
        #if UNITY_EDITOR
            _isEditor=true;
        #endif
        if(((GSceneManager.EscPressed())//||Input.GetKeyDown(KeyCode.Backspace)||Input.GetKeyDown(KeyCode.JoystickButton7))
        ||(!Application.isFocused&&!_isEditor&&SaveSerial.instance!=null&&SaveSerial.instance.settingsData.pauseWhenOOF))
        &&(UIInputSystem.instance.currentSelected==null||(UIInputSystem.instance.currentSelected!=null&&UIInputSystem.instance.currentSelected.GetComponent<TMPro.TextMeshProUGUI>()!=null))){
            if(GameIsPaused){
                if(Application.isFocused){
                    if(pauseMenuUI.activeSelf){Resume();return;}
                    if(optionsUI.transform.GetChild(0).gameObject.activeSelf){SaveSerial.instance.SaveSettings();pauseMenuUI.SetActive(true);return;}
                    if(optionsUI.transform.GetChild(1).gameObject.activeSelf){optionsUI.GetComponent<SettingsMenu>().OpenSettings();PauseEmpty();return;}
                }
            }else{
                if(_isPausable()){Pause();}
            }
        }//if(Input.GetKeyDown(KeyCode.R)){//in GameManager}
        if(!GameIsPaused){
            if(unpausedTimer==-1)unpausedTimer=0;
            unpausedTimer+=Time.unscaledDeltaTime;
        }
    }
    public void Resume(){
        pauseMenuUI.SetActive(false);
        blurChild.SetActive(false);
        if(optionsUI.transform.childCount>0)if(optionsUI.transform.GetChild(0).gameObject.activeSelf){if(SettingsMenu.instance!=null)SettingsMenu.instance.Back();}
        GameManager.instance.gameSpeed=1;
        GameIsPaused=false;
    }
    public void PauseEmpty(){
        GameIsPaused=true;
        GameManager.instance.gameSpeed=0;
        unpausedTimer=-1;
    }
    public void Pause(){
        pauseMenuUI.SetActive(true);
        blurChild.SetActive(true);
        PauseEmpty();
    }
    
    public void OpenOptions(){
        optionsUI.GetComponent<SettingsMenu>().OpenSettings();
        pauseMenuUI.SetActive(false);
    }
    public void Quit(){
        GSceneManager.instance.LoadStartMenuGame();
    }

    public bool _isPausable(){
        var _isEditor=false;
        #if UNITY_EDITOR
            _isEditor=true;
        #endif
        bool _firstLevel=SaveSerial.instance!=null&&SaveSerial.instance.playerData!=null&&SaveSerial.instance.playerData.firstLevelPassedInitial;
        bool _pauseWhenOOF=SaveSerial.instance!=null&&SaveSerial.instance.settingsData!=null&&SaveSerial.instance.settingsData.pauseWhenOOF;
        return(
        (!VictoryCanvas.Won)&&//(_firstLevel==true)&&
        ((unpausedTimer>=unpausedTimeReq||unpausedTimer==-1))&&
        ((Application.isFocused)||(!Application.isFocused&&!_isEditor&&_pauseWhenOOF))
        );
    }
}