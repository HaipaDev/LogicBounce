using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryVFX : MonoBehaviour{
    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player")||other.CompareTag("World")||other.CompareTag("LogicGate")||other.CompareTag("Laser")||other.CompareTag("Mirror"))Destroy(other.gameObject);
    }
}
