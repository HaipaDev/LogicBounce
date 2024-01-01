using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class Loader : MonoBehaviour{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] bool loaded;
    [SerializeField] bool forceLoad;
    void Load(){
        var ss=SaveSerial.instance.settingsData;
        if(Application.platform==RuntimePlatform.Android){ss.pprocessing=false;ss.pauseWhenOOF=false;}
        if(Application.platform==RuntimePlatform.WebGLPlayer){ss.windowMode=2;}
        else{ss.pprocessing=true;}

        if(!loaded){
            SaveSerial.instance.Load();
            SaveSerial.instance.LoadSettings();
            loaded=true;
        }
        if(Application.platform!=RuntimePlatform.Android&&Application.platform!=RuntimePlatform.WebGLPlayer){Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);//Screen.fullScreen=ss.fullscreen;if(ss.fullscreen)Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,true,60);
        }if(Application.platform!=RuntimePlatform.Android&&Application.platform!=RuntimePlatform.WebGLPlayer){
            Screen.SetResolution(ss.resolution.x,ss.resolution.y,SettingsMenu.GetFullScreenMode(ss.windowMode));
        }
        //if(Application.platform==RuntimePlatform.WebGLPlayer){Screen.SetResolution(Screen.width,Screen.height,SettingsMenu.GetFullScreenMode(0));}
        //if (Application.platform == RuntimePlatform.Android)ss.moveByMouse=false;
        audioMixer.SetFloat("MasterVolume", ss.masterVolume);
        audioMixer.SetFloat("SoundVolume", ss.soundVolume);
        audioMixer.SetFloat("MusicVolume", ss.musicVolume);
        //#if !UNITY_EDITOR
            if(_firstInitialLevelLoader==null)_firstInitialLevelLoader=StartCoroutine(LoadFirstInitialLevel());
        //#endif
        //Jukebox.instance.SetMusicToCstmzMusic();
    }
    Coroutine _firstInitialLevelLoader;
    IEnumerator LoadFirstInitialLevel(){
        CoreSetup.instance.Load();
        yield return new WaitForSecondsRealtime(0.1f);
        if(!SaveSerial.instance.playerData.firstLevelPassedInitial){
            AudioManager.instance.Play("Glitch2");
            //GSceneManager.instance.LoadGameScene();}
            if(LevelMapManager.instance==null){Instantiate(CoreSetup.instance._levelMapManagerPrefab(),Vector2.zero,Quaternion.identity);}
            LevelMapManager.instance.LoadLevel(0);
        }
    }
    public void ForceLoad(){
        if(loaded)GSceneManager.instance.LoadStartMenuEmpty();
    }
    void Update(){
        Load();
        if(forceLoad){ForceLoad();}
    }
}
