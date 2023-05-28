using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;

public class LogicGate : MonoBehaviour{
    [Header("Variables")]
    [SerializeField] public LogicGateType logicGateType;
    [SerializeField] public LogicGateSignalsType logicGateSignalsType;
    [SerializeField] public bool charged1;
    [SerializeField] public bool charged2;
    [SerializeField] public bool active;
    [SerializeField] float _delaySet=0.15f;
    [SerializeField] public bool motherGate=false;
    [HideIf("@this.gatePowered!=null")][SceneObjectsOnly][SerializeField]public LogicGate gatePowering1;
    [HideIf("@this.gatePowered!=null")][SceneObjectsOnly][SerializeField]public  LogicGate gatePowering2;
    [HideIf("@this.gatePowering1!=null")][DisableInEditorMode][SceneObjectsOnly][SerializeField] LogicGate gatePowered;
    [HideIf("@this.gatePowering1==null")][DisableInEditorMode][SerializeField] GateLine gatePowering1Line;
    [HideIf("@this.gatePowering1==null")][DisableInEditorMode][SerializeField] GateLine gatePowering2Line;

    [Header("References")]
    [AssetsOnly][SerializeField] GameObject gatePoweringLinePrefab;
    [ChildGameObjectsOnly][SerializeField] SpriteRenderer logicGateSymbol;
    [ChildGameObjectsOnly][SerializeField] GameObject lightChild;
    [ChildGameObjectsOnly][SerializeField] GameObject signalSingleSymbol;
    [ChildGameObjectsOnly][SerializeField] GameObject signal1Symbol;
    [ChildGameObjectsOnly][SerializeField] GameObject signal2Symbol;
    [ChildGameObjectsOnly][SerializeField] GameObject gateMotherSymbol;
    [ChildGameObjectsOnly][SerializeField] UnityEngine.UI.Image signalSingleTypeIcon;
    [ChildGameObjectsOnly][SerializeField] UnityEngine.UI.Image signal1TypeIcon;
    [ChildGameObjectsOnly][SerializeField] UnityEngine.UI.Image signal2TypeIcon;
    [Header("Sprites, Colors, Materials")]
    [SerializeField] Sprite[] logicGateSymbols;
    [SerializeField] SpriteNColor spritencolor_activated;
    [SerializeField] Material material_activated;
    [SerializeField] SpriteNColor spritencolor_deactivated;
    [SerializeField] Material material_deactivated;
    [DisableInEditorMode][SerializeField] Vector2 _startPos;
    [DisableInEditorMode][SerializeField] float _delay;
    [DisableInEditorMode][SerializeField] bool _initialized;
    [SerializeField] bool _debug;

