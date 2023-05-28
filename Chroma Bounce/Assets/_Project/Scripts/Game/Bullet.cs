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

    Rigidbody2D rb;
    SpriteRenderer spr;
    void Start(){
        rb=GetComponent<Rigidbody2D>();
        spr=GetComponent<SpriteRenderer>();
        SetPolarity(positive);
    }
    void Update(){
        //if(!GameManager.GlobalTimeIsPaused)
        if(Vector2.Distance(transform.position,Vector2.zero)>15f){Destroy(gameObject);}///Cleanup
        if(Player.instance.bulletBounceLimit>0&&bounceCountTotal!=0){///Limit bounces
            float _alpha=1f-(float)bounceCountTotal/Player.instance.bulletBounceLimit;
            //spr.color=new Color(1,1,1,_alpha);
            GetComponentInChildren<Light2D>().color=new Color(GetComponentInChildren<Light2D>().color.r,GetComponentInChildren<Light2D>().color.g,GetComponentInChildren<Light2D>().color.b,_alpha);
            if(bounceCountTotal>=Player.instance.bulletBounceLimit){DestroyBullet();}
        }
    }
    void DestroyBullet(){
        //if(positive)AssetsManager.instance.VFX("BulletDestroyPositive",transform.position,0.15f);
        //else AssetsManager.instance.VFX("BulletDestroyNegative",transform.position,0.15f);
        Destroy(gameObject);
    }
    public void SwitchPolarity(){SetPolarity(!positive);return;}
    public void SetPolarity(bool _positive=true,bool force=true){
        if(spr==null){spr=GetComponent<SpriteRenderer>();}
        if(positive!=_positive||force){
            positive=_positive;
            if(positive){
                spr.sprite=spritencolor_positive.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_positive.color;
                AudioManager.instance.Play("BulletChangePositive");AudioManager.instance.StopPlaying("BulletChangeNegative");
            }else{
                spr.sprite=spritencolor_negative.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_negative.color;
                AudioManager.instance.Play("BulletChangeNegative");AudioManager.instance.StopPlaying("BulletChangePositive");
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
            Vector2 _wallNormal=other.contacts[0].normal;
            Vector2 reflectDir=Vector2.Reflect(rb.velocity,_wallNormal).normalized;

            rb.velocity=reflectDir.normalized*Player.instance.bulletSpeed;
            float rot=Mathf.Atan2(reflectDir.y,reflectDir.x)*Mathf.Rad2Deg;
            transform.eulerAngles=new Vector3(0,0,rot-Player.instance.b_correctionAngle);
            bounceCountTotal++;
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
        }
    }
}
