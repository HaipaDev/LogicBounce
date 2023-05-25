using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class LevelSelectCanvas : MonoBehaviour{
    [ChildGameObjectsOnly][SerializeField] RectTransform listContent;
    [SerializeField] Sprite spriteDefault;
    [SerializeField] Sprite spritePassed;
    void Start(){
        for(var i=0;i<LevelMapManager.instance.levelMaps.Length;i++){
            GameObject bt=listContent.GetChild(0).gameObject;
            if(i>0){
                bt=Instantiate(bt,listContent);bt.name="Level "+(i+1);
            }
            bt.GetComponent<Button>().onClick.RemoveAllListeners();
            bt.GetComponent<Button>().onClick.AddListener(delegate { LevelMapManager.instance.SetLevel(i);});
            bt.GetComponentInChildren<TextMeshProUGUI>().text=(i+1).ToString();
            if(SaveSerial.instance!=null){
                if(SaveSerial.instance.playerData!=null){
                    var sp=SaveSerial.instance.playerData;
                    if(sp.levelPassedValues.Length>i&&sp.levelPassedValues!=null){
                        if(sp.levelPassedValues[i]!=null){
                            if(sp.levelPassedValues[i].passed){
                                bt.GetComponent<Image>().sprite=spritePassed;
                                bt.transform.GetChild(1).gameObject.SetActive(true);
                            }else{
                                Default();
                            }
                        }else{Default();Debug.LogWarning("levelPassedValues["+i+"] is null!");}
                    }else{Default();Debug.LogWarning("levelPassedValues is null or shorter than the levelMaps list!");}
                    void Default(){
                        bt.GetComponent<Image>().sprite=spriteDefault;
                        bt.transform.GetChild(1).gameObject.SetActive(false);
                    }
                }else{Debug.LogWarning("SaveSerial.playerData = null!");}
            }else{Debug.LogWarning("SaveSerial = null!");}
        }
    }
}