    SpriteRenderer spr;
    void Start(){
        spr=GetComponent<SpriteRenderer>();
        _startPos=transform.position;
        ConnectWithGatePowering();
        //if(!charged){chargedSymbol.SetActive(false);}else{chargedSymbol.SetActive(true);}
        if(motherGate){gateMotherSymbol.SetActive(true);}else{gateMotherSymbol.SetActive(false);}
        if(gatePowering1!=null){gatePowering1.gatePowered=this;}//Crossreference
        if(gatePowering2!=null){gatePowering2.gatePowered=this;}//Crossreference
    }
    void Update(){
        ActivateBasedOnLogic();
        UpdateGatePowering1Color();
        UpdateGatePowering2Color();
        SetSprites();
        if(Vector2.Distance((Vector2)transform.position,_startPos)>0.4f&&_startPos!=Vector2.zero){
            if(gatePowering1!=null){ConnectWithGatePowering1();}
            //else if(gatePowered!=null){gatePowered.ConnectWithGatePowering();_startPos=transform.position;}
            if(gatePowering2!=null){ConnectWithGatePowering2();}
            //if(gatePowered!=null){gatePowered.ConnectWithGatePowering();_startPos=transform.position;}
        }
        if(_delay>0)_delay-=Time.deltaTime;
        if(_debug)Debug.Log(signal1()+" | "+signal2()+" | ="+active);
    }
    void SetSprites(){
        logicGateSymbol.sprite=logicGateSymbols[(int)logicGateType];

        if(!singleSignalType()){
            signalSingleSymbol.SetActive(false);
            signal1Symbol.SetActive(signal1());
            signal2Symbol.SetActive(signal2());
        }else{
            signalSingleSymbol.SetActive(signal1());
            signal1Symbol.SetActive(false);
            signal2Symbol.SetActive(false);
        }

        string _type1Str="bullet",_type2Str="bullet";
        string _signal1Str="1",_signal2Str="1";

        switch(logicGateSignalsType){
            case LogicGateSignalsType.justbullet:
                _type1Str="bullet";
                if(signal1())_signal1Str="1";
                else{_signal1Str="2";}
            break;
            case LogicGateSignalsType.justgate:
                _type1Str="gate";
                if(signal1())_signal1Str="Activated";
                else{_signal1Str="Deactivated";}
                
            break;
            case LogicGateSignalsType.bulletgate:
                _type1Str="bullet";
                _type2Str="gate";
                if(signal1())_signal1Str="1";
                else{_signal1Str="2";}
                if(signal2())_signal2Str="Activated";
                else{_signal2Str="Deactivated";}

            break;
            case LogicGateSignalsType.twogates:
                _type1Str="gate";
                _type2Str="gate";
                if(signal1())_signal1Str="Activated";
                else{_signal1Str="Deactivated";}
                if(signal2())_signal2Str="Activated";
                else{_signal2Str="Deactivated";}
            break;
            case LogicGateSignalsType.twobullets:
                _type1Str="bullet";
                _type2Str="bullet";
                if(signal1())_signal1Str="1";
                else{_signal1Str="2";}
                if(signal2())_signal2Str="1";
                else{_signal2Str="2";}

            break;
        }

        bool reverseDisplay=true;string _signal1StrFinal=_signal1Str,_signal2StrFinal=_signal2Str;
        if(reverseDisplay){
            if(_signal1Str=="1")_signal1StrFinal="2";
            if(_signal1Str=="2")_signal1StrFinal="1";
            if(_signal2Str=="1")_signal2StrFinal="2";
            if(_signal2Str=="2")_signal2StrFinal="1";
            if(_signal1Str=="Activated")_signal1StrFinal="Deactivated";
            if(_signal1Str=="Deactivated")_signal1StrFinal="Activated";
            if(_signal2Str=="Activated")_signal2StrFinal="Deactivated";
            if(_signal2Str=="Deactivated")_signal2StrFinal="Activated";
        }
        Sprite _sprType1=AssetsManager.instance.Spr(_type1Str+_signal1StrFinal),_sprType2=AssetsManager.instance.Spr(_type2Str+_signal2StrFinal);
        signalSingleTypeIcon.gameObject.SetActive(singleSignalType());
        signal1TypeIcon.gameObject.SetActive(!singleSignalType());
        signal2TypeIcon.gameObject.SetActive(!singleSignalType());
        signalSingleTypeIcon.sprite=_sprType1;
        signal1TypeIcon.sprite=_sprType1;
        signal2TypeIcon.sprite=_sprType2;
    }
    bool singleSignalType(){return logicGateSignalsType==LogicGateSignalsType.justbullet||logicGateSignalsType==LogicGateSignalsType.justgate;}
    bool signal1(){
        bool _s=false;
        switch(logicGateSignalsType){
            case LogicGateSignalsType.justbullet:
                _s=(charged1);
            break;
            case LogicGateSignalsType.justgate:
                _s=(gatePowering1.active);
            break;
            case LogicGateSignalsType.bulletgate:
                //_s=(charged1||gatePowering1.active);
                _s=(charged1);
            break;
            case LogicGateSignalsType.twogates:
                //_s=(gatePowering1.active||gatePowering2.active);
                _s=(gatePowering1.active);
            break;
            case LogicGateSignalsType.twobullets:
                _s=(charged1);
            break;
        }return _s;
    }
    bool signal2(){
        bool _s=false;
        switch(logicGateSignalsType){//the same as signal1
            case LogicGateSignalsType.justbullet:
                _s=(charged1);
            break;
            case LogicGateSignalsType.justgate://the same as signal1
                _s=(gatePowering1.active);
            break;
            case LogicGateSignalsType.bulletgate://exclude signal1
                //if(charged1){_s=gatePowering1.active;}
                //if(gatePowering1.active){_s=charged1;}
                _s=gatePowering1.active;
            break;
            case LogicGateSignalsType.twogates://exclude signal1
                //if(gatePowering1.active){_s=gatePowering2.active;}
                //if(gatePowering2.active){_s=gatePowering1.active;}
                _s=gatePowering2.active;
            break;
            case LogicGateSignalsType.twobullets://exclude signal1
                /*if(charged1){_s=charged2;}
                if(charged2){_s=charged1;}*/
                _s=charged2;
            break;
        }return _s;
    }
    bool LogicForGates(){
        bool _isTrue=false;
        switch(logicGateType){
            case LogicGateType.and:
                if(signal1()&&signal2()){_isTrue=true;}else{_isTrue=false;}
            break;
            case LogicGateType.nand:
                if(!signal1()||!signal2()){_isTrue=true;}else{_isTrue=false;}
            break;
            case LogicGateType.xor:
                if((!signal1()&&signal2())||(signal1()&&!signal2())){_isTrue=true;}else{_isTrue=false;}
            break;
            case LogicGateType.xnor:
                if((!signal1()&&!signal2())||(signal1()&&signal2())){_isTrue=true;}else{_isTrue=false;}
            break;
            case LogicGateType.or:
                if(signal1()||signal2()){_isTrue=true;}else{_isTrue=false;}
            break;
            case LogicGateType.nor:
                if(!signal1()&&!signal2()){_isTrue=true;}else{_isTrue=false;}

            break;
            case LogicGateType.not:
                if(!signal1()&&!signal2()){_isTrue=true;}else{_isTrue=false;}
            break;
        }
        return _isTrue;
    }
    void ActivateBasedOnLogic(){
        if(LogicForGates()){Activate();}else{Deactivate();}
    }
    public void Activate(bool force=false){
        if(!active||force){
            spr.sprite=spritencolor_activated.spr;
            lightChild.GetComponent<Light2D>().enabled=true;
            spr.material=material_activated;
            active=true;
            if(_initialized){
                AudioManager.instance.Play("GateActivate");
                AudioManager.instance.StopPlaying("GateDeactivate");
                if(motherGate){VictoryCanvas.instance.Win();}
            }if(!_initialized){_initialized=true;}
        }
        
    }
    public void Deactivate(bool force=false){
        if(!_initialized){_initialized=true;return;}
        if(active||force){
            spr.sprite=spritencolor_deactivated.spr;
            lightChild.GetComponent<Light2D>().enabled=false;
            spr.material=material_deactivated;
            active=false;
            if(_initialized){
                AudioManager.instance.Play("GateDeactivate");
                AudioManager.instance.StopPlaying("GateActivate");
            }if(!_initialized){_initialized=true;}
        }
    }
    public void Charge(){
        if(!charged1||(logicGateSignalsType==LogicGateSignalsType.twobullets&&charged1&&!charged2)){
            if(_delay<=0){
                if(!charged1){charged1=true;}//signal1Symbol.SetActive(true);}
                else if(logicGateSignalsType==LogicGateSignalsType.twobullets&&charged1&&!charged2){charged2=true;}//signal2Symbol.SetActive(true);}
                _delay=_delaySet;
                AudioManager.instance.Play("GateCharge");AudioManager.instance.StopPlaying("GateDischarge");
            }
        }
    }
    public void Discharge(){
        if(charged1||(logicGateSignalsType==LogicGateSignalsType.twobullets&&!charged1&&charged2)){
            if(_delay<=0){
                if(charged1){charged1=false;}//signal1Symbol.SetActive(false);}
                else if(logicGateSignalsType==LogicGateSignalsType.twobullets&&!charged1&&charged2){charged2=false;}//signal2Symbol.SetActive(false);}
                _delay=_delaySet;
                AudioManager.instance.Play("GateDischarge");AudioManager.instance.StopPlaying("GateCharge");
            }
        }
    }
    public void ForceUpdateActive(bool _active){
        _initialized=false;
        active=_active;
        if(active){Activate(true);}else{Deactivate(true);}
    }

