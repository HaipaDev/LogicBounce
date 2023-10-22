using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Globalization;
using TMPro;

public class GunRotationSlider : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler{
    Vector2 m_localPos, m_screenPos; 
    Camera m_eventCamera;
    float currentAngle;
    // bool isPointerDown,isPointerReleased;
    void Start(){}

    void Update(){
        // Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Vector3 direction = mousePosition - transform.position;
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // //transform.rotation  = Quaternion.AngleAxis(angle, Vector3.forward);
        // // transform.rotation = new Quaternion(0,0,angle,0);
        // transform.localEulerAngles = new Vector3(0,0,angle);
    }
    public void SetAngleFromString(string str){
        float _angle;
        float.TryParse(str.Replace(",","."), NumberStyles.Any, CultureInfo.InvariantCulture, out _angle);
        //SetAngle((_angle + 360) % 360,false);
        //SetAngle((_angle - 90) % 360,false);
        //SetAngle(_angle-90,false);
        SetAngle(_angle,true);
    }
    public void SetAngle(float _angle,bool fromstr=false){
        // currentAngle=_angle;
        currentAngle= -1*(_angle - 360) % 360;
        if(!LevelMapManager.instance.GetCurrentLevelMap().accurateGunRotation){currentAngle=Mathf.RoundToInt(currentAngle);}

        Debug.Log("Current angle: "+currentAngle);
        if(fromstr){
            //float correctedRotationAngle = (currentAngle + 360) % 360;
            //float correctedRotationAngle = -1 * (currentAngle + 360) % 360;
            float correctedRotationAngle = -currentAngle;
            transform.GetChild(0).localEulerAngles=new Vector3(0,0,currentAngle);
            // transform.GetChild(0).localEulerAngles=new Vector3(0,0,correctedRotationAngle);
        }else{
            transform.parent.parent.GetComponentInChildren<TMP_InputField>().text=System.Math.Round(currentAngle,2).ToString();
        }
    }
    // public void UpdateValue(){
    //     //float angle=GetComponent<RadialSlider>().Angle-120;
    //     float value=GetComponent<RadialSlider>().Value;
    //     // Debug.Log(value);
    //     float shiftedValue=1.0f - Mathf.Abs(value - 0.5f) * 2.0f;
    //     // Debug.Log(shiftedValue);
    //     float angle=value*360;
    //     //Debug.Log(angle);
    //     transform.GetChild(0).localEulerAngles=new Vector3(0,0,angle);
    // }

    // float GetAngleFromMousePoint(){
    //     // RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, m_screenPos, m_eventCamera, out m_localPos);

    //     // // radial pos of the mouse position.
    //     // return (Mathf.Atan2(-m_localPos.y, m_localPos.x) * 180f / Mathf.PI + 180f) / 360f;
        
    //     Vector2 localPos = m_screenPos - (Vector2)transform.position;

    //     // Calculate the angle based on the relative mouse position
    //     float angle = Mathf.Atan2(-localPos.y, localPos.x) * Mathf.Rad2Deg;

    //     // angle = (angle + 360) % 360;

    //     return angle / 360;
    // }
    float GetAngleFromMousePoint(){
        // Calculate the vector from the center of the UI element to the mouse position
        Vector2 direction = m_screenPos - (Vector2)transform.position;

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ensure the angle is positive and within [0, 360] degrees
        angle = (angle + 360) % 360;
        // With plus it works but is reversed so when pointing towards the bottom (which should be 90) it shows 270, it goes against the clocks direction
        // With minus it would work but is negative, and by Mathf.Abs() it breaks everything
        // if (angle < 0) {
        //     angle += 360;
        // }

        return angle;
    }

    #region Interfaces
        // Called when the pointer enters our GUI component.
        // Start tracking the mouse
        public void OnPointerEnter(PointerEventData eventData){
            m_screenPos = eventData.position;
            m_eventCamera = eventData.enterEventCamera;
        }

        public void OnPointerDown(PointerEventData eventData){
            m_screenPos = eventData.position;
            m_eventCamera = eventData.enterEventCamera;
            // isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData){
            m_screenPos = Vector2.zero;
            // isPointerDown = false;
            // isPointerReleased = true;
        }

        public void OnDrag(PointerEventData eventData){
            m_screenPos = eventData.position;
            SetAngle(GetAngleFromMousePoint(),false);
        }
        #endregion
}
