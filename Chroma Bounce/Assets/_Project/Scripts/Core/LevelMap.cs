
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName="LevelMap")]
public class LevelMap : SerializedScriptableObject{
    public GameObject parent;
    public int stepEnergy=6;
    public bool startingChargePositive=true;
    [Range(0,360)]public float defaultGunRotation=45;
    public bool accurateGunRotation=false;
    public int bulletBounceLimit=10;
    public float bulletSpeed=6f;
    public float bulletAcceleration=0f;
    public bool bulletAccelerationMultiply=false;
    public bool bulletMaxSpeedInfinite=true;
    [EnableIf("@this.bulletMaxSpeedInfinite==false")]public float bulletMaxSpeed=0;
    public List<StepProperties> defaultSteps;
    public List<LevelRankCritiria> levelRankCritiria;
    [DictionaryDrawerSettings(KeyLabel = "Type", ValueLabel = "IsAllowed")]
    public Dictionary<StepPropertiesType,bool> allowedStepTypes=new Dictionary<StepPropertiesType,bool>(){
        {StepPropertiesType.delay,true}
        ,{StepPropertiesType.gunShoot,true}
        ,{StepPropertiesType.gunPolarity,true}
        ,{StepPropertiesType.gunRotation,true}
        ,{StepPropertiesType.mirrorPos,false}
        ,{StepPropertiesType.laserPos,false}
        ,{StepPropertiesType.switchAllLasers,false}
    };
    [DictionaryDrawerSettings(KeyLabel = "Type", ValueLabel = "Cost")]
    public Dictionary<StepPropertiesType,int> stepTypesCosts=new Dictionary<StepPropertiesType,int>();
    public List<StoryboardText> storyboardText;
}
public class LevelRankCritiria{
    public LevelRankAchieved rank;
    public int energyUsedMax;
    public float timeToCompletion;
}