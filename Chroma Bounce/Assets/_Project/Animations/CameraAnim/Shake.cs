using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour{     public static Shake instance;
    public Animator camAnim;
    [HideInInspector]public float strength;
    [SerializeField]Vector2 ammount=new Vector2(-0.11f,0);
    void Awake(){instance=this;}
    public void CamShake(float multiplier, float speed){
        if(SaveSerial.instance.settingsData.screenshake){
            if(multiplier>strength||camAnim.GetBool("shake")!=true){
                camAnim.ResetTrigger("shake");
                camAnim.SetTrigger("shake");
                camAnim.speed=speed;
                strength=multiplier;
            }
        }
    }
    void Update(){
        camAnim.transform.position=new Vector3(ammount.x*strength,ammount.y*strength,camAnim.transform.position.z);
    }
}