    public void ConnectWithGatePoweringDelay(float delay=0.2f){StartCoroutine(ConnectWithGatePoweringDelayI(delay));}
    IEnumerator ConnectWithGatePoweringDelayI(float delay){
        //ConnectWithGatePowering1();
        yield return new WaitForSeconds(delay);
        //ConnectWithGatePowering2();
        ConnectWithGatePowering();
    }
    public void ConnectWithGatePowering(){
        ConnectWithGatePowering1();
        ConnectWithGatePowering2();
    }
    void ConnectWithGatePowering1(){
        if(gatePowering1!=null){
            if(gatePowering1Line==null){
                gatePowering1Line=Instantiate(gatePoweringLinePrefab,WorldCanvas.instance.transform).GetComponent<GateLine>();ConnectWithGatePowering();
            }else{
                _startPos=transform.position;
                Vector2 _pos1=gatePowering1.transform.position;_pos1=new Vector2(_pos1.x+0.5f,_pos1.y+0.5f);
                Vector2 _pos2=transform.position;_pos2=new Vector2(_pos2.x+0.5f,_pos2.y+0.5f);
                gatePowering1Line.SetBothPointsNull();
                gatePowering1Line.SetPoints(_pos1,_pos2);
                gatePowering1.gatePowered=this;
                if(_pos2.y<_pos1.y){gatePowering1Line.SetScanningDir(true);}else{/*Do nothing since its already upwards*/}
                UpdateGatePowering1Color();
            }
        }
    }
    void ConnectWithGatePowering2(){
        if(gatePowering2!=null){
            if(gatePowering2Line==null){
                gatePowering2Line=Instantiate(gatePoweringLinePrefab,WorldCanvas.instance.transform).GetComponent<GateLine>();ConnectWithGatePowering();
            }else{
                _startPos=transform.position;
                Vector2 _pos1=gatePowering2.transform.position;_pos1=new Vector2(_pos1.x+0.5f,_pos1.y+0.5f);
                Vector2 _pos2=transform.position;_pos2=new Vector2(_pos2.x+0.5f,_pos2.y+0.5f);
                gatePowering2Line.SetBothPointsNull();
                gatePowering2Line.SetPoints(_pos1,_pos2);
                gatePowering2.gatePowered=this;
                if(_pos2.y<_pos1.y){gatePowering2Line.SetScanningDir(true);}else{/*Do nothing since its already upwards*/}
                UpdateGatePowering2Color();
            }
        }
    }
    void UpdateGatePowering1Color(){
        if(gatePowering1!=null){
            if(gatePowering1.active){if(!gatePowering1Line.CompareColors(spritencolor_activated.color)){gatePowering1Line.SetColor(spritencolor_activated.color);}}
            else{if(!gatePowering1Line.CompareColors(spritencolor_deactivated.color)){gatePowering1Line.SetColor(spritencolor_deactivated.color);}}
        }else{ConnectWithGatePowering1();}
    }
    void UpdateGatePowering2Color(){
        if(gatePowering2!=null){
            if(gatePowering2.active){if(!gatePowering2Line.CompareColors(spritencolor_activated.color)){gatePowering2Line.SetColor(spritencolor_activated.color);}}
            else{if(!gatePowering2Line.CompareColors(spritencolor_deactivated.color)){gatePowering2Line.SetColor(spritencolor_deactivated.color);}}
        }else{ConnectWithGatePowering2();}
    }
}
public enum LogicGateSignalsType{justbullet,justgate,bulletgate,twogates,twobullets}