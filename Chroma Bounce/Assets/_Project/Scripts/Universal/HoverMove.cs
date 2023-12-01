using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverMove : MonoBehaviour{
    [SerializeField] float speed=1f;//Its like 100 times more for UI elements
    [SerializeField] Vector2 offset;
    [HideInEditorMode][SerializeField] Vector2 origin;
    RectTransform rt;
    void Start(){
        if(GetComponent<RectTransform>()!=null){
            rt=GetComponent<RectTransform>();
            origin=(Vector2)rt.anchoredPosition;
        }else{
            origin=(Vector2)transform.position;
        }
    }
    void Update(){
        Vector2 _targetPos=origin;
        if(IsPointerOverUIElement()){
            _targetPos=origin+offset;
        }//else just leave it at origin

        if(GetComponent<RectTransform>()==null){//If not a UI element
            float _delta=Time.deltaTime*speed;
            transform.position=Vector2.MoveTowards(transform.position,_targetPos,_delta);
        }else{
            float _delta=Time.deltaTime*speed;
            rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,_targetPos,_delta);
        }
    }
    bool CheckAllOtherPointers(){
        if(Array.Exists(FindObjectsOfType<HoverUIEnable>(),x=>x.IsPointerOverUIElement()&&x.name==name)){return true;}
        else{return false;}
    }
    ///Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement(){
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    ///Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults){
        for(int i=0;i<eventSystemRaysastResults.Count;i++){
            RaycastResult curRaysastResult=eventSystemRaysastResults[i];
            if(curRaysastResult.gameObject.name==gameObject.name)
                return true;
        }
        return false;
    }
    ///Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults(){
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position=Input.mousePosition;
        List<RaycastResult> raysastResults=new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
