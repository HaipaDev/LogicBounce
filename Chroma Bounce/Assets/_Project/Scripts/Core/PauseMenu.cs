using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class PauseMenu : MonoBehaviour{     public static PauseMenu instance;
    public static bool GameIsPaused = false;
    [ChildGameObjectsOnly]public GameObject pauseMenuUI;
    [ChildGameObjectsOnly]public GameObject optionsUI;
    float unpausedTimer;
    float unpausedTimeReq=0.3f;
    //Shop shop;
    void Start(){
        instance=this;
        Resume();
        unpausedTimeReq=0;
        //shop=FindObjectOfType<Shop>();
    }
    void Update(){
        var _isEditor=false;
        #if UNITY_EDITOR
            _isEditor=true;
        #endif
        if((GSceneManager.EscPressed()||Input.GetKeyDown(KeyCode.Backspace)||Input.GetKeyDown(KeyCode.JoystickButton7))
        ||(!Application.isFocused&&!_isEditor&&SaveSerial.instance!=null&&SaveSerial.instance.settingsData.pauseWhenOOF)){
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
        if(optionsUI.transform.GetChild(0).gameObject.activeSelf){SettingsMenu.instance.Back();}
        //if(optionsUI.transform.GetChild(1).gameObject.activeSelf){optionsUI.GetComponent<SettingsMenu>().OpenSettings();}
        GameManager.instance.gameSpeed=1;
        GameIsPaused=false;
        //Debug.Log("Resuming pause");
    }
    public void PauseEmpty(){
        GameIsPaused=true;
        GameManager.instance.gameSpeed=0;
        unpausedTimer=-1;
        //Debug.Log("Pausing");
    }
    public void Pause(){
        pauseMenuUI.SetActive(true);
        PauseEmpty();
        //ParticleSystem.Stop();
        //var ptSystems = FindObjectOfType<ParticleSystem>();
        //foreach(ptSystem in ptSystems){ParticleSystem.Pause();}
    }
    
    public void OpenOptions(){
        optionsUI.GetComponent<SettingsMenu>().OpenSettings();
        pauseMenuUI.SetActive(false);
    }

    public void Restart(){
        GSceneManager.instance.RestartGame();
    }
    public void Quit(){
        GSceneManager.instance.LoadStartMenuGame();
    }

    public bool _isPausable(){
        var _isEditor=false;
        #if UNITY_EDITOR
            _isEditor=true;
        #endif
        return
        (!GameOverCanvas.instance.gameOver&&(unpausedTimer>=unpausedTimeReq||unpausedTimer==-1))&&
        ((Application.isFocused)||(!Application.isFocused&&!_isEditor&&SaveSerial.instance!=null&&SaveSerial.instance.settingsData.pauseWhenOOF));
    }
}