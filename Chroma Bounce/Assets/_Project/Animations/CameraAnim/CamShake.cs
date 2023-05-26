using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour{     public static CamShake instance;
    [SerializeField]Vector2 defaultPos;
    [SerializeField]Vector2 ammount=new Vector2(-0.11f,0);
    [HideInInspector]float strength;

    Animator camAnim;
    void Awake(){instance=this;}
    void Start(){defaultPos=transform.position;camAnim=GetComponent<Animator>();}
    public void DoCamShake(float multiplier, float speed){
        if(SaveSerial.instance.settingsData.screenshake){
            if(multiplier>strength||camAnim.GetBool("shake")!=true){
                camAnim.ResetTrigger("shake");
                camAnim.SetTrigger("shake");
                camAnim.speed=speed;
                strength=multiplier;
            }
        }
    }
    public void SetDefaultPos(Vector2 pos){defaultPos=pos;}
    public Vector2 GetDefaultPos(){return defaultPos;}
    void Update(){
        transform.position=new Vector3(defaultPos.x+(ammount.x*strength),defaultPos.y+(ammount.y*strength),transform.position.z);
    }
}
