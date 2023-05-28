
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName="LevelMap")]
public class LevelMap : SerializedScriptableObject{
    public GameObject parent;
    public int stepEnergy=6;
    public bool startingChargePositive=true;
    public float defaultGunRotation=45;
    public bool accurateGunRotation=false;
    public int bulletBounceLimit=10;
    public float bulletSpeed=6f;
    public dir playerDir=dir.down;
    public List<StepProperties> defaultSteps;
    public List<LevelRankCritiria> levelRankCritiria;
    [DictionaryDrawerSettings(KeyLabel = "Type", ValueLabel = "IsAllowed")]
    public Dictionary<StepPropertiesType,bool> allowedStepTypes=new Dictionary<StepPropertiesType,bool>(){
        {StepPropertiesType.delay,true}
        ,{StepPropertiesType.gunShoot,true}
        ,{StepPropertiesType.gunPolarity,true}
        ,{StepPropertiesType.gunRotation,true}
        ,{StepPropertiesType.mirrorPos,false}
    };
    [DictionaryDrawerSettings(KeyLabel = "Type", ValueLabel = "Cost")]
    public Dictionary<StepPropertiesType,int> stepTypesCosts=new Dictionary<StepPropertiesType,int>();
}
public class LevelRankCritiria{
    public LevelRankAchieved rank;
    public int energyUsedMax;
    public float timeToCompletion;
}