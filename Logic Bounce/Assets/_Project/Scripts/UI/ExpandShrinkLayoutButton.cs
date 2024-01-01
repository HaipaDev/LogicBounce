using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class ExpandShrinkLayoutButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler{
    [SerializeField] Sprite shrinkSpr;
    [SerializeField] Sprite expandSpr;
    [SerializeField] Sprite compactSpr;
    [SerializeField] Sprite expandedSpr;
    bool isPointerOver=false;
    void Update(){
        if(isPointerOver){
            if(LevelSelectCanvas.instance.expandedView){
                GetComponentInChildren<TextMeshProUGUI>().text="Shrink";
                //GetComponentInChildren<Image>().sprite=shrinkSpr;
                transform.GetChild(0).GetComponent<Image>().sprite=shrinkSpr;
            }else{
                GetComponentInChildren<TextMeshProUGUI>().text="Expand";
                transform.GetChild(0).GetComponent<Image>().sprite=expandSpr;
            }
        }
        else{
            if(LevelSelectCanvas.instance.expandedView){
                GetComponentInChildren<TextMeshProUGUI>().text="Expanded Layout";
                transform.GetChild(0).GetComponent<Image>().sprite=expandedSpr;
            }else{
                GetComponentInChildren<TextMeshProUGUI>().text="Compact Layout";
                transform.GetChild(0).GetComponent<Image>().sprite=compactSpr;
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData){isPointerOver=true;}
    public void OnPointerExit(PointerEventData eventData){isPointerOver=false;}
}
