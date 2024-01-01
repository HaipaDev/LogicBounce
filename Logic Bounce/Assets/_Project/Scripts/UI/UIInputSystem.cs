using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIInputSystem : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler{     public static UIInputSystem instance;
    [SerializeField]EventSystem es;
    [SerializeField]public GameObject currentSelected;
    [SerializeField]GameObject lastSelected;
    [SerializeField]public Button btn;
    [SerializeField]Button btnLast;
    //[SerializeField]public bool inputSelecting=true;
    //[SerializeField]Vector2 mousePosCanv;
    //[SerializeField]Vector2 mousePosPrev;
    void Start(){
        instance=this;
        if(FindObjectOfType<EventSystem>()!=null)es=FindObjectOfType<EventSystem>();
        if(es!=null)if(es.firstSelectedGameObject!=null)btn=es.firstSelectedGameObject.GetComponent<Button>();//mousePosPrev=Input.mousePosition;
    }
    public void SetSelected(){
        if(es!=null)es.SetSelectedGameObject(null);
        if(es!=null&&btn!=null){es.SetSelectedGameObject(btn.gameObject);btnLast=btn;}
    }
    void Update(){
        /*if(Input.GetButtonDown("Horizontal")||Input.GetButtonDown("Vertical")){inputSelecting=true;mousePosPrev=Input.mousePosition;}
        else{if((Vector2)Input.mousePosition!=mousePosPrev){inputSelecting=false;}}
        if(!inputSelecting){if(GetButtonUnderMouse()!=null)btn=GetButtonUnderMouse();if(btn!=btnLast)SetSelected();}*/
        //currentSelected=es.currentSelectedGameObject;
        //if(currentSelected!=null&&currentSelected.GetComponent<ButtonScript>()==null){currentSelected.AddComponent<ButtonScript>();}
        if(currentSelected!=lastSelected&&currentSelected==null&&lastSelected!=null){lastSelected=null;return;}
        else if((currentSelected!=lastSelected&&currentSelected!=null)||lastSelected==null){/*AudioManager.instance.Play("ButtonSelect");*/lastSelected=currentSelected;return;}
        if(Input.GetKeyDown(KeyCode.Return)&&currentSelected.GetComponent<TMP_InputField>()!=null){currentSelected.GetComponent<TMP_InputField>().DeactivateInputField();}
        if(StepsManager.instance!=null){
            if((currentSelected==null)||(currentSelected!=null&&StepsManager.instance.selectedStep!=null&&currentSelected!=StepsManager.instance.selectedStep.gameObject)){
                StepsManager.instance.SelectStep(null);}
        }
    }
    public Button GetButtonUnderMouse(){
        Button btn=null;

        /*PointerEventData eventDataCurrentPosition=new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position=(Vector2)Input.mousePosition;
        List<RaycastResult> results=new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition,results);
        foreach(RaycastResult rr in results){if(rr.gameObject.GetComponent<Button>()!=null){btn=rr.gameObject.GetComponent<Button>();}}//else{btn=null;}}*/
        return btn;
    }
    public void OnPointerEnter(PointerEventData eventData){
        /*if(eventData.pointerCurrentRaycast.gameObject.GetComponent<Button>()==null){currentSelected=null;}
        else{if(eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ScrollRect>()==null){
            foreach(GameObject go in eventData.hovered){Debug.Log(go.name);if(go.GetComponent<Button>()!=null){btn=go.GetComponent<Button>();}}
        }}*/
    }
    public void OnPointerExit(PointerEventData eventData){currentSelected=null;}

    /*public static bool IsPointerOverUIObject(){
        return false;
        PointerEventData eventDataCurrentPosition=new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position=(Vector2)Input.mousePosition;
        List<RaycastResult> results=new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition,results);
        return results.Count>0;
    }*/
}
