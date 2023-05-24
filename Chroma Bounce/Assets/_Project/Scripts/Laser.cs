using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Laser : MonoBehaviour{
    [Header("References")]
    [SerializeField]SpriteNColor[] spritesAndColors;
    [Header("Variables")]
    [SerializeField]public int currentColorId;
    [SerializeField]float scaleupSpeed=3.5f;

    SpriteRenderer spr;
    void Start(){
        spr=GetComponent<SpriteRenderer>();
        ChangeColor(currentColorId);
        _posWhenTouched=transform.position;touchingBottom=false;touchingUp=false;
    }
    void Update(){AutoScale();}
    bool touchingBottom=false;bool touchingUp=false;
    float raycastLength=0.04f;
    Vector2 raycastDownOrigin;Vector2 laserDownDirection;
    Vector2 raycastUpOrigin;Vector2 laserUpDirection;
    Vector2 _posWhenTouched;
    void AutoScale(){
        if(!touchingUp){//Move upwards till hits a wall
            float _step=scaleupSpeed*Time.deltaTime;
            Vector2 moveUpwards=laserUpDirection.normalized*_step;
            transform.Translate(moveUpwards);
        }
        if(touchingUp&&!touchingBottom){//Scale up downwards
            float step=scaleupSpeed*Time.deltaTime;
            transform.localScale=new Vector2(transform.localScale.x,transform.localScale.y+step);
        }
        if((Vector2)transform.position!=_posWhenTouched&&_posWhenTouched!=Vector2.zero||transform.localScale.y>10){//Reset when position changed or too big (outside walls)
            transform.localScale=new Vector2(transform.localScale.x,0.1f);_posWhenTouched=Vector2.zero;
        }
        if(touchingUp&&touchingBottom&&transform.localScale.y<0.15f){//Push out of the wall
            float _step=scaleupSpeed*Time.deltaTime;
            Vector2 moveUpwards=laserUpDirection.normalized*_step;
            transform.Translate(-moveUpwards);
        }
    }
    void FixedUpdate(){
        laserDownDirection = Quaternion.Euler(0f, 0f, transform.eulerAngles.z) * Vector2.down;
        float _correction=0;
        if(transform.eulerAngles.z<360&&transform.eulerAngles.z>180){_correction=90f;}
        else if(transform.eulerAngles.z>0&&transform.eulerAngles.z<180){_correction=-90f;}
        else if(transform.eulerAngles.z==180){_correction=180f;}
        laserUpDirection = Quaternion.Euler(0f, 0f, transform.eulerAngles.z+_correction) * Vector2.up;

        raycastDownOrigin = transform.position - (transform.up * (transform.localScale.y * 2f))+(transform.up*raycastLength/2);
        raycastUpOrigin = transform.position-(transform.up*raycastLength/2);//raycastLength gives it some buffer to detect when inside a wall
        
        LayerMask wallLayer=LayerMask.GetMask("StaticColliders");
        RaycastHit2D hitDown = Physics2D.Raycast(raycastDownOrigin, laserDownDirection, raycastLength, wallLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(raycastUpOrigin, laserUpDirection, raycastLength, wallLayer);
        
        if(hitDown.collider!=null&&hitDown.collider.gameObject!=gameObject){touchingBottom=true;if(_posWhenTouched==Vector2.zero){_posWhenTouched=transform.position;}Debug.Log("touchingBottom");}
        else{touchingBottom=false;}
        if(hitUp.collider!=null){touchingUp=true;Debug.Log("touchingUp");}
        else{touchingUp=false;}

        /*
        LayerMask wallLayer=LayerMask.GetMask("StaticColliders","Lasers");
        RaycastHit2D hitDown = Physics2D.Raycast(raycastDownOrigin, laserDownDirection, raycastLength, wallLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(raycastUpOrigin, laserUpDirection, raycastLength, wallLayer);
        
        if(hitDown.collider!=null&&hitDown.collider.gameObject!=gameObject){touchingBottom=true;if(_posWhenTouched==Vector2.zero){_posWhenTouched=transform.position;}Debug.Log("touchingBottom");}
        else if(hitDown.collider==null||(hitDown.collider!=null&&hitDown.collider.gameObject!=gameObject)){touchingBottom=false;}
        if(hitUp.collider!=null&&hitUp.collider.gameObject!=gameObject){touchingUp=true;Debug.Log("touchingUp");}
        else if(hitUp.collider==null||(hitUp.collider!=null&&hitUp.collider.gameObject!=gameObject)){touchingUp=false;}
        */
    }
    void OnDrawGizmos(){
        Gizmos.color=Color.red;
        Gizmos.DrawRay(raycastDownOrigin,laserDownDirection*raycastLength);
        Gizmos.color=Color.green;
        Gizmos.DrawRay(raycastUpOrigin,laserUpDirection*raycastLength);
    }
    public void SwitchColor(){
        switch(currentColorId){
            case 0:
                ChangeColor(1);
            break;
            case 1:
                ChangeColor(0);
            break;
        }
    }
    public void ChangeColor(int id){
        currentColorId=id;
        spr.sprite=spritesAndColors[id].spr;
        if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritesAndColors[id].color;
    }
}
