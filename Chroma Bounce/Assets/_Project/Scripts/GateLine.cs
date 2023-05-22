using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class GateLine : MonoBehaviour{
    UILineRenderer uiLine;
    void Start(){
        uiLine=GetComponent<UILineRenderer>();
    }
    public void SetPoints(Vector2 point1, Vector2 point2){
        uiLine.Points[0]=new Vector2(point1.x,point1.y);
        uiLine.Points[1]=new Vector2(point2.x,point2.y);
        uiLine.SetAllDirty();
    }
    public void SetBothPointsNull(){
        uiLine.Points[0]=Vector2.zero;
        uiLine.Points[1]=Vector2.zero;
        uiLine.SetAllDirty();
    }
    public Vector2 GetPoint(int id){return uiLine.Points[id];}
    public bool BothPointsNull(){return GetPoint(0)==Vector2.zero&&GetPoint(1)==Vector2.zero;}
    public void SetColor(Color c){
        if(AssetsManager.instance.GetMat("HolographDissolve")!=null){uiLine.material=Instantiate(AssetsManager.instance.GetMat("HolographDissolve"));}
        uiLine.material.SetColor("Color_7C878D04",new Color(c.r,c.g,c.b,(170f/255f)));
        //uiLine.color=new Color(c.r,c.g,c.b,(170f/255f));
        //uiLine.CrossFadeColor(new Color(c.r,c.g,c.b,(170f/255f)),0.05f,true,false);
        uiLine.SetAllDirty();
        uiLine.GraphicUpdateComplete();
        uiLine.OnRebuildRequested();
    }
    public bool CompareColors(Color c){Color _c=uiLine.material.GetColor("Color_7C878D04");return (_c.r==c.r && _c.g==c.g && _c.b==c.b);}
}
