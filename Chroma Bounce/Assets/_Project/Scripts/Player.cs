using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Player : MonoBehaviour{    public static Player instance;
    [ChildGameObjectsOnly][SerializeField] Transform rotatePoint;
    [ChildGameObjectsOnly][SerializeField] Transform crosshairTransform;
    [ChildGameObjectsOnly][SerializeField] SpriteRenderer gunSpr;
    [AssetsOnly][SerializeField] GameObject bulletPrefab;
    [SerializeField][Range(-360,360)] int rotZmin=180;
    [SerializeField][Range(-360,360)] int rotZmax=360;
    [SerializeField] int startingColorId=0;
    [SerializeField] float timeBetweenFiring=1f;
    [SerializeField]public float bulletSpeed=4f;
    [SerializeField]public float b_correctionAngle=-90;
    [DisableInEditorMode][SerializeField] bool canFire=true;
    [DisableInEditorMode][SerializeField] float timer;


    Vector3 mousePos;
    void Awake(){if(Player.instance!=null){Destroy(gameObject);}else{instance=this;}}
    void Start(){
        canFire=true;timer=timeBetweenFiring;
    }
    void Update(){
        ///Rotating and limiting
        mousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rot = mousePos-rotatePoint.position;
        float rotZ=Mathf.Atan2(rot.y,rot.x)*Mathf.Rad2Deg;
        //if(rotZ>180)rotZ-=360;
        //Debug.Log("Before: "+rotZ);
        //if(rotZ<-180)rotZ+=360;
        //rotZ=Mathf.Clamp(rotZ,rotZmin,rotZmax);
        //Debug.Log("After: "+rotZ);

        rotatePoint.rotation=Quaternion.Euler(0,0,rotZ);
        //Debug.Log("Euler Z: "+rotatePoint.eulerAngles.z);

        Vector3 euler=rotatePoint.eulerAngles;
        //Debug.Log("Before: "+euler.z+" | "+rotZ);
        //if(euler.z>180)euler.z-=360;
        //Debug.Log("After Step 1: "+euler.z+" | "+(rotZmin-90));
        if(euler.z<(rotZmin-90)){euler.z=rotZmax;}
        //if(euler.z>(rotZmax+90)&&euler.z>90){euler.z=rotZmax;}
        euler.z=Mathf.Clamp(euler.z,rotZmin,rotZmax);
        rotatePoint.eulerAngles=euler;
        //Debug.Log("After Step 2: "+euler.z);

        ///Rotate gun sprite
        if(euler.z</*270*/((rotZmin+rotZmax)/2)){gunSpr.flipY=true;}else{gunSpr.flipY=false;}


        ///Shooting
        if(Input.GetMouseButton(0)&&canFire){
            canFire=false;timer=timeBetweenFiring;
            
            GameObject bullet=Instantiate(bulletPrefab,rotatePoint.position,Quaternion.identity);
            Transform b_trans=bullet.transform;
            Bullet b_comp=bullet.GetComponent<Bullet>();b_comp.currentColorId=startingColorId;
            Rigidbody2D b_rb=bullet.GetComponent<Rigidbody2D>();

            Vector3 _dir=crosshairTransform.position-b_trans.position;
            Vector3 _rotation=b_trans.position-crosshairTransform.position;
            b_rb.velocity=new Vector2(_dir.x, _dir.y).normalized*bulletSpeed;
            float b_rotZ = Mathf.Atan2(_rotation.y,_rotation.x)*Mathf.Rad2Deg;
            b_trans.rotation=Quaternion.Euler(0,0,b_rotZ+b_correctionAngle);
        }
        if(!canFire){
            if(timer>0)timer-=Time.deltaTime;
            if(timer<=0){canFire=true;}
        }
    }
}
