using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class LimitInputFieldNum : MonoBehaviour{
    /*[DisableIf("@this.minValF==0&&this.maxValF==0")]*/[SerializeField]int minVal,maxVal;
    /*[DisableIf("@this.minVal==0&&this.maxVal==0")]*/[SerializeField]float minValF,maxValF;
    public void UpdateInputField(string val){
        if(!string.IsNullOrEmpty(val)){
            var i=int.Parse(val);
            i=Mathf.Clamp(i,minVal,maxVal);
            GetComponent<TMPro.TMP_InputField>().text=i.ToString();
        }else{GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(minVal.ToString());}
    }
    public void UpdateInputFieldFloat(string val){
        if(!string.IsNullOrEmpty(val)){
            if(float.TryParse(val, out float i)){
                //var i=float.Parse(val);
                Debug.Log(i);
                i=Mathf.Clamp(i,minValF,maxValF);
                Debug.Log(i);
                Debug.Log(i.ToString("F2"));
                Debug.Log(i.ToString("F2").Replace('.',','));
                GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(i.ToString("F2").Replace('.',','));
            }
        }else{GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(minValF.ToString("F2").Replace('.',','));}
    }
    public void UpdateInputFieldAuto(string val){
        if(minVal==0&&maxVal==0){UpdateInputFieldFloat(val);}//float
        if(minValF==0&&maxValF==0){UpdateInputField(val);}//int
    }
    public void SwitchToInt(){if((minValF!=0&&maxValF!=0)){//&&!(minValF==0&&maxValF!=0)){
        Debug.Log("SwitchingToInt");
        minVal=Mathf.RoundToInt(minValF);maxVal=Mathf.RoundToInt(maxValF);
        minValF=0;maxValF=0;
    }}
    public void SwitchToFloat(){if((minVal!=0&&minVal!=0)){//&&!(minVal==0&&maxVal!=0)){
        Debug.Log("SwitchingToFloat");
        minValF=minVal;maxValF=maxVal;
        minVal=0;maxVal=0;
    }}
    public Vector2Int GetLimits(){return new Vector2Int(minVal,maxVal);}
    public Vector2 GetLimitsFloat(){return new Vector2(minValF,maxValF);}
}
