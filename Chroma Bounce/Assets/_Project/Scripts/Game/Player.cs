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
    [SerializeField][Range(0,360)] float currentAngle;
    [SerializeField] public bool positive=true;
    [SerializeField] float timeBetweenFiring=1f;
    [SerializeField]public float b_correctionAngle=-90;
    [Header("Current")]
    [DisableInEditorMode][SerializeField] float timer;

    Vector3 mousePos;
    SpriteRenderer gunSpr;
    void Awake(){if(Player.instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        gunSpr=gunTransform.GetComponent<SpriteRenderer>();
        SetPolarity(positive,true,true);
    }
    Vector2 rotZdirectionMin,rotZdirectionMax;
    void Update(){}
    public void SetGunRotation(float rotZ,bool _flipped=false){
        float _rotZ=360-rotZ;if(_flipped)_rotZ=rotZ;
        rotatePoint.eulerAngles=new Vector3(rotatePoint.eulerAngles.x,rotatePoint.eulerAngles.y,_rotZ);
        if(rotZ>90&&rotZ<270){GetComponent<SpriteRenderer>().flipX=true;}else{GetComponent<SpriteRenderer>().flipX=false;}
    }
    public void ShootBullet(){
        timer=timeBetweenFiring;

        GameObject bullet=Instantiate(bulletPrefab,gunTransform.position,Quaternion.identity);
        Transform gunPoint=gunTransform.GetChild(0);
        Transform b_trans=bullet.transform;
        Bullet b_comp=bullet.GetComponent<Bullet>();b_comp.SetPolarity(positive);
        Rigidbody2D b_rb=bullet.GetComponent<Rigidbody2D>();

        Vector3 _dir=gunPoint.position-b_trans.position;
        Vector3 _rotation=b_trans.position-gunPoint.position;
        b_rb.velocity=new Vector2(_dir.x, _dir.y).normalized*LevelMapManager.instance.GetCurrentLevelMap().bulletSpeed;
        float b_rotZ = Mathf.Atan2(_rotation.y,_rotation.x)*Mathf.Rad2Deg;
        b_trans.rotation=Quaternion.Euler(0,0,b_rotZ+b_correctionAngle);
        if(positive){AudioManager.instance.Play("ShootPositive");AudioManager.instance.StopPlaying("ShootNegative");}
        else{AudioManager.instance.Play("ShootNegative");AudioManager.instance.StopPlaying("ShootPositive");}
    }
    void OnDrawGizmos(){
        Gizmos.color=Color.blue;
        Gizmos.DrawRay(transform.position,rotZdirectionMin*2.5f);
        Gizmos.color=Color.red;
        Gizmos.DrawRay(transform.position,rotZdirectionMax*2.5f);
    }
    public void SwitchPolarity(){SetPolarity(!positive);return;}
    public void SetPolarity(bool _positive=true,bool force=true,bool quiet=false){
        if(gunSpr==null){gunSpr=gunTransform.GetComponent<SpriteRenderer>();}
        if(positive!=_positive||force){
            positive=_positive;
            if(positive){
                gunSpr.sprite=gunSpritencolor_positive.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=gunSpritencolor_positive.color;
                if(!quiet){AudioManager.instance.Play("GunChangePositive");AudioManager.instance.StopPlaying("GunChangeNegative");}
            }else{
                gunSpr.sprite=gunSpritencolor_negative.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=gunSpritencolor_negative.color;
                if(!quiet){AudioManager.instance.Play("GunChangeNegative");AudioManager.instance.StopPlaying("GunChangePositive");}
            }
        }
    }
    public Sprite GetGunSpr(bool positive=true){
        if(positive)return gunSpritencolor_positive.spr;
        else return gunSpritencolor_negative.spr;
    }
}
