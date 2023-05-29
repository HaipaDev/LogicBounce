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
        Debug.Log("UpdateInt");
        if(!string.IsNullOrEmpty(val)){
            var i=int.Parse(val);
            i=Mathf.Clamp(i,minVal,maxVal);
            GetComponent<TMP_InputField>().text=i.ToString();
        }else{GetComponent<TMP_InputField>().SetTextWithoutNotify(minVal.ToString());}
    }
    public void UpdateInputFieldFloat(string val){
        Debug.Log("UpdateFloat");
        if(!string.IsNullOrEmpty(val)){
            if(float.TryParse(val, out float i)){
                //var i=float.Parse(val);
                Debug.Log(i);
                i=Mathf.Clamp(i,minValF,maxValF);
                Debug.Log(i);
                Debug.Log(i.ToString("F2"));
                Debug.Log(i.ToString("F2").Replace('.',','));
                GetComponent<TMP_InputField>().SetTextWithoutNotify(i.ToString("F2").Replace('.',','));
            }
        }else{GetComponent<TMP_InputField>().SetTextWithoutNotify(minValF.ToString("F2").Replace('.',','));}
    }
    public void UpdateInputFieldAuto(string val){
        if(!_floatValsNotEmpty()){UpdateInputField(val);}//float
        if(!_intValsNotEmpty()){UpdateInputFieldFloat(val);}//int
    }
    public void UpdateInputFieldAutoFromComponent(){
        if(_floatValsNotEmpty()){UpdateInputFieldFloat(GetComponent<TMP_InputField>().text);}//float
        if(_intValsNotEmpty()){UpdateInputField(GetComponent<TMP_InputField>().text);}//int
    }
    /*bool _floatValsEmpty(){return ((minValF!=0&&maxValF!=0)||(minValF==0&&maxValF>0)||(minValF<0&&maxValF==0));}
    bool _intValsEmpty(){return ((minVal!=0&&maxVal!=0)||(minVal==0&&maxVal>0)||(minVal<0&&maxVal==0));}*/
    bool _floatValsNotEmpty(){return ((minValF!=0&&maxValF!=0)||(minValF==0&&maxValF>0)||(minValF<0&&maxValF==0));}
    bool _intValsNotEmpty(){return ((minVal!=0&&maxVal!=0)||(minVal==0&&maxVal>0)||(minVal<0&&maxVal==0));}
    public void SwitchToInt(){if(_floatValsNotEmpty()){//&&!(minValF==0&&maxValF!=0)){
        Debug.Log("SwitchingToInt");
        minVal=Mathf.RoundToInt(minValF);maxVal=Mathf.RoundToInt(maxValF);
        minValF=0;maxValF=0;
    }}
    public void SwitchToFloat(){if(_intValsNotEmpty()){//&&!(minVal==0&&maxVal!=0)){
        Debug.Log("SwitchingToFloat");
        minValF=minVal;maxValF=maxVal;
        minVal=0;maxVal=0;
    }}
    public Vector2Int GetLimits(){return new Vector2Int(minVal,maxVal);}
    public Vector2 GetLimitsFloat(){return new Vector2(minValF,maxValF);}
}
