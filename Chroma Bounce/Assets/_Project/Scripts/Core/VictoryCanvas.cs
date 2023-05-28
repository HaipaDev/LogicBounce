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
    void Start(){
        instance=this;Won=false;
        blurChild.SetActive(false);
        victoryUI.SetActive(false);
    }
    public void Win(){
        Won=true;
        SaveSerial.instance.playerData.levelPassedValues[LevelMapManager.instance.levelCurrent].passed=true;
        StartCoroutine(WinI());
    }
    IEnumerator WinI(){
        AssetsManager.instance.VFX("Victory",Vector2.zero,1f);
        AudioManager.instance.Play("Victory");
        yield return new WaitForSecondsRealtime(0.5f);
        blurChild.SetActive(true);
        victoryUI.SetActive(true);
        StepsManager.instance.CloseStepsUI();

        Destroy(GameObject.Find("bg"));Destroy(GameObject.Find("bg2"));
        Destroy(FindObjectOfType<Spawnpoint>());
        Destroy(GameObject.Find("Level"));
        Destroy(Player.instance.gameObject);

        //yield return new WaitForSecondsRealtime(0.1f);
        //StepsManager.instance.OpenStepsUI();
    }
    public void LoadNextLevel(){LevelMapManager.instance.LoadNextLevel();}
}
