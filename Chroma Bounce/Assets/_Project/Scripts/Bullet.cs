using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Bullet : MonoBehaviour{
    [SerializeField]SpriteNColor[] spritesAndColors;
    public int currentColorId;

    Rigidbody2D rb;
    SpriteRenderer spr;
    void Start(){
        rb=GetComponent<Rigidbody2D>();
        spr=GetComponent<SpriteRenderer>();
        ChangeColor(currentColorId);
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
    void OnCollisionEnter2D(Collision2D other){
        if(other.gameObject.CompareTag("Laser1")&&currentColorId==1){
            Ricochet(other);
            SwitchColor();return;
        }else if(other.gameObject.CompareTag("Laser2")&&currentColorId==0){
            Ricochet(other);
            SwitchColor();return;
        }else if(other.gameObject.CompareTag("World")){
            Ricochet(other);return;
        }else if(other.gameObject.CompareTag("Enemy")){
            other.gameObject.GetComponent<Enemy>().Die();Destroy(gameObject);return;
        }
        void Ricochet(Collision2D other){
            Vector2 _wallNormal=other.contacts[0].normal;
            Vector2 reflectDir=Vector2.Reflect(rb.velocity,_wallNormal).normalized;

            rb.velocity=reflectDir.normalized*Player.instance.bulletSpeed;
            float rot=Mathf.Atan2(reflectDir.y,reflectDir.x)*Mathf.Rad2Deg;
            transform.eulerAngles=new Vector3(0,0,rot-Player.instance.b_correctionAngle);
        }
    }
}
