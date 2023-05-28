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
    public void Win(){
        Won=true;
        var sp=SaveSerial.instance.playerData;
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].passed=true;
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankAchieved=CalculateRank();
        sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet=
            new LevelRankCritiria{energyUsedMax=StepsManager.instance.currentStepsEnergyUsed,timeToCompletion=LevelMapManager.instance.levelTimer};
        Debug.Log(sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankAchieved.ToString()+" | "+
            sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet.energyUsedMax.ToString()+" | "+
            sp.levelPassedValues[LevelMapManager.instance.levelCurrent].rankCriteriaMet.timeToCompletion.ToString());
        StartCoroutine(WinI());
    }
    IEnumerator WinI(){
        AssetsManager.instance.VFX("Victory",Vector2.zero,1f);
        AudioManager.instance.Play("Victory");
        yield return new WaitForSecondsRealtime(0.5f);
        blurChild.SetActive(true);
        victoryUI.SetActive(true);
        rankDisplay.gameObject.SetActive(true);
        rankDisplay.sprite=ranksSprites[(int)CalculateRank()];
        levelText.text="Level "+LevelMapManager.instance.levelCurrent+" completed!";
        StepsManager.instance.CloseStepsUI();

        Destroy(GameObject.Find("bg"));Destroy(GameObject.Find("bg2"));
        Destroy(FindObjectOfType<Spawnpoint>());
        Destroy(GameObject.Find("Level"));
        WorldCanvas.instance.Cleanup();
        Destroy(Player.instance.gameObject);

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
    public void ReplayLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadLevel(LevelMapManager.instance.levelCurrent);}
    public void LoadNextLevel(){SaveSerial.instance.Save();LevelMapManager.instance.LoadNextLevel();}
}
