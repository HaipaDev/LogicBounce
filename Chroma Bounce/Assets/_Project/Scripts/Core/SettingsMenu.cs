using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using TMPro;
using Sirenix.OdinInspector;

public class SettingsMenu : MonoBehaviour{      public static SettingsMenu instance;
    [SerializeField] int panelActive=0;
    [SerializeField] GameObject[] panels;
    [Title("Sound")]
    public AudioMixer audioMixer;
    [SceneObjectsOnly][SerializeField]Slider masterSlider;
    [SceneObjectsOnly][SerializeField]Slider masterOOFSlider;
    [SceneObjectsOnly][SerializeField]Slider soundSlider;
    [SceneObjectsOnly][SerializeField]Slider musicSlider;
    
    [Title("Graphics")]
    [SceneObjectsOnly][SerializeField]TMP_Dropdown windowModeDropdown;
    [SceneObjectsOnly][SerializeField]TMP_Dropdown resolutionDropdown;
    //[SceneObjectsOnly][SerializeField]Toggle fullscreenToggle;
    [SceneObjectsOnly][SerializeField]Toggle vSyncToggle;
    [SceneObjectsOnly][SerializeField]Toggle lockCursorToggle;
    [SceneObjectsOnly][SerializeField]Toggle pauseWhenOOFToggle;
    [SceneObjectsOnly][SerializeField]TMP_Dropdown qualityDropdopwn;
    [SceneObjectsOnly][SerializeField]Toggle pprocessingToggle;
    [SceneObjectsOnly][SerializeField]Toggle screenshakeToggle;
    [SceneObjectsOnly][SerializeField]Toggle discordRPCToggle;

    [AssetsOnly][SerializeField]VolumeProfile pprocessingPrefab;
    SaveSerial.SettingsData settingsData;
    void Start(){   instance=this;if(SaveSerial.instance!=null){settingsData=SaveSerial.instance.settingsData;}

        masterSlider.value=settingsData.masterVolume;
        masterOOFSlider.value=settingsData.masterOOFVolume;
        soundSlider.value=settingsData.soundVolume;
        musicSlider.value=settingsData.musicVolume;

        SetAvailableResolutions();
        windowModeDropdown.value=settingsData.windowMode;
        //fullscreenToggle.isOn=settingsData.fullscreen;
        vSyncToggle.isOn=settingsData.vSync;
        pauseWhenOOFToggle.isOn=settingsData.pauseWhenOOF;
        lockCursorToggle.isOn=settingsData.lockCursor;
        pprocessingToggle.isOn=settingsData.pprocessing;
        screenshakeToggle.isOn=settingsData.screenshake;
        discordRPCToggle.isOn=settingsData.discordRPC;

        if(SceneManager.GetActiveScene().name=="Options")OpenSettings();
        SetPanelActive(0);
        foreach(Scrollbar sc in GetComponentsInChildren<Scrollbar>()){sc.value=1;}
    }
    void Update(){
        if(settingsData.masterVolume<=-50){settingsData.masterVolume=-80;}
        if(settingsData.soundVolume<=-50){settingsData.soundVolume=-80;}
        if(settingsData.musicVolume<=-50){settingsData.musicVolume=-80;}

        if(GSceneManager.EscPressed()){Back();}
    }
    public void SetPanelActive(int i){panelActive=i;foreach(GameObject p in panels){p.SetActive(false);}panels[panelActive].SetActive(true);}
    public void OpenSettings(){transform.GetChild(0).gameObject.SetActive(true);transform.GetChild(1).gameObject.SetActive(false);}
    public void OpenDeleteAll(){transform.GetChild(1).gameObject.SetActive(true);transform.GetChild(0).gameObject.SetActive(false);}
    public void Close(){SaveSerial.instance.SaveSettings();transform.GetChild(0).gameObject.SetActive(false);transform.GetChild(1).gameObject.SetActive(false);}
    public void Back(){
        if(transform.GetChild(1).gameObject.activeSelf){OpenSettings();return;}
        else{
            if(SceneManager.GetActiveScene().name=="Options"){GSceneManager.instance.LoadStartMenu();SaveSerial.instance.SaveSettings();}
            else if(SceneManager.GetActiveScene().name=="Game"&&PauseMenu.GameIsPaused){Close();PauseMenu.instance.Pause();}
        }
    }


#region//Game
    public void SetDiscordRPC(bool isOn){settingsData.discordRPC=isOn;}
#endregion


#region//Sound
    public void SetMasterVolume(float val){settingsData.masterVolume=(float)System.Math.Round(val,2);}
    public void SetMasterOOFVolume(float val){settingsData.masterOOFVolume=(float)System.Math.Round(val,2);}
    public void SetSoundVolume(float val){settingsData.soundVolume=(float)System.Math.Round(val,2);}
    public void SetMusicVolume(float val){settingsData.musicVolume=(float)System.Math.Round(val,2);}
#endregion


#region//Graphics
    public static FullScreenMode GetFullScreenMode(int id){
        switch(id){
            case 0:return FullScreenMode.ExclusiveFullScreen;
            case 1:return FullScreenMode.FullScreenWindow;
            case 2:return FullScreenMode.Windowed;
            default:return FullScreenMode.ExclusiveFullScreen;
        }
    }
    public static int GetFullScreenModeID(FullScreenMode fullScreenMode){
        switch(fullScreenMode){
            case FullScreenMode.ExclusiveFullScreen:return 0;
            case FullScreenMode.FullScreenWindow:return 1;
            case FullScreenMode.Windowed:return 2;
            default:return 0;
        }
    }
    public void SetWindowMode(int id){
        Screen.SetResolution(settingsData.resolution.x,settingsData.resolution.y,GetFullScreenMode(settingsData.windowMode));
        settingsData.windowMode=id;
    }
    public void SetVSync(bool isOn){
        QualitySettings.vSyncCount=AssetsManager.BoolToInt(isOn);
        settingsData.vSync=isOn;
    }
    public void SetLockCursor(bool isOn){
        Cursor.lockState=(CursorLockMode)(AssetsManager.BoolToInt(isOn)*2);
        settingsData.lockCursor=isOn;
    }
    public void SetPauseWhenOOF(bool isOn){
        settingsData.pauseWhenOOF=isOn;
    }
    public void SetResolution(int resIndex){
        Vector2Int _res=_availableResolutionsList[resIndex];
        Screen.SetResolution(_res.x,_res.y,GetFullScreenMode(settingsData.windowMode));
        settingsData.resolution=_res;
    }
    public void SetPostProcessing(bool isOn){
        settingsData.pprocessing=isOn;
        //if(isOn==true&&GameManager.instance.PostProcessVolume()==null){GameManager.instance.PostProcessVolume()=Instantiate(pprocessingPrefab,Camera.main.transform).GetComponent<Volume>();}//FindObjectOfType<Level>().RestartScene();}
        if(isOn==true&&GameManager.instance.PostProcessVolume()!=null){GameManager.instance.PostProcessVolume().enabled=true;}
        if(isOn==false&&FindObjectOfType<Volume>()!=null){FindObjectOfType<Volume>().enabled=false;}//Destroy(FindObjectOfType<Volume>());}
    }
    public void SetScreenshake(bool isOn){settingsData.screenshake=isOn;}
#endregion
    public void ResetSettings(){GameManager.instance.ResetSettings();}
    

