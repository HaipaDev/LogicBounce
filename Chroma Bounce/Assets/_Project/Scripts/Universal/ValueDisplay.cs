using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class ValueDisplay : MonoBehaviour{
    [SerializeField] public string value="score";
    [DisableInPlayMode][SerializeField] bool onlyOnEnable=false;
    [HideInPlayMode][SerializeField] bool onValidate=false;
    TextMeshProUGUI txt;
    TMP_InputField tmpInput;
    void Start(){
        if(GetComponent<TextMeshProUGUI>()!=null)txt=GetComponent<TextMeshProUGUI>();
        if(GetComponent<TMP_InputField>()!=null)tmpInput=GetComponent<TMP_InputField>();
        if(onlyOnEnable){ChangeText();}
    }
    void OnEnable(){if(onlyOnEnable){ChangeText();}}
    void OnValidate(){if(onValidate){ChangeText();}}
    void Update(){if(!onlyOnEnable){ChangeText();}}


    void ChangeText(){      string _txt="";
    #region//GameManager
        if(GameManager.instance!=null){     var gs=GameManager.instance;
            if(value=="score") _txt=gs.score.ToString();
        }
    #endregion
    #region//Player
        if(Player.instance!=null){      var p=Player.instance;}
    #endregion
    #region//SaveSerial
        if(SaveSerial.instance!=null){      var s=SaveSerial.instance;var sp=s.playerData;var ss=s.settingsData;
            if(value=="masterVolume"){_txt=ss.masterVolume.ToString();}
            else if(value=="masterOOFVolume"){_txt=ss.masterOOFVolume.ToString();}
            else if(value=="soundVolume"){_txt=ss.soundVolume.ToString();}
            else if(value=="musicVolume"){_txt=ss.musicVolume.ToString();}
        }
    #endregion
        
        if(txt!=null)txt.text=_txt;
        else{if(tmpInput!=null){//if(UIInputSystem.instance!=null)if(UIInputSystem.instance.currentSelected!=tmpInput.gameObject){tmpInput.text=_txt;}
        foreach(TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>()){t.text=_txt;}}}
    }
}
