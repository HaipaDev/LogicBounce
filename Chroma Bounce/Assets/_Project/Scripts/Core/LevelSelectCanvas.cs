using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class LevelSelectCanvas : MonoBehaviour{
    [ChildGameObjectsOnly][SerializeField] RectTransform listContent;
    [SerializeField] Sprite lockedSprite;
    [SerializeField] Sprite[] ranksSprites;
    bool unlockedAll=false;
    void Start(){Setup();}
    void Setup(){
        GameObject go=listContent.GetChild(0).gameObject;
        if(listContent.childCount>1)for(var c=listContent.childCount-1;c>0;c--){Destroy(listContent.GetChild(c).gameObject);}
        for(var i=0;i<LevelMapManager.instance.levelMaps.Length;i++){
            if(i>0)go=Instantiate(go,listContent);go.name="Level "+(i+1).ToString();//Clone starting object
            go.GetComponentInChildren<TextMeshProUGUI>().text=(i+1).ToString();
            bool unlocked=false;
            if(SaveSerial.instance!=null){
                if(SaveSerial.instance.playerData!=null){
                    var sp=SaveSerial.instance.playerData;
                    if(sp.levelPassedValues!=null&&sp.levelPassedValues.Length>i){
                        if(sp.levelPassedValues[i]!=null){
                            unlocked=(i==0||(i>0&&(sp.levelPassedValues[i-1].passed||unlockedAll)));
                            //if(i>0)Debug.Log(unlocked+" "+i+" | "+sp.levelPassedValues[i-1].passed+" | "+unlockedAll);
                            if(sp.levelPassedValues[i].passed){
                                go.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareBlue");
                                go.transform.GetChild(1).gameObject.SetActive(true);
                                go.transform.GetChild(1).GetComponent<Image>().sprite=ranksSprites[0];
                            }else if(!unlocked){
                                Locked();
                            }else if(!sp.levelPassedValues[i].passed&&unlocked){
                                Default();
                            }
                        }else{Locked();Debug.LogWarning("levelPassedValues["+i+"] is null!");unlocked=false;}
                    }else{Locked();Debug.LogWarning("levelPassedValues is null or shorter than the levelMaps list!");unlocked=false;}
                    void Locked(){
                        go.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareRed");
                        go.transform.GetChild(1).gameObject.SetActive(true);
                        go.transform.GetChild(1).GetComponent<Image>().sprite=lockedSprite;
                    }
                    void Default(){
                        go.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquare");
                        go.transform.GetChild(1).gameObject.SetActive(false);
                    }
                }else{Debug.LogWarning("SaveSerial.playerData = null!");}
            }else{Debug.LogWarning("SaveSerial = null!");}
            var _index=i;
            go.GetComponent<Button>().onClick.RemoveAllListeners();
            if(i==0||unlocked)go.GetComponent<Button>().onClick.AddListener(delegate { LevelMapManager.instance.LoadLevel(_index);});
        }
    }
    public void UnlockAll(){unlockedAll=true;Setup();}
}
