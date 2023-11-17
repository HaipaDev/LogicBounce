using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class LevelSelectCanvas : MonoBehaviour{
    public static LevelSelectCanvas instance;
    [SerializeField] public bool expandedView=false;
    [ChildGameObjectsOnly][SerializeField] RectTransform compactListContent;
    [AssetsOnly][SerializeField] GameObject compactElementPrefab;
    [ChildGameObjectsOnly][SerializeField] RectTransform expandedListContent;
    [AssetsOnly][SerializeField] GameObject expandedElementPrefab;
    [ChildGameObjectsOnly][SerializeField] TextMeshProUGUI passedOutOfAllText;
    [SerializeField] public Sprite lockedSprite;
    [SerializeField] public Sprite[] ranksSprites;
    bool unlockedAll=false;
    void Awake() {
        instance=this;
    }
    void Start(){
        expandedView=SaveSerial.instance.settingsData.expandedLevelSelectLayout;
        SetupAll();
    }
    public void SetupAll(){
        SetupCompact();
        SetupExpanded();
        SetupPassedOutOfAll();
        expandedListContent.parent.gameObject.SetActive(false);
        compactListContent.parent.gameObject.SetActive(false);
        expandedListContent.parent.gameObject.SetActive(expandedView);
        compactListContent.parent.gameObject.SetActive(!expandedView);
    }
    void SetupCompact(){
        for(var c=compactListContent.childCount-1;c>=0;c--){Destroy(compactListContent.GetChild(c).gameObject);}//Clean up
        GameObject go=compactElementPrefab;
        for(var i=0;i<LevelMapManager.instance._levelMapsLength();i++){
            go=Instantiate(go,compactListContent);go.name="Level "+(i+1).ToString();
            go.GetComponentInChildren<TextMeshProUGUI>().text=(i+1).ToString();
            bool unlocked=false;
            if(SaveSerial.instance!=null){
                if(SaveSerial.instance.playerData!=null){
                    var sp=SaveSerial.instance.playerData;
                    if(sp.levelPassedValues!=null&&sp.levelPassedValues.Count>i){
                        if(sp.levelPassedValues[i]!=null){
                            unlocked=(i==0||(i>0&&(sp.levelPassedValues[i].passed||sp.levelPassedValues[i-1].passed||unlockedAll)));
                            //if(i>0)Debug.Log(unlocked+" "+i+" | "+sp.levelPassedValues[i-1].passed+" | "+unlockedAll);
                            if(sp.levelPassedValues[i].passed){
                                go.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareBlue");
                                go.transform.GetChild(1).gameObject.SetActive(true);
                                go.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite=ranksSprites[(int)sp.levelPassedValues[i].rankAchieved];
                                if((int)sp.levelPassedValues[i].rankAchieved==0){go.GetComponent<Image>().sprite=AssetsManager.instance.Spr("uiSquareYellow");}
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
                        go.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite=lockedSprite;
                        go.transform.GetChild(1).GetChild(0).localScale=Vector2.one;
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
    void SetupExpanded(bool _recursive=false){
        bool _reset=false;
        for(var c=expandedListContent.childCount-1;c>=0;c--){Destroy(expandedListContent.GetChild(c).gameObject);}//Clean up
        GameObject go=expandedElementPrefab;
        for(var i=0;i<LevelMapManager.instance._levelMapsLength();i++){
            go=Instantiate(go,expandedListContent);go.name="Level "+(i+1).ToString();
            go.GetComponent<LevelSelectElement>().id=i;
            if(SaveSerial.instance!=null){
                if(SaveSerial.instance.playerData!=null){
                    var sp=SaveSerial.instance.playerData;
                    if(sp.levelPassedValues!=null&&sp.levelPassedValues.Count>i){
                        if(sp.levelPassedValues[i]!=null){
                            go.GetComponent<LevelSelectElement>().unlocked=(i==0||(i>0&&(sp.levelPassedValues[i-1].passed||unlockedAll)));
                        }else{sp.levelPassedValues.Add(new LevelPassValues());_reset=true;}//sp.levelPassedValues[i]=new LevelPassValues();_reset=true;}
                    }else{sp.levelPassedValues.Add(new LevelPassValues());_reset=true;}//sp.levelPassedValues[i]=new LevelPassValues();_reset=true;}
                }else{Debug.LogWarning("SaveSerial.playerData = null!");}
            }else{Debug.LogWarning("SaveSerial = null!");}
            go.GetComponent<LevelSelectElement>().Setup();
        }
        if(_reset&&!_recursive){SetupExpanded(true);}
    }
    void SetupPassedOutOfAll(){
        int sRankLevels=0;
        int passedLevels=0;
        int allLevels=LevelMapManager.instance._levelMapsLength();

        if(SaveSerial.instance!=null){
            if(SaveSerial.instance.playerData!=null){
                if(SaveSerial.instance.playerData.levelPassedValues!=null){
                    var levelPassedValues=SaveSerial.instance.playerData.levelPassedValues;
                    if(levelPassedValues.Count>0){
                        for(int i=0;i<levelPassedValues.Count;i++){
                            if(levelPassedValues[i].passed){
                                passedLevels++;
                                if(levelPassedValues[i].rankAchieved==0){
                                    sRankLevels++;
                                }
                            }
                        }
                    }else{Debug.LogError("SaveSerial.instance.playerData.levelPassedValues Count <= 0 !");}
                }else{Debug.LogError("SaveSerial.instance.playerData.levelPassedValues = null");}
            }else{Debug.LogError("SaveSerial.instance.playerData = null");SaveSerial.instance.RecreatePlayerData();}
        }else{Debug.LogError("SaveSerial.instance = null");}

        if(passedOutOfAllText!=null){
            passedOutOfAllText.text=sRankLevels+" / "+passedLevels+" / "+allLevels;
        }else{Debug.LogWarning("passedOutOfAllText is null");}
    }
    public void SwitchLayout(){
        expandedView=!expandedView;
        SaveSerial.instance.settingsData.expandedLevelSelectLayout=expandedView;
        expandedListContent.parent.gameObject.SetActive(expandedView);
        compactListContent.parent.gameObject.SetActive(!expandedView);
    }
    public void SetLayout(bool isExpanded){
        expandedView=isExpanded;
        SaveSerial.instance.settingsData.expandedLevelSelectLayout=expandedView;
        expandedListContent.parent.gameObject.SetActive(expandedView);
        compactListContent.parent.gameObject.SetActive(!expandedView);
    }
    public void UnlockAll(){unlockedAll=true;SetupAll();}
}
