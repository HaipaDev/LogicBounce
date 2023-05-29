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
        if(Won&&LevelMapManager.instance.levelCurrent==LevelMapManager.instance.levelMaps.Length-1&&StoryboardManager.IsOpen&&StoryboardManager.instance.finishedTyping){//Redirect on the last level
            if(Input.GetMouseButtonDown(0)){GSceneManager.instance.LoadWebsite("https://hypergamesdev.itch.io/chroma-bounce");StoryboardManager.instance.Close();}
        }
    }
    public void Win(){
        Won=true;
        var sp=SaveSerial.instance.playerData;
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].passed=true;
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankAchieved=CalculateRank();
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet=
            new LevelRankCritiria{rank=sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankAchieved,energyUsedMax=StepsManager.instance.currentStepsEnergyUsed,timeToCompletion=LevelMapManager.instance.levelTimer};
        //if(LevelMapManager.instance.levelCurrent==0){SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();}

        Debug.Log(sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankAchieved.ToString()+" | "+
            sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet.energyUsedMax.ToString()+" | "+
            sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet.timeToCompletion.ToString());
        SaveSerial.instance.Save();
        StartCoroutine(WinI());
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
        rankDisplay.sprite=ranksSprites[(int)CalculateRank()];
        levelText.text="Level "+(LevelMapManager.instance.levelCurrent+1)+" completed!";
        StepsManager.instance.CloseStepsUI();

        Destroy(GameObject.Find("bg"));Destroy(GameObject.Find("bg2"));
        Destroy(FindObjectOfType<Spawnpoint>());
        Destroy(GameObject.Find("Level"));
        WorldCanvas.instance.Cleanup();
        Destroy(Player.instance.gameObject);
        /*#if !UNITY_EDITOR
            if(LevelMapManager.instance.levelCurrent==0&&!SaveSerial.instance.playerData.firstLevelPassedInitial){
                SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
            }
        #endif*/
        //GSceneManager.instance.RelaunchTheGame();
        

        if(LevelMapManager.instance.levelCurrent==LevelMapManager.instance.levelMaps.Length-1){
            Debug.Log("Last level passed!");
            StoryboardManager.instance.TalkManual("Thank you for playing the 'demo' of my game I hope you enjoyed it!\n Please do Rate it and share your achieved ranks and solutions etc on itch <3",StoryboardTextType.tutorial,0.05f);
        }

        //yield return new WaitForSecondsRealtime(0.1f);
        //StepsManager.instance.OpenStepsUI();
    }
    LevelRankAchieved CalculateRank(){LevelRankAchieved r=LevelRankAchieved.S;
        var s=StepsManager.instance;
        var lm=LevelMapManager.instance;var l=lm.GetCurrentLevelMap();var lr=l.levelRankCritiria;
        for(var i=0;i<lr.Count;i++){
            //Debug.Log(i+" | "+r);
            if(s.currentStepsEnergyUsed>lr[i].energyUsedMax||lm.levelTimer>lr[i].timeToCompletion){
                r=(LevelRankAchieved)(i+1);
                //Debug.Log("Lowering rank "+r.ToString());
            }else{break;}
        }
        return r;
    }
    IEnumerator AfterFirstInitialLevel(){
        AudioManager.instance.Play("Glitch");
        yield return new WaitForSecondsRealtime(0.5f);//Because its before the VictoryVFX etc
        foreach(Button bt in GetComponentsInChildren<Button>()){bt.interactable=false;}
        yield return new WaitForSecondsRealtime(0.01f);
        SaveSerial.instance.playerData.firstLevelPassedInitial=true;SaveSerial.instance.Save();
        yield return new WaitForSecondsRealtime(0.04f);
        GSceneManager.instance.RelaunchTheGame();
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
    }
    public void ReplayLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadLevel(LevelMapManager.instance.levelCurrent);}
    public void LoadNextLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadNextLevel();}
}
