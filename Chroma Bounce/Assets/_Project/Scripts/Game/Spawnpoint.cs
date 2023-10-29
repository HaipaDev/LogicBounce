using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour{
    [SerializeField]public Vector2 playerOffset=new Vector2(0,0.5f);
    void Start(){
        //if(GSceneManager.CheckScene("Game"))
        if(GSceneManager.CheckScene("Game")){if(Player.instance==null){Instantiate(CoreSetup.instance._getPlayerPrefab(),(Vector2)transform.position+playerOffset,Quaternion.identity);}}
        else{var _player=Instantiate(CoreSetup.instance._getPlayerPrefab(),this.transform);_player.transform.position=(Vector2)transform.position+playerOffset;}
    }
}