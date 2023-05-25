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
    [SerializeField] SpriteNColor gunSpritencolor_positive;
    [SerializeField] SpriteNColor gunSpritencolor_negative;
    [SerializeField] Sprite[] playerSprites;
    
    [Header("Variables")]
    [SerializeField][Range(0,360)] int rotZmin=180;
    [SerializeField][Range(0,360)] int rotZmax=360;
    [SerializeField] public bool positive=true;
    [SerializeField] float timeBetweenFiring=1f;
    [SerializeField]public float bulletSpeed=7f;
    [SerializeField]public float b_correctionAngle=-90;
    [SerializeField]public int bulletBounceLimit=10;
    [Header("Current")]
    [DisableInEditorMode][SerializeField] bool canFire=true;
    [DisableInEditorMode][SerializeField] float timer;

    Vector3 mousePos;
    SpriteRenderer gunSpr;
    void Awake(){if(Player.instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        canFire=true;timer=timeBetweenFiring;
        gunSpr=gunTransform.GetComponent<SpriteRenderer>();
    }
    Vector2 rotZdirectionMin,rotZdirectionMax;
    void Update(){
        if(!GameManager.GlobalTimeIsPausedOrStepped){
            ///Rotating and limiting
            mousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 rot=mousePos-rotatePoint.position;
            float rotZ=Mathf.Repeat(Mathf.Atan2(rot.y,rot.x)*Mathf.Rad2Deg,360f);
            //Vector3 rotMouse=Quaternion.Euler(0,0,(rotZ/360f)*360f);

            rotZdirectionMin=new Vector2(Mathf.Cos(rotZmin*Mathf.Deg2Rad),Mathf.Sin(rotZmin*Mathf.Deg2Rad));
            rotZdirectionMax=new Vector2(Mathf.Cos(rotZmax*Mathf.Deg2Rad),Mathf.Sin(rotZmax*Mathf.Deg2Rad));

            rotatePoint.rotation=Quaternion.Euler(0,0,rotZ);
            Vector3 euler=rotatePoint.eulerAngles;
            //For moving above limit between halfpoint
            if((euler.z<(rotZmin-90)&&rotZmin>=90)||(euler.z<(rotZmax+90)&&rotZmin==0)){euler.z=rotZmax;}
            /*if(rotZmin>0f&&euler.z<(rotZmin-90f)){euler.z=rotZmin;}
            else if(rotZmin==0f&&euler.z<(rotZmax+90f)){euler.z=rotZmax;}*/
            
            Debug.Log(euler.z+" | "+rotZ);
            euler.z=Mathf.Clamp(euler.z,rotZmin,rotZmax);
            Debug.Log(euler.z);
            rotatePoint.eulerAngles=euler;
            ///Rotate gun sprite
            if(euler.z<((rotZmin+rotZmax)/2)){gunSpr.flipY=true;}else{gunSpr.flipY=false;}


            ///Shooting
            if(Input.GetMouseButton(0)&&canFire){
                canFire=false;timer=timeBetweenFiring;
                
                GameObject bullet=Instantiate(bulletPrefab,gunTransform.position,Quaternion.identity);
                Transform gunPoint=gunTransform.GetChild(0);
                Transform b_trans=bullet.transform;
                Bullet b_comp=bullet.GetComponent<Bullet>();b_comp.positive=positive;
                Rigidbody2D b_rb=bullet.GetComponent<Rigidbody2D>();

                Vector3 _dir=gunPoint.position-b_trans.position;
                Vector3 _rotation=b_trans.position-gunPoint.position;
                b_rb.velocity=new Vector2(_dir.x, _dir.y).normalized*bulletSpeed;
                float b_rotZ = Mathf.Atan2(_rotation.y,_rotation.x)*Mathf.Rad2Deg;
                b_trans.rotation=Quaternion.Euler(0,0,b_rotZ+b_correctionAngle);
                if(positive){AudioManager.instance.Play("ShootPositive");AudioManager.instance.StopPlaying("ShootNegative");}
                else{AudioManager.instance.Play("ShootNegative");AudioManager.instance.StopPlaying("ShootPositive");}
            }
            if(!canFire){
                if(timer>0)timer-=Time.deltaTime;
                if(timer<=0){canFire=true;}
            }

            ///Change bullet charge
            if(Input.GetMouseButtonDown(1)){
                SwitchPolarity();
            }
        }
    }
    void OnDrawGizmos(){
        Gizmos.color=Color.blue;
        Gizmos.DrawRay(transform.position,rotZdirectionMin*2.5f);
        Gizmos.color=Color.red;
        Gizmos.DrawRay(transform.position,rotZdirectionMax*2.5f);
    }
    public void SwitchPolarity(){SetPolarity(!positive);return;}
    public void SetPolarity(bool _positive=true){
        positive=_positive;
        if(positive){
            gunSpr.sprite=gunSpritencolor_positive.spr;
            if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=gunSpritencolor_positive.color;
            AudioManager.instance.Play("GunChangePositive");AudioManager.instance.StopPlaying("GunChangeNegative");
        }else{
            gunSpr.sprite=gunSpritencolor_negative.spr;
            if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=gunSpritencolor_negative.color;
            AudioManager.instance.Play("GunChangeNegative");AudioManager.instance.StopPlaying("GunChangePositive");
        }
    }
    public void SetDirection(dir dir){
        if(playerSprites.Length>(int)dir){GetComponent<SpriteRenderer>().sprite=playerSprites[(int)dir];}
        switch((int)dir){
            case 0://Up
                rotZmin=0;
                rotZmax=180;
            break;
            case 1://Down
                rotZmin=180;
                rotZmax=360;
            break;
            case 2://Left
                rotZmin=90;
                rotZmax=270;
            break;
            case 3://Right
                rotZmin=270;
                rotZmax=90;
            break;
            default://1 - down
                rotZmin=180;
                rotZmax=360;
            break;
        }
    }
}