    public void PlayDing(){if(Application.isPlaying)GetComponent<AudioSource>().Play();}
    public void PlayDingOOF(){audioMixer.SetFloat("MasterVolume", AssetsManager.InvertNormalizedMin(settingsData.masterOOFVolume,-50));if(Application.isPlaying)GetComponent<AudioSource>().Play();}

    void SetAvailableResolutions(){
        _availableResolutionsList.Clear();
        resolutionDropdown.ClearOptions();
        foreach(Vector2Int r in resolutionsList){
            if(Display.main.systemWidth>Display.main.systemHeight){//Horizontal
                if(r.x>r.y){
                    if(r.x<=Display.main.systemWidth&&r.y<=Display.main.systemHeight){
                        _availableResolutionsList.Add(r);
                        resolutionDropdown.AddOptions(new List<string>(){(r.x+"x"+r.y)});
                    }
                }
            }else{//Vertical
                if(r.y>r.x){
                    if(r.x<=Display.main.systemWidth&&r.y<=Display.main.systemHeight){
                        _availableResolutionsList.Add(r);
                        resolutionDropdown.AddOptions(new List<string>(){(r.x+"x"+r.y)});
                    }
                }
            }
        }
        if(_availableResolutionsList.Count==0){foreach(Vector2Int r in forcedResolutionsList){
            _availableResolutionsList.Add(r);
            resolutionDropdown.AddOptions(new List<string>(){(r.x+"x"+r.y)});
        }}

        Vector2Int _currentRes=SaveSerial.instance.settingsData.resolution;
        if(!_availableResolutionsList.Contains(_currentRes)){
            _availableResolutionsList.Insert(0,_currentRes);
            List<TMP_Dropdown.OptionData> _optionsCache=new List<TMP_Dropdown.OptionData>();
            for(var i=0;i<resolutionDropdown.options.Count;i++){_optionsCache.Add(resolutionDropdown.options[i]);}//Cache old options

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(new List<string>(){(_currentRes.x+"x"+_currentRes.y)});//Add current on top
            resolutionDropdown.AddOptions(_optionsCache);
        }

        resolutionDropdown.value=_availableResolutionsList.FindIndex(e=>e==settingsData.resolution);
    }
    [DisableInEditorMode]public List<Vector2Int> _availableResolutionsList;
    [HideInEditorMode]public List<Vector2Int> resolutionsList=new List<Vector2Int>(){
        //16:9
        new Vector2Int(3840,2160),
        new Vector2Int(2560,1440),
        new Vector2Int(1920,1080),
        new Vector2Int(1600,900),
        new Vector2Int(1366,768),
        new Vector2Int(1280,720),
        
        //16:10
        new Vector2Int(1920,1200),
        new Vector2Int(1680,1050),
        new Vector2Int(1440,900),
        new Vector2Int(1280,800),

        //4:3
        new Vector2Int(1024,768),
        new Vector2Int(800,600),
        new Vector2Int(640,480),

        //3:2
        new Vector2Int(2160,1440),
        new Vector2Int(1440,960),

        //21:9
        new Vector2Int(2560,1080),

        //9:16
        new Vector2Int(1080,1920),
        new Vector2Int(720,1280),
    };
    [HideInEditorMode]public List<Vector2Int> forcedResolutionsList=new List<Vector2Int>(){
        new Vector2Int(1920,1080),
        new Vector2Int(640,480),
        new Vector2Int(1080,1920),
    };
}
