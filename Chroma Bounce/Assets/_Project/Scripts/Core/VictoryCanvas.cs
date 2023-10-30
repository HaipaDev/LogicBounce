using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class VictoryCanvas : MonoBehaviour{     public static VictoryCanvas instance;
    public static bool Won;
    [ChildGameObjectsOnly][SerializeField]GameObject victoryUI;
    [ChildGameObjectsOnly][SerializeField]GameObject blurChild;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI levelText;
    [ChildGameObjectsOnly][SerializeField]Image rankDisplay;
    [ChildGameObjectsOnly][SerializeField]GameObject nextButton;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI yourTimeText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sTimeText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI yourEnergyText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sEnergyText;
    [SerializeField]Sprite[] ranksSprites;
    void Start(){
        instance=this;Won=false;
        blurChild.SetActive(false);
        victoryUI.SetActive(false);
        rankDisplay.gameObject.SetActive(false);
    }
    void Update(){
        //#if !UNITY_EDITOR
        /*if(SaveSerial.instance!=null){
            if(SaveSerial.instance.playerData!=null){
                if(Won&&LevelMapManager.instance.levelCurrent==0&&!SaveSerial.instance.playerData.firstLevelPassedInitial){
                    foreach(Button bt in GetComponentsInChildren<Button>()){bt.interactable=false;}
                    if(Input.anyKeyDown)StartCoroutine(AfterFirstInitialLevel());
                }
            }else{Debug.LogError("SaveSerial.instance.playerData = null");SaveSerial.instance.RecreatePlayerData();}
        }else{Debug.LogError("SaveSerial.instance = null");}*/
        //#endif
        if(Won&&LevelMapManager.instance.levelCurrent==LevelMapManager.instance._levelMapsLength()-1&&StoryboardManager.IsOpen&&StoryboardManager.instance.finishedTyping){//Redirect on the last level
            if(Input.GetMouseButtonDown(0)){GSceneManager.instance.LoadWebsite("https://hypergamesdev.itch.io/chroma-bounce");StoryboardManager.instance.Close();}
        }
    }
    public void Win(){
        Won=true;
        var sp=SaveSerial.instance.playerData;
        var lm=LevelMapManager.instance;

        // if((sp.levelPassedValues.Count-1<lm.levelCurrent||sp.levelPassedValues[lm.levelCurrent]==null)||
        // (sp.levelPassedValues.Count>lm.levelCurrent&&sp.levelPassedValues[lm.levelCurrent]!=null&&!sp.levelPassedValues[lm.levelCurrent].passed)){
        //     if(LevelMapManager.instance.levelCurrent==LevelMapManager.instance._levelMapsLength()-1){
        //         Debug.Log("Last level passed!");
        //         StoryboardManager.instance.TalkManual("Thank you for playing the 'demo' of my game I hope you enjoyed it!\n Please do Rate it and share your achieved ranks and solutions etc on itch <3",StoryboardTextType.narrator,0.05f);
        //     }
        // }
        

        if(sp.levelPassedValues.Count-1<lm.levelCurrent||sp.levelPassedValues[lm.levelCurrent]==null){
            Debug.LogWarning("levelPassedValues incorrect!");
            for(int i=sp.levelPassedValues.Count;i<LevelMapManager.instance._levelMapsLength();i++){
                // sp.levelPassedValues[lm.levelCurrent]=new LevelPassValues();
                sp.levelPassedValues.Add(new LevelPassValues());
            }
        }

        if(!sp.levelPassedValues[lm.levelCurrent].passed){
            if(LevelMapManager.instance.levelCurrent==LevelMapManager.instance._levelMapsLength()-1){
                Debug.Log("Last level passed!");
                StoryboardManager.instance.TalkManual("Thank you for playing the 'demo' of my game I hope you enjoyed it!\n Please do Rate it and share your achieved ranks and solutions etc on itch <3",StoryboardTextType.narrator,0.05f);
            }
        }

        if(!sp.levelPassedValues[lm.levelCurrent].passed)sp.levelPassedValues[lm.levelCurrent].skipallDialogues=true;//Only overwrite before it is actually manually changed
        sp.levelPassedValues[lm.levelCurrent].passed=true;
        sp.levelPassedValues[lm.levelCurrent].rankAchieved=CalculateRankCurrent();
        sp.levelPassedValues[lm.levelCurrent].rankCriteriaMet=
            new LevelRankCritiria{rank=sp.levelPassedValues[lm.levelCurrent].rankAchieved,energyUsedMax=StepsManager.instance.currentStepsEnergyUsed,timeToCompletion=lm.levelTimer};
        //if(lm.levelCurrent==0){SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();}

        // Set text components
        var lr=lm.GetCurrentLevelMap().levelRankCritiria;
        yourTimeText.text=GameManager.FormatTimeSecMs(lm.levelTimer);
        sTimeText.text=GameManager.FormatTimeSecMs(lr[0].timeToCompletion);
        yourEnergyText.text=StepsManager.instance.currentStepsEnergyUsed.ToString();
        sEnergyText.text=lr[0].energyUsedMax.ToString();

        Debug.Log(sp.levelPassedValues[lm.levelCurrent].rankAchieved.ToString()+" | "+
            yourTimeText.text+" / "+
            sTimeText.text+
            " | "+
            yourEnergyText.text+" / "+
            sEnergyText.text
        );
        SaveSerial.instance.Save();
        VictoryCanvas.instance.StartCoroutine(VictoryCanvas.instance.WinI());
    }
    IEnumerator WinI(){
        if(SaveSerial.instance!=null){
            if(SaveSerial.instance.playerData!=null){
                if(Won&&LevelMapManager.instance.levelCurrent==0&&!SaveSerial.instance.playerData.firstLevelPassedInitial/*&&(Input.GetMouseButtonDown(0)||)*/){
                    StartCoroutine(AfterFirstInitialLevel());
                }
            }else{Debug.LogError("SaveSerial.instance.playerData = null");SaveSerial.instance.RecreatePlayerData();}
        }else{Debug.LogError("SaveSerial.instance = null");}

        Vector2 _vfxPos=Vector2.zero;
        if(FindObjectsOfType<LogicGate>().Length>0){_vfxPos=LevelMapManager.instance.logicGateListSorted.Find(x=>x.motherGate&&x.active).transform.position;}
        AssetsManager.instance.VFX("Victory",_vfxPos,1f);
        AudioManager.instance.Play("Victory");
        yield return new WaitForSecondsRealtime(0.5f);
        blurChild.SetActive(true);
        victoryUI.SetActive(true);
        rankDisplay.gameObject.SetActive(true);
        rankDisplay.sprite=ranksSprites[(int)CalculateRankCurrent()];
        levelText.text="Level "+(LevelMapManager.instance.levelCurrent+1)+" completed!";
        StepsManager.instance.CloseStepsUI();
        if(LevelMapManager.instance._nextLevelAvailable()){nextButton.SetActive(true);}else{nextButton.SetActive(false);}

        Destroy(GameObject.Find("bg"));Destroy(GameObject.Find("bg2"));
        Destroy(LevelMapManager.instance.levelParent);
        Destroy(FindObjectOfType<Spawnpoint>());
        Destroy(Player.instance.gameObject);
        WorldCanvas.instance.Cleanup();
        /*#if !UNITY_EDITOR
            if(LevelMapManager.instance.levelCurrent==0&&!SaveSerial.instance.playerData.firstLevelPassedInitial){
                SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
            }
        #endif*/
        //GSceneManager.instance.RelaunchTheGame();
        

        // if(LevelMapManager.instance.levelCurrent==LevelMapManager.instance._levelMapsLength()-1){
        //     Debug.Log("Last level passed!");
        //     StoryboardManager.instance.TalkManual("Thank you for playing the 'demo' of my game I hope you enjoyed it!\n Please do Rate it and share your achieved ranks and solutions etc on itch <3",StoryboardTextType.narrator,0.05f);
        // }

        //yield return new WaitForSecondsRealtime(0.1f);
        //StepsManager.instance.OpenStepsUI();
    }
    public static LevelRankAchieved CalculateRankCurrent(){
        var s=StepsManager.instance;
        var lm=LevelMapManager.instance;var l=lm.GetCurrentLevelMap();var lr=l.levelRankCritiria;
        return CalculateRank(s.currentStepsEnergyUsed,lm.levelTimer,lr);
    }
    public static LevelRankAchieved CalculateRank(int energyUsed,float levelTimer,List<LevelRankCritiria> levelRankCritiria){LevelRankAchieved r=LevelRankAchieved.S;
        for(var i=0;i<levelRankCritiria.Count;i++){
            //Debug.Log(i+" | "+r);
            if(energyUsed>levelRankCritiria[i].energyUsedMax||levelTimer>levelRankCritiria[i].timeToCompletion){
                r=(LevelRankAchieved)(i+1);
                //Debug.Log("Lowering rank "+r.ToString());
            }else{break;}
        }
        return r;
    }
    
    public static LevelRankAchieved CalculateTimeRankCurrent(){
        var lm=LevelMapManager.instance;var l=lm.GetCurrentLevelMap();var lr=l.levelRankCritiria;
        return CalculateTimeRank(lm.levelTimer,lr);
    }
    public static LevelRankAchieved CalculateTimeRank(float levelTimer,List<LevelRankCritiria> levelRankCritiria){LevelRankAchieved r=LevelRankAchieved.S;
        for(var i=0;i<levelRankCritiria.Count;i++){
            //Debug.Log(i+" | "+r);
            if(levelTimer>levelRankCritiria[i].timeToCompletion){
                r=(LevelRankAchieved)(i+1);
                //Debug.Log("Lowering time rank "+r.ToString());
            }else{break;}
        }
        return r;
    }
    public static LevelRankAchieved CalculateEnergyRankCurrent(){
        var s=StepsManager.instance;
        var lm=LevelMapManager.instance;var l=lm.GetCurrentLevelMap();var lr=l.levelRankCritiria;
        return CalculateEnergyRank(s.currentStepsEnergyUsed,lr);
    }
    public static LevelRankAchieved CalculateEnergyRank(int energyUsed,List<LevelRankCritiria> levelRankCritiria){LevelRankAchieved r=LevelRankAchieved.S;
        for(var i=0;i<levelRankCritiria.Count;i++){
            //Debug.Log(i+" | "+r);
            if(energyUsed>levelRankCritiria[i].energyUsedMax){
                r=(LevelRankAchieved)(i+1);
                //Debug.Log("Lowering energy rank "+r.ToString());
            }else{break;}
        }
        return r;
    }
    IEnumerator AfterFirstInitialLevel(){
        AudioManager.instance.Play("Glitch");
        foreach(Button bt in GetComponentsInChildren<Button>()){bt.interactable=false;}Destroy(victoryUI);
        SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();
        yield return new WaitForSecondsRealtime(0.5f);//Because its before the VictoryVFX etc
        GSceneManager.instance.RelaunchTheGame();
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
    }
    public void ReplayLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadLevel(LevelMapManager.instance.levelCurrent);}
    public void LoadNextLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadNextLevel();}
}
