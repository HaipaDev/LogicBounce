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
        else{ss.pprocessing=true;}

        if(!loaded){
            SaveSerial.instance.Load();
            SaveSerial.instance.LoadSettings();
            loaded=true;
        }
        if(Application.platform!=RuntimePlatform.Android){Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);//Screen.fullScreen=ss.fullscreen;if(ss.fullscreen)Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,true,60);
        }if(Application.platform!=RuntimePlatform.Android){
            Screen.SetResolution(ss.resolution.x,ss.resolution.y,SettingsMenu.GetFullScreenMode(ss.windowMode));
        }
        //if (Application.platform == RuntimePlatform.Android)ss.moveByMouse=false;
        audioMixer.SetFloat("MasterVolume", ss.masterVolume);
        audioMixer.SetFloat("SoundVolume", ss.soundVolume);
        audioMixer.SetFloat("MusicVolume", ss.musicVolume);
        //Jukebox.instance.SetMusicToCstmzMusic();
    }
    public void ForceLoad(){
        if(GameObject.Find("IntroLong").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("IntroLongStart")){GameObject.Find("IntroLong").GetComponent<Animator>().SetBool("force",true);}
        else{if(loaded)GSceneManager.instance.LoadStartMenuLoader();}
    }
    void Update(){
        Load();
        if(forceLoad){ForceLoad();}
    }
}
