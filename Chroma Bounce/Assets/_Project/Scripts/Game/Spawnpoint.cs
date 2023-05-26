using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour{
    [SerializeField]public Vector2 playerOffset=new Vector2(0,0.5f);
    [SerializeField]public dir playerDir=dir.down;
    void Start(){
        if(Player.instance==null){Instantiate(CoreSetup.instance._getPlayerPrefab(),(Vector2)transform.position+playerOffset,Quaternion.identity);}
        Player.instance.SetDirection(playerDir);
    }
}