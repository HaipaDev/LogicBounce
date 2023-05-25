using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;

public class LogicGate : MonoBehaviour{
    [Header("Variables")]
    [SerializeField] LogicGateType logicGateType;
    [SerializeField] public bool charged;
    [SerializeField] public bool active;
    [SerializeField] float _delaySet=0.15f;

    [Header("References")]
    [AssetsOnly][SerializeField] GameObject gatePoweringLinePrefab;
    [ChildGameObjectsOnly][SerializeField] SpriteRenderer logicGateSymbol;
    [ChildGameObjectsOnly][SerializeField] GameObject lightChild;
    [ChildGameObjectsOnly][SerializeField] GameObject chargedSymbol;
    [HideIf("@this.gatePowered!=null")][SceneObjectsOnly][SerializeField] LogicGate gatePowering;
    [DisableInEditorMode][HideIf("@this.gatePowering!=null")][SceneObjectsOnly][SerializeField] LogicGate gatePowered;
    [HideIf("@this.gatePowering==null")][DisableInEditorMode][SerializeField] GateLine gatePoweringLine;
    [Header("Sprites, Colors, Materials")]
    [SerializeField] Sprite[] logicGateSymbols;
    [SerializeField] SpriteNColor spritencolor_activated;
    [SerializeField] Material material_activated;
    [SerializeField] SpriteNColor spritencolor_deactivated;
    [SerializeField] Material material_deactivated;
    [DisableInEditorMode][SerializeField] Vector2 _startPos;
    [DisableInEditorMode][SerializeField] float _delay;
    [DisableInEditorMode][SerializeField] bool _initialized;

    SpriteRenderer spr;
    void Start(){
        spr=GetComponent<SpriteRenderer>();
        _startPos=transform.position;
        ConnectWithGatePowering();
        if(!charged){chargedSymbol.SetActive(false);}else{chargedSymbol.SetActive(true);}
        if(gatePowering!=null){gatePowering.gatePowered=this;}//Crossreference
    }
    void Update(){
        logicGateSymbol.sprite=logicGateSymbols[(int)logicGateType];
        LogicForGates();
        UpdateGatePoweringColor();
        if(Vector2.Distance((Vector2)transform.position,_startPos)>0.4f&&_startPos!=Vector2.zero){
            if(gatePowering!=null){ConnectWithGatePowering();}
            else if(gatePowered!=null){gatePowered.ConnectWithGatePowering();_startPos=transform.position;}
        }
        if(_delay>0)_delay-=Time.deltaTime;
    }
    void LogicForGates(){
        bool signal(){if(gatePowering==null){return true;}else{return gatePowering.active;}}
        switch(logicGateType){
            case LogicGateType.and:
                if(signal()&&charged){Activate();}else{Deactivate();}
            break;
            case LogicGateType.nand:
                if(!signal()||!charged){Activate();}else{Deactivate();}
                //if((!signal()&&!charged)||(!signal()&&charged)||(signal()&&!charged)){Activate();}else{Deactivate();}
            break;
            case LogicGateType.xor:
                if((!signal()&&charged)||(signal()&&!charged)){Activate();}else{Deactivate();}
            break;
            case LogicGateType.xnor:
                if((!signal()&&!charged)||(signal()&&charged)){Activate();}else{Deactivate();}
            break;
            case LogicGateType.or:
                if(signal()||charged){Activate();}else{Deactivate();}

            break;
            case LogicGateType.nor:
                if(!signal()&&!charged){Activate();}else{Deactivate();}

            break;
            case LogicGateType.not:
                if(!signal()&&!charged){Activate();}else{Deactivate();}
            break;
        }
    }
    public void Activate(){
        if(!active&&spr.sprite!=spritencolor_activated.spr){
            spr.sprite=spritencolor_activated.spr;
            lightChild.GetComponent<Light2D>().enabled=true;
            spr.material=material_activated;
            active=true;
            if(_initialized){
                AudioManager.instance.Play("GateActivate");
                AudioManager.instance.StopPlaying("GateDeactivate");
            }if(!_initialized){_initialized=true;}
        }
        
    }
    public void Deactivate(){
        if(!_initialized){_initialized=true;return;}
        if(active&&spr.sprite!=spritencolor_deactivated.spr){
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
    public void Charge(){if(!charged){if(_delay<=0){charged=true;_delay=_delaySet;AudioManager.instance.Play("GateCharge");chargedSymbol.SetActive(true);AudioManager.instance.StopPlaying("GateDischarge");}}}
    public void Discharge(){if(charged){if(_delay<=0){charged=false;_delay=_delaySet;AudioManager.instance.Play("GateDischarge");chargedSymbol.SetActive(false);AudioManager.instance.StopPlaying("GateCharge");}}}

    void ConnectWithGatePowering(){
        if(gatePowering!=null){
            if(gatePoweringLine!=null){
                _startPos=transform.position;
                Vector2 _pos1=gatePowering.transform.position;_pos1=new Vector2(_pos1.x+0.5f,_pos1.y+0.5f);
                Vector2 _pos2=transform.position;_pos2=new Vector2(_pos2.x+0.5f,_pos2.y+0.5f);
                gatePoweringLine.SetBothPointsNull();
                gatePoweringLine.SetPoints(_pos1,_pos2);
                if(_pos2.y<_pos1.y){gatePoweringLine.SetScanningDir(true);}else{/*Do nothing since its already upwards*/}
                UpdateGatePoweringColor();
            }else{gatePoweringLine=Instantiate(gatePoweringLinePrefab,WorldCanvas.instance.transform).GetComponent<GateLine>();ConnectWithGatePowering();}
        }
    }
    void UpdateGatePoweringColor(){
        if(gatePowering!=null){
            if(gatePowering.active){if(!gatePoweringLine.CompareColors(spritencolor_activated.color)){gatePoweringLine.SetColor(spritencolor_activated.color);}}
            else{if(!gatePoweringLine.CompareColors(spritencolor_deactivated.color)){gatePoweringLine.SetColor(spritencolor_deactivated.color);}}
        }
    }
}
