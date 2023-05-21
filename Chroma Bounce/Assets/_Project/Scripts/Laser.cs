using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Laser : MonoBehaviour{
    [SerializeField]SpriteNColor[] spritesAndColors;
    [SerializeField]int currentColorId;

    SpriteRenderer spr;
    void Start(){
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
}
