using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomParticles : MonoBehaviour{
    [SerializeField] Material[] materials;
    [SerializeField] float delayBetweenChange=0.1f;
    [SerializeField] float delayBetweenChangeTimer;
    void Start(){
        delayBetweenChangeTimer=delayBetweenChange;
    }

    void Update(){
        if(delayBetweenChangeTimer>0){
            delayBetweenChangeTimer-=Time.deltaTime;
        }else{
            var rend=GetComponent<ParticleSystemRenderer>();
            rend.material = materials[Random.Range(0,materials.Length)];
            delayBetweenChangeTimer=delayBetweenChange;
        }
    }
}
