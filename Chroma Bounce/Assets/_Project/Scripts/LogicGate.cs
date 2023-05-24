using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;

public class LogicGate : MonoBehaviour{
    [Header("Variables")]
    [SerializeField] LogicGateType logicGateType;
    [SerializeField] bool charged;
    [SerializeField] bool active;

    [Header("References")]
    [AssetsOnly][SerializeField] GameObject gatePoweringLinePrefab;
    [ChildGameObjectsOnly][SerializeField] SpriteRenderer logicGateSymbol;
    [ChildGameObjectsOnly][SerializeField] GameObject lightChild;
    [ChildGameObjectsOnly][SerializeField] GameObject chargedSymbol;
    [SceneObjectsOnly][SerializeField] LogicGate gatePowering;
    [HideIf("@this.gatePowering==null")][DisableInEditorMode][SerializeField] GateLine gatePoweringLine;
    [Header("Sprites, Colors, Materials")]
    [SerializeField] Sprite[] logicGateSymbols;
    [SerializeField] SpriteNColor spritencolor_activated;
    [SerializeField] Material material_activated;
    [SerializeField] SpriteNColor spritencolor_deactivated;
    [SerializeField] Material material_deactivated;

    SpriteRenderer spr;
    void Start(){
        spr=GetComponent<SpriteRenderer>();
        ConnectWithGatePowering();
        if(!charged){chargedSymbol.SetActive(false);}else{chargedSymbol.SetActive(true);}
    }
    void Update(){
        logicGateSymbol.sprite=logicGateSymbols[(int)logicGateType];
        LogicForGates();
        UpdateGatePoweringColor();
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
            AudioManager.instance.Play("GateActivate");
            AudioManager.instance.StopPlaying("GateDeactivate");
        }
    }
    public void Deactivate(){
        if(active&&spr.sprite!=spritencolor_deactivated.spr){
            spr.sprite=spritencolor_deactivated.spr;
            lightChild.GetComponent<Light2D>().enabled=false;
            spr.material=material_deactivated;
            active=false;
            AudioManager.instance.Play("GateDeactivate");
            AudioManager.instance.StopPlaying("GateActivate");
        }
    }
    public void Charge(){if(!charged){charged=true;AudioManager.instance.Play("GateCharge");chargedSymbol.SetActive(true);AudioManager.instance.StopPlaying("GateDischarge");}}
    public void Discharge(){if(charged){charged=false;AudioManager.instance.Play("GateDischarge");chargedSymbol.SetActive(false);AudioManager.instance.StopPlaying("GateCharge");}}

    void ConnectWithGatePowering(){
        if(gatePowering!=null){
            if(gatePoweringLine==null){gatePoweringLine=Instantiate(gatePoweringLinePrefab,WorldCanvas.instance.transform).GetComponent<GateLine>();ConnectWithGatePowering();}
            else{
                Vector2 _pos1=gatePowering.transform.position;_pos1=new Vector2(_pos1.x+0.5f,_pos1.y+0.5f);
                Vector2 _pos2=transform.position;_pos2=new Vector2(_pos2.x+0.5f,_pos2.y+0.5f);
                gatePoweringLine.SetBothPointsNull();
                gatePoweringLine.SetPoints(_pos1,_pos2);
                //Debug.Log(_pos2.y+" < "+_pos1.y);
                if(_pos2.y<_pos1.y){gatePoweringLine.SetScanningDir(true);}else{/*Do nothing since its already upwards*/}

                if(gatePowering.active){if(!gatePoweringLine.CompareColors(spritencolor_activated.color)){gatePoweringLine.SetColor(spritencolor_activated.color);}}
                else{if(!gatePoweringLine.CompareColors(spritencolor_deactivated.color)){gatePoweringLine.SetColor(spritencolor_deactivated.color);}}
            }
        }
    }
    void UpdateGatePoweringColor(){
        if(gatePowering!=null){
            if(gatePowering.active){if(!gatePoweringLine.CompareColors(spritencolor_activated.color)){gatePoweringLine.SetColor(spritencolor_activated.color);}}
            else{if(!gatePoweringLine.CompareColors(spritencolor_deactivated.color)){gatePoweringLine.SetColor(spritencolor_deactivated.color);}}
        }
    }
}
