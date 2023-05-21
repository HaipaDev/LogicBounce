using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GSceneManager : MonoBehaviour{ public static GSceneManager instance;
    void Awake(){
        if(GSceneManager.instance!=null){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);}
    }
    void Update(){
        CheckESC();
    }
    
    public void LoadStartMenuLoader(){SceneManager.LoadScene("Menu");}
    public void LoadStartMenu(){
        SaveSerial.instance.Save();
        SceneManager.LoadScene("Menu");
        if(SceneManager.GetActiveScene().name=="Menu"){GameManager.instance.speedChanged=false;GameManager.instance.gameSpeed=1f;}
    }
    public void LoadStartMenuGame(){GSceneManager.instance.StartCoroutine(LoadStartMenuGameI());}
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
        
        //GameManager.instance.savableData.Save();
        //SaveSerial.instance.Save();
    }
    public void LoadGameScene(){
        SceneManager.LoadScene("Game");
        //GameManager.instance.ResetScore();
        GameManager.instance.gameSpeed=1f;
    }
    public void LoadOptionsScene(){SceneManager.LoadScene("Options");}
    public void LoadCreditsScene(){SceneManager.LoadScene("Credits");}
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
    }
    public void ReloadScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.speedChanged=false;
        GameManager.instance.gameSpeed=1f;
    }
    public void QuitGame(){Application.Quit();}
    public void RestartApp(){
        SceneManager.LoadScene("Loading");
        GameManager.instance.speedChanged=false;
        GameManager.instance.gameSpeed=1f;
    }
    public static bool EscPressed(){return Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Joystick1Button1);}
    void CheckESC(){    if(EscPressed()){
            var scene=SceneManager.GetActiveScene().name;
            if(scene==""){LoadStartMenu();}
    }}
    public static bool CheckScene(string name){return SceneManager.GetActiveScene().name==name;}
}
