using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Bullet : MonoBehaviour{
    [Header("References")]
    [SerializeField] SpriteNColor spritencolor_positive;
    //[SerializeField] Material material_positive;
    [SerializeField] SpriteNColor spritencolor_negative;
    //[SerializeField] Material material_negative;
    [Header("Variables")]
    public bool positive=true;
    public int bounceCountLaser=0;
    public int bounceCountWall=0;
    public int bounceCountTotal=0;
    [SerializeField] float bulletSpeedBase=6f;
    [SerializeField] float bulletSpeed;
    float bounceCountDelay=0.05f;
    float bounceCountDelayTimer;

    Rigidbody2D rb;
    SpriteRenderer spr;
    void Start(){
        rb=GetComponent<Rigidbody2D>();
        spr=GetComponent<SpriteRenderer>();
        SetPolarity(positive,true,true);
        bulletSpeedBase=LevelMapManager.instance.GetCurrentLevelMap().bulletSpeed;
        bulletSpeed=bulletSpeedBase;
    }
    void Update(){
        //if(!GameManager.GlobalTimeIsPaused)
        if(Vector2.Distance(transform.position,Vector2.zero)>15f){Destroy(gameObject);}///Cleanup
        if(LevelMapManager.instance.GetCurrentLevelMap().bulletBounceLimit>=0 && bounceCountTotal>0){///Limit bounces
            float _alpha=1f-(float)bounceCountTotal/LevelMapManager.instance.GetCurrentLevelMap().bulletBounceLimit;
            //spr.color=new Color(1,1,1,_alpha);
            GetComponentInChildren<Light2D>().color=new Color(GetComponentInChildren<Light2D>().color.r,GetComponentInChildren<Light2D>().color.g,GetComponentInChildren<Light2D>().color.b,_alpha);
            if(bounceCountTotal>LevelMapManager.instance.GetCurrentLevelMap().bulletBounceLimit){DestroyBullet();}
        }
        if(bounceCountDelayTimer>0){bounceCountDelayTimer-=Time.deltaTime;}
    }
    void DestroyBullet(){
        //if(positive)AssetsManager.instance.VFX("BulletDestroyPositive",transform.position,0.15f);
        //else AssetsManager.instance.VFX("BulletDestroyNegative",transform.position,0.15f);
        Destroy(gameObject);
    }
    public void SwitchPolarity(){SetPolarity(!positive);return;}
    public void SetPolarity(bool _positive=true,bool force=true,bool quiet=false){
        if(spr==null){spr=GetComponent<SpriteRenderer>();}
        if(positive!=_positive||force){
            positive=_positive;
            if(positive){
                spr.sprite=spritencolor_positive.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_positive.color;
                if(!quiet){AudioManager.instance.Play("BulletChangePositive");AudioManager.instance.StopPlaying("BulletChangeNegative");}
            }else{
                spr.sprite=spritencolor_negative.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_negative.color;
                if(!quiet){AudioManager.instance.Play("BulletChangeNegative");AudioManager.instance.StopPlaying("BulletChangePositive");}
            }
        }
    }
    void OnCollisionEnter2D(Collision2D other){
        if(other.gameObject.CompareTag("Laser")){
            if(other.gameObject.GetComponent<Laser>().positive!=this.positive&&other.gameObject.GetComponent<Laser>().bouncy){
                Ricochet(other);bounceCountLaser++;
                SwitchPolarity();
                AudioManager.instance.Play("BounceLaser");
                return;
            }
        }else if(other.gameObject.CompareTag("World")){
            Ricochet(other);bounceCountWall++;
            AudioManager.instance.Play("Bounce");
            return;
        }
        void Ricochet(Collision2D other){
            if(LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed==-1||
                (LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed!=-1 && 
                    (
                        (bulletSpeed<LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed && LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed>0)||
                        (bulletSpeed>LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed && LevelMapManager.instance.GetCurrentLevelMap().bulletMaxSpeed<0)
                    )
                )
            ){
                if(LevelMapManager.instance.GetCurrentLevelMap().bulletAccelerationMultiply){
                    bulletSpeed*=LevelMapManager.instance.GetCurrentLevelMap().bulletAcceleration;
                }else{
                    bulletSpeed+=LevelMapManager.instance.GetCurrentLevelMap().bulletAcceleration;
                }
            }

            Vector2 _wallNormal=other.contacts[0].normal;
            Vector2 reflectDir=Vector2.Reflect(rb.velocity,_wallNormal).normalized;

            rb.velocity=reflectDir.normalized*bulletSpeed;
            float rot=Mathf.Atan2(reflectDir.y,reflectDir.x)*Mathf.Rad2Deg;
            transform.eulerAngles=new Vector3(0,0,rot-Player.instance.b_correctionAngle);

            if(bounceCountDelayTimer<=0){
                bounceCountTotal++;
                bounceCountDelayTimer=bounceCountDelay;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("LogicGate")){
            if(positive){other.gameObject.GetComponent<LogicGate>().Charge();return;}
            else{other.gameObject.GetComponent<LogicGate>().Discharge();return;}
        }else if(other.gameObject.CompareTag("Laser")){
            if(other.gameObject.GetComponent<Laser>().positive!=this.positive&&!other.gameObject.GetComponent<Laser>().bouncy){
                SwitchPolarity();
                //AudioManager.instance.Play("BounceLaser");
                return;
            }
        }else if(other.gameObject.GetComponent<Boombox>()!=null){
            if(positive){other.gameObject.GetComponent<Boombox>().EnableAndPlay();}
            else{other.gameObject.GetComponent<Boombox>().Disable();}
        }
    }
}
