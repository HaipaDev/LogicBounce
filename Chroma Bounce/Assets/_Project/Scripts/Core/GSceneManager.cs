using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GSceneManager : MonoBehaviour{ public static GSceneManager instance;
    void Awake(){if(GSceneManager.instance!=null){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}}
    void Update(){
        CheckESC();
    }
    
    public void LoadStartMenuEmpty(){SceneManager.LoadScene("Menu");OnChangeScene();}
    public void LoadStartMenu(){
        SaveSerial.instance.Save();
        SceneManager.LoadScene("Menu");
        if(SceneManager.GetActiveScene().name=="Menu"){GameManager.instance.speedChanged=false;GameManager.instance.gameSpeed=1f;}
        OnChangeScene();
    }
    public void LoadStartMenuGame(){
        if(SaveSerial.instance.playerData.firstLevelPassedInitial){GSceneManager.instance.StartCoroutine(GSceneManager.instance.LoadStartMenuGameI());}
        else{SaveSerial.instance.SaveSettings();Application.Quit();}
    }
    IEnumerator LoadStartMenuGameI(){
        if(SceneManager.GetActiveScene().name=="Game"){
            //GameManager.instance.SaveHighscore();
            yield return new WaitForSecondsRealtime(0.01f);
            //GameManager.instance.ResetScore();
        }
        yield return new WaitForSecondsRealtime(0.05f);
        SaveSerial.instance.Save();
        GameManager.instance.ResetMusicPitch();
        SceneManager.LoadScene("Menu");
        if(SceneManager.GetActiveScene().name=="Menu"){GameManager.instance.speedChanged=false;GameManager.instance.gameSpeed=1f;}
        OnChangeScene();
    }
    public void LoadLevelSelectScene(){
        //#if !UNITY_EDITOR
        if(SaveSerial.instance.playerData.secondLevelEntered&&SaveSerial.instance.playerData.firstLevelPassedInitial){
            SceneManager.LoadScene("LevelSelect");OnChangeScene();
        }else if(!SaveSerial.instance.playerData.secondLevelEntered){
            SaveSerial.instance.playerData.secondLevelEntered=true;SaveSerial.instance.Save();
            if(LevelMapManager.instance==null)Instantiate(CoreSetup.instance._levelMapManagerPrefab(),Vector2.zero,Quaternion.identity);
            LevelMapManager.instance.LoadLevel(1);
        }
        /*#else
        SceneManager.LoadScene("LevelSelect");OnChangeScene();
        #endif*/
    }
    public void LoadGameScene(){
        SceneManager.LoadScene("Game");
        if(LevelMapManager.instance!=null){if(LevelMapManager.instance.levelCurrent==0)GSceneManager.instance.StartCoroutine(GSceneManager.instance.LoadInitialFirstLevel());}
        //GameManager.instance.ResetScore();
        if(GameManager.instance!=null)GameManager.instance.gameSpeed=1f;
        OnChangeScene();
    }
    IEnumerator LoadInitialFirstLevel(){
        yield return new WaitForSecondsRealtime(0.4f);
        if(SaveSerial.instance!=null&&SaveSerial.instance.playerData!=null&&!SaveSerial.instance.playerData.firstLevelPassedInitial){
            LevelMapManager.instance.CallRestart(0.1f,false);LevelMapManager.instance.CallRestart(0.15f,true);
        }else{if(SaveSerial.instance==null||SaveSerial.instance.playerData==null){
            if(SaveSerial.instance!=null){Debug.LogError("SaveSerial.instance = null");}
            if(SaveSerial.instance.playerData!=null){Debug.LogError("SaveSerial.instance.playerData = null");SaveSerial.instance.RecreatePlayerData();}
            LevelMapManager.instance.CallRestart(0.1f,false);LevelMapManager.instance.CallRestart(0.15f,true);
        }}
    }
    public void LoadOptionsScene(){SceneManager.LoadScene("Options");OnChangeScene();}
    public void LoadCreditsScene(){SceneManager.LoadScene("Credits");OnChangeScene();}
    public void LoadWebsite(string url){Application.OpenURL(url);}
    public void RestartGame(){GSceneManager.instance.StartCoroutine(RestartGameI());}
    IEnumerator RestartGameI(){
        //GameManager.instance.SaveHighscore();
        yield return new WaitForSecondsRealtime(0.01f);
        //GameManager.instance.ResetScore();
        GameManager.instance.ResetMusicPitch();
        yield return new WaitForSecondsRealtime(0.05f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.speedChanged=false;
        GameManager.instance.gameSpeed=1f;
        OnChangeScene();
    }
    public void ReloadScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.speedChanged=false;
        GameManager.instance.gameSpeed=1f;
        OnChangeScene();
    }
    public void QuitGame(){Application.Quit();}
    public void RestartApp(){
        SceneManager.LoadScene("Loading");
        GameManager.instance.speedChanged=false;
        GameManager.instance.gameSpeed=1f;
        OnChangeScene();
    }
    public void RelaunchTheGame(){
        #if !UNITY_EDITOR
        Debug.Log("Relaunching the game...");
        string executablePath = Application.dataPath.Replace("_Data", "");//System.IO.Path.Combine(Application.dataPath.Replace("_Data", ""), "Chroma Bounce.exe");
        System.Diagnostics.Process.Start(executablePath);
        Application.Quit();
        #endif
    }
    public static bool EscPressed(){return Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Joystick1Button1);}
    void CheckESC(){    if(EscPressed()){
            var scene=SceneManager.GetActiveScene().name;
            if(CheckScene("LevelSelect")||CheckScene("Credits")){LoadStartMenu();}
    }}
    void OnChangeScene(){
        //if(LevelMapManager.instance!=null){if(LevelMapManager.OutOfContextScene()){Destroy(LevelMapManager.instance.gameObject);}SaveSerial.instance.RecreatePlayerData();}
    }
    public static bool CheckScene(string name){return SceneManager.GetActiveScene().name==name;}
}
