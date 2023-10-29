using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class LevelSelectElement : MonoBehaviour{
    [ReadOnly]public int id;
    [ReadOnly]public bool unlocked;
    [ChildGameObjectsOnly][SerializeField] Image borderSpr;
    [ChildGameObjectsOnly][SerializeField] Image previewImg;
    [ReadOnly][SerializeField] Texture2D previewTex=null;
    [ReadOnly][SerializeField] Sprite previewSpr=null;
    [ChildGameObjectsOnly][SerializeField] TextMeshProUGUI levelIdTxt;
    [ChildGameObjectsOnly][SerializeField] RectTransform rankSprParent;
    [ChildGameObjectsOnly][SerializeField] Image rankSpr;
    [ChildGameObjectsOnly][SerializeField] Button dropdownButton;
    [ChildGameObjectsOnly][SerializeField] RectTransform dropdownPanel;
    [ChildGameObjectsOnly][SerializeField] Button dropdownHideButton;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI yourTimeText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sTimeText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI yourEnergyText;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI sEnergyText;
    [ChildGameObjectsOnly][SerializeField]Toggle skipallDialoguesToggle;
    [ReadOnly][SerializeField]float _tempVolume;
    void Start(){
        Setup();
        CloseDropdown();
    }

    void Update(){}
    public void Setup(bool forceRegenPreview=false){
        levelIdTxt.text=(id+1).ToString();
        var l=LevelMapManager.instance.GetLevelMapFromList(id);
        var lr=l.levelRankCritiria;
        sTimeText.text=GameManager.FormatTimeSecMs(lr[0].timeToCompletion);
        sEnergyText.text=lr[0].energyUsedMax.ToString();
        GetComponent<Button>().enabled=unlocked;
        StartCoroutine(SetPreviewImage(forceRegenPreview));

        if(SaveSerial.instance!=null){
            if(SaveSerial.instance.playerData!=null){
                var sp=SaveSerial.instance.playerData;
                if(sp.levelPassedValues!=null&&sp.levelPassedValues.Count>id){
                    if(sp.levelPassedValues[id]!=null){
                        if(!unlocked)unlocked=(id==0||(id>0&&(sp.levelPassedValues[id-1].passed)));//Only double check after LevelSelectCanvas checks with inclusion of unlockedAll
                        //if(id>0)Debug.Log(unlocked+" "+i+" | "+sp.levelPassedValues[i-1].passed+" | "+unlockedAll);
                        skipallDialoguesToggle.isOn=sp.levelPassedValues[id].skipallDialogues;
                        if(sp.levelPassedValues[id].passed){
                            borderSpr.sprite=AssetsManager.instance.Spr("uiSquareBlue");
                            rankSprParent.gameObject.SetActive(true);
                            rankSpr.sprite=LevelSelectCanvas.instance.ranksSprites[(int)sp.levelPassedValues[id].rankAchieved];
                            if((int)sp.levelPassedValues[id].rankAchieved==0){borderSpr.sprite=AssetsManager.instance.Spr("uiSquareYellow");}
                            
                            yourTimeText.text=GameManager.FormatTimeSecMs(sp.levelPassedValues[id].rankCriteriaMet.timeToCompletion);
                            yourEnergyText.text=sp.levelPassedValues[id].rankCriteriaMet.energyUsedMax.ToString();
                        }else if(!unlocked){
                            Locked();
                        }else if(!sp.levelPassedValues[id].passed&&unlocked){
                            Default();
                        }
                    }else{Locked();Debug.LogWarning("levelPassedValues["+id+"] is null!");}
                }else{Locked();Debug.LogWarning("levelPassedValues is null or shorter than the levelMaps list!");}
                void Locked(){
                    unlocked=false;
                    skipallDialoguesToggle.interactable=false;
                    borderSpr.sprite=AssetsManager.instance.Spr("uiSquareRed");
                    rankSprParent.gameObject.SetActive(true);
                    rankSpr.sprite=LevelSelectCanvas.instance.lockedSprite;
                    rankSpr.transform.localScale=Vector2.one;
                }
                void Default(){
                    borderSpr.sprite=AssetsManager.instance.Spr("uiSquare");
                    rankSprParent.gameObject.SetActive(false);
                }
            }else{Debug.LogWarning("SaveSerial.playerData = null!");}
        }else{Debug.LogWarning("SaveSerial = null!");}
    }
    public void StartLevel(){
        LevelMapManager.instance.LoadLevel(id);
        if(_tempVolume!=0)SaveSerial.instance.settingsData.soundVolume=_tempVolume;//Revert sound to previous volume if all fails
    }
    public void OpenDropdown(){
        dropdownPanel.gameObject.SetActive(true);
        dropdownButton.gameObject.SetActive(false);
        dropdownHideButton.gameObject.SetActive(true);
        dropdownHideButton.GetComponent<Image>().maskable=false;
    }
    public void CloseDropdown(){
        dropdownPanel.gameObject.SetActive(false);
        dropdownButton.gameObject.SetActive(true);
        dropdownHideButton.gameObject.SetActive(false);
    }
    // IEnumerator SetPreviewImage(bool forceRegenPreview=false){
    //     _tempVolume=SaveSerial.instance.settingsData.soundVolume;
    //     // yield return new WaitForSecondsRealtime(1.05f+(0.05f*id));
    //     yield return new WaitForSecondsRealtime(0.1f*(id+1));
    //     // var _waitingTime=0.1f;
    //     var _waitingTime=0.5f;
    //     var l=LevelMapManager.instance.GetLevelMapFromList(id);
    //     if(previewSpr==null){
    //         if(AssetsManager.instance.generatedLevelPreviews!=null){
    //             if(AssetsManager.instance.generatedLevelPreviews.Length>id){
    //                 if(AssetsManager.instance.generatedLevelPreviews[id]!=null){
    //                     previewSpr=AssetsManager.instance.generatedLevelPreviews[id];
    //                     previewImg.sprite=previewSpr;
    //                 }
    //             }else{AssetsManager.instance.generatedLevelPreviews=new Sprite[LevelMapManager.instance._levelMapsLength()];Debug.Log("Initializing generatedLevelPreviews array");}
    //         }else{AssetsManager.instance.generatedLevelPreviews=new Sprite[LevelMapManager.instance._levelMapsLength()];Debug.Log("Initializing generatedLevelPreviews array");}
    //         if(previewSpr==null||forceRegenPreview){//After initial try of fetching from array
    //             StartCoroutine(AssetsManager.instance.CaptureScreenshotCoroutine(l.parent,result => {
    //                 if(result != null) {
    //                     // Debug.Log("result: " + result);
    //                     previewSpr=result;
    //                     if(previewSpr!=null){
    //                         previewImg.sprite=previewSpr;
    //                     }else{
    //                         Debug.LogWarning("previewSpr = null in coroutine");
    //                     }
    //                 }else{
    //                     Debug.LogWarning("result = null in coroutine");
    //                 }
    //             },100*id,0f,_waitingTime));
    //             yield return new WaitForSecondsRealtime(_waitingTime*2.5f);
    //             // previewSpr=AssetsManager.PrefabSnapshotSpr(l.parent);
    //             if(previewSpr!=null){
    //                 previewImg.sprite=previewSpr;
    //                 AssetsManager.instance.generatedLevelPreviews[id]=previewSpr;
    //             }else{
    //                 Debug.LogWarning("previewSpr = null");
    //             }
    //             if(id==LevelMapManager.instance._levelMapsLength()-1){if(WorldCanvas.instance!=null){Destroy(WorldCanvas.instance.gameObject);}}

    //             // var previewTex=UnityEditor.AssetPreview.GetAssetPreview(l.parent);
    //             // if(previewTex!=null){
    //             //     previewSpr=Sprite.Create(previewTex,new Rect(0.0f, 0.0f, previewTex.width, previewTex.height),new Vector2(0.5f, 0.5f), 100f);
    //             //     previewImg.sprite=previewSpr;
    //             // }else{
    //             //     Debug.LogWarning("previewTex = null");
    //             // }
    //         }
    //     }
    // }
    IEnumerator SetPreviewImage(bool forceRegenPreview=false){
        _tempVolume=SaveSerial.instance.settingsData.soundVolume;
        yield return new WaitForSecondsRealtime(0.01f*(id+1));
        // yield return new WaitForSecondsRealtime(0.1f*(id+1));
        // var _waitingTime=0.1f;
        var _waitingTime=0.5f;
        var l=LevelMapManager.instance.GetLevelMapFromList(id);
        if(previewSpr==null){
            if(previewTex!=null){previewSpr=AssetsManager.TextureToSprite(previewTex);}
            
            if(AssetsManager.instance.generatedLevelPreviews!=null){
                if(AssetsManager.instance.generatedLevelPreviews.Length>id){
                    if(AssetsManager.instance.generatedLevelPreviews[id]!=null){
                        previewTex=AssetsManager.instance.generatedLevelPreviews[id];
                        if(previewTex!=null&&previewSpr==null){
                            previewSpr=AssetsManager.TextureToSprite(previewTex);
                        }
                        previewImg.sprite=previewSpr;
                    }
                }else{AssetsManager.instance.generatedLevelPreviews=new Texture2D[LevelMapManager.instance._levelMapsLength()];Debug.Log("Initializing generatedLevelPreviews array");}
            }else{AssetsManager.instance.generatedLevelPreviews=new Texture2D[LevelMapManager.instance._levelMapsLength()];Debug.Log("Initializing generatedLevelPreviews array");}

            if(previewSpr==null||forceRegenPreview){//After initial try of fetching from array, create new textures
                yield return new WaitForSecondsRealtime(0.1f*(id+1));
                StartCoroutine(AssetsManager.instance.CaptureScreenshotCoroutine(l.parent,result => {
                    if(result != null){
                        // Debug.Log("result: " + result);
                        if(previewTex!=null){Destroy(previewTex);}
                        previewTex=result;
                        if(previewTex!=null&&previewSpr==null){
                            previewSpr=AssetsManager.TextureToSprite(previewTex);
                            AssetsManager.instance.generatedLevelPreviews[id]=previewTex;
                        }
                        if(previewSpr!=null){
                            previewImg.sprite=previewSpr;
                        }else{
                            Debug.LogWarning("previewSpr = null in coroutine");
                        }
                    }else{
                        Debug.LogWarning("result = null in coroutine");
                    }
                },100*id,0f,_waitingTime));
                yield return new WaitForSecondsRealtime(_waitingTime*2.5f);
                // previewSpr=AssetsManager.PrefabSnapshotSpr(l.parent);
                if(previewTex!=null&&previewSpr==null){
                    previewSpr=AssetsManager.TextureToSprite(previewTex);
                    AssetsManager.instance.generatedLevelPreviews[id]=previewTex;
                    // AssetsManager.instance.generatedLevelPreviews[id]=previewSpr;
                }
                if(previewSpr!=null){
                    previewImg.sprite=previewSpr;
                    // AssetsManager.instance.generatedLevelPreviews[id]=previewSpr;
                }else{
                    Debug.LogWarning("previewSpr = null");
                }
                if(id==LevelMapManager.instance._levelMapsLength()-1){if(WorldCanvas.instance!=null){Destroy(WorldCanvas.instance.gameObject);}}
            }
        }
    }
    public void SkipallDialoguesSwitchToggle(bool isOn){
        if(SaveSerial.instance!=null){
            if(SaveSerial.instance.playerData!=null){
                var sp=SaveSerial.instance.playerData;
                if(sp.levelPassedValues!=null&&sp.levelPassedValues.Count>id){
                    if(sp.levelPassedValues[id]!=null){
                        sp.levelPassedValues[id].skipallDialogues=isOn;
                    }else{Debug.LogWarning("levelPassedValues["+id+"] is null!");unlocked=false;}
                }else{Debug.LogWarning("levelPassedValues is null or shorter than the levelMaps list!");unlocked=false;}
            }else{Debug.LogWarning("SaveSerial.playerData = null!");}
        }else{Debug.LogWarning("SaveSerial = null!");}
    }
    void OnDestroy(){
        // if(previewTex!=null)Destroy(previewTex);
        if(previewSpr!=null)Destroy(previewSpr);
    }
}
