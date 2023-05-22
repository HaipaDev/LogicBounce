using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvas : MonoBehaviour{public static WorldCanvas instance;
    void Awake(){if(WorldCanvas.instance!=null){Destroy(gameObject);}else{instance=this;}}
}
