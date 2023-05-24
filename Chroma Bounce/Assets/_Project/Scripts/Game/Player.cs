using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour{    public static Player instance;
    [Header("References")]
    [ChildGameObjectsOnly][SerializeField] Transform rotatePoint;
    [ChildGameObjectsOnly][SerializeField] Transform gunTransform;
    [AssetsOnly][SerializeField] GameObject bulletPrefab;
    [SerializeField] SpriteNColor[] gunSprColor;
    
    [Header("Variables")]
    [SerializeField][Range(-360,360)] int rotZmin=180;
    [SerializeField][Range(-360,360)] int rotZmax=360;
    [SerializeField] int startingColorId=0;
    [SerializeField] float timeBetweenFiring=1f;
    [SerializeField]public float bulletSpeed=4f;
    [SerializeField]public float b_correctionAngle=-90;
    [Header("Current")]
    [DisableInEditorMode][SerializeField] bool canFire=true;
    [DisableInEditorMode][SerializeField] float timer;


    Vector3 mousePos;
    SpriteRenderer gunSpr;

    void Awake(){if(Player.instance!=null){Destroy(gameObject);}else{instance=this;}}
    void Start(){
        canFire=true;timer=timeBetweenFiring;
        gunSpr=gunTransform.GetComponent<SpriteRenderer>();
    }
    void Update(){
        ///Rotating and limiting
        mousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rot = mousePos-rotatePoint.position;
        float rotZ=Mathf.Atan2(rot.y,rot.x)*Mathf.Rad2Deg;

        rotatePoint.rotation=Quaternion.Euler(0,0,rotZ);

        Vector3 euler=rotatePoint.eulerAngles;
        if(euler.z<(rotZmin-90)){euler.z=rotZmax;}
        euler.z=Mathf.Clamp(euler.z,rotZmin,rotZmax);
        rotatePoint.eulerAngles=euler;

        ///Rotate gun sprite
        if(euler.z</*270*/((rotZmin+rotZmax)/2)){gunSpr.flipY=true;}else{gunSpr.flipY=false;}


        ///Shooting
        if(Input.GetMouseButton(0)&&canFire){
            canFire=false;timer=timeBetweenFiring;
            
            GameObject bullet=Instantiate(bulletPrefab,gunTransform.position,Quaternion.identity);
            Transform gunPoint=gunTransform.GetChild(0);
            Transform b_trans=bullet.transform;
            Bullet b_comp=bullet.GetComponent<Bullet>();b_comp.currentColorId=startingColorId;
            Rigidbody2D b_rb=bullet.GetComponent<Rigidbody2D>();

            Vector3 _dir=gunPoint.position-b_trans.position;
            Vector3 _rotation=b_trans.position-gunPoint.position;
            b_rb.velocity=new Vector2(_dir.x, _dir.y).normalized*bulletSpeed;
            float b_rotZ = Mathf.Atan2(_rotation.y,_rotation.x)*Mathf.Rad2Deg;
            b_trans.rotation=Quaternion.Euler(0,0,b_rotZ+b_correctionAngle);
            if(startingColorId!=0){AudioManager.instance.Play("ShootNegative");AudioManager.instance.StopPlaying("ShootPositive");}
            else{AudioManager.instance.Play("ShootPositive");AudioManager.instance.StopPlaying("ShootNegative");}
        }
        if(!canFire){
            if(timer>0)timer-=Time.deltaTime;
            if(timer<=0){canFire=true;}
        }

        ///Change bullet charge
        if(Input.GetMouseButtonDown(1)){
            if(startingColorId==0){ChangeColor(1);return;}
            else{ChangeColor(0);return;}
        }
    }
    void ChangeColor(int id){
        startingColorId=id;
        gunSpr.sprite=gunSprColor[id].spr;
        gunTransform.GetComponentInChildren<Light2D>().color=gunSprColor[id].color;
        if(id==0){AudioManager.instance.Play("GunChangePositive");AudioManager.instance.StopPlaying("GunChangeNegative");}
        else{AudioManager.instance.Play("GunChangeNegative");AudioManager.instance.StopPlaying("GunChangePositive");}
        return;
    }
}
