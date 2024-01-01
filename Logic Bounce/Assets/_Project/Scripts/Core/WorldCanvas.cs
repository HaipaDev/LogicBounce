using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvas : MonoBehaviour{public static WorldCanvas instance;
    void Awake(){if(WorldCanvas.instance!=null){Destroy(gameObject);}else{instance=this;}}
    public void Cleanup(){
        for(var i=transform.childCount-1;i>=0;i--){Destroy(transform.GetChild(i).gameObject);}
    }
}
