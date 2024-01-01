using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleColliderToParticle : MonoBehaviour{
    [SerializeField] float maxSize=20f;
    ParticleSystem ps;CircleCollider2D cc2d;
    void Start(){
        ps=GetComponent<ParticleSystem>();
        cc2d=GetComponent<CircleCollider2D>();
    }
    void Update(){
        /*float currentSize=ps.sizeOverLifetime.size.Evaluate(0f);
        cc2d.radius=currentSize;*/
        cc2d.radius=(ps.time / ps.main.duration)*maxSize;
    }
}
