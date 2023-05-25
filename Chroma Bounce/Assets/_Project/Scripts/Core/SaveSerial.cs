using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;

public class SaveSerial : MonoBehaviour{	public static SaveSerial instance;
	void Awake(){if(instance!=null){Destroy(gameObject);}else{instance=this;DontDestroyOnLoad(gameObject);gameObject.name=gameObject.name.Split('(')[0];}}
	[SerializeField] string filename = "playerData";
	[SerializeField] string filenameSettings = "settings";
	void Start(){
		RecreatePlayerData();
	}

#region//Player Data
	public PlayerData playerData=new PlayerData();
	public float buildFirstLoaded;
	public float buildLastLoaded;
	[System.Serializable]public class PlayerData{
		public LevelPassValues[] levelPassedValues;
	}

	public string _playerDataPath(){return Application.persistentDataPath+"/"+filename+".hyper";}
	public void Save(){
        var settings=new ES3Settings(_playerDataPath(),ES3.EncryptionType.None);
		if(!ES3.KeyExists("buildFirstLoaded",settings)){buildFirstLoaded=GameManager.instance.buildVersion;ES3.Save("buildFirstLoaded",buildFirstLoaded,settings);}
		buildLastLoaded=GameManager.instance.buildVersion;ES3.Save("buildLastLoaded",buildLastLoaded,settings);
		ES3.Save("playerData",playerData,settings);
		Debug.Log("Game Data saved");
	}
	public void Load(){
		if(ES3.FileExists(_playerDataPath())){
			var settings=new ES3Settings(_playerDataPath(),ES3.EncryptionType.None);

			if(ES3.KeyExists("buildFirstLoaded",settings)){buildFirstLoaded=ES3.Load<float>("buildFirstLoaded",8,settings);}
			else{Debug.LogWarning("Key for buildFirstLoaded not found in: "+_playerDataPath());}

			if(ES3.KeyExists("buildLastLoaded",settings)){buildLastLoaded=ES3.Load<float>("buildLastLoaded",8,settings);}
			else{Debug.LogWarning("Key for buildLastLoaded not found in: "+_playerDataPath());}

			if(ES3.KeyExists("playerData",settings)){ES3.LoadInto<PlayerData>("playerData",playerData,settings);}
			else{Debug.LogWarning("Key for playerData not found in: "+_playerDataPath());}
			//var hi=-1;foreach(int h in playerData.highscore){hi++;if(h!=0)playerData.highscore[hi]=h;}
			Debug.Log("Game Data loaded");
		}else Debug.LogWarning("Game Data file not found in: "+_playerDataPath());
	}
	public void Delete(){
		playerData=new PlayerData();
		RecreatePlayerData();
		Debug.Log("Game Data reset");
		GC.Collect();
		if(ES3.FileExists(_playerDataPath())){
			ES3.DeleteFile(_playerDataPath());
			Debug.Log("Game Data deleted!");
		}
	}
	public void RecreatePlayerData(){
		if(LevelMapManager.instance!=null){
			if(playerData.levelPassedValues.Length==0||playerData.levelPassedValues==null){
				playerData.levelPassedValues=new LevelPassValues[LevelMapManager.instance.levelMaps.Length];
				for(var l=0;l<playerData.levelPassedValues.Length;l++){playerData.levelPassedValues[l]=new LevelPassValues();}
			}
		}else{
			if(playerData.levelPassedValues.Length==0||playerData.levelPassedValues==null){
				playerData.levelPassedValues=new LevelPassValues[CoreSetup.instance._levelMapManagerPrefab().GetComponent<LevelMapManager>().levelMaps.Length];
				for(var l=0;l<playerData.levelPassedValues.Length;l++){playerData.levelPassedValues[l]=new LevelPassValues();}
			}
		}
		//playerData.achievsCompleted=new AchievData[StatsAchievsManager._AchievsListCount()];
	}
#endregion
#region//Settings Data
	public SettingsData settingsData;
	[System.Serializable]public class SettingsData{
		public float masterVolume=0.95f;
		public float masterOOFVolume=0.25f;
		public float soundVolume=0.95f;
		public float musicVolume=0.66f;
		
		
		public int windowMode=1;
		public Vector2Int resolution=new Vector2Int(1920,1080);
		public bool vSync=false;
		public bool lockCursor=false;
		public bool pauseWhenOOF=true;
		public bool pprocessing=true;
		public bool screenshake=true;
	}
	
	public string _settingsDataPath(){return Application.persistentDataPath+"/"+filenameSettings+".json";}
	public void SaveSettings(){
		var settings=new ES3Settings(_settingsDataPath(),ES3.EncryptionType.None);
		ES3.Save("settingsData",settingsData,settings);
		Debug.Log("Settings saved");
	}
	public void LoadSettings(){
		if(ES3.FileExists(_settingsDataPath())){
		var settings=new ES3Settings(_settingsDataPath(),ES3.EncryptionType.None);
			if(ES3.KeyExists("settingsData",settings)){ES3.LoadInto<SettingsData>("settingsData",settingsData,settings);}
			else{Debug.LogWarning("Key for settingsData not found in: "+_settingsDataPath());}
		}else Debug.LogWarning("Settings file not found in: "+_settingsDataPath());
	}
	public void DeleteSettings(){
		settingsData=new SettingsData();
		GC.Collect();
		if(ES3.FileExists(_settingsDataPath())){
			ES3.DeleteFile(_settingsDataPath());
			Debug.Log("Settings deleted");
		}
	}
#endregion
}

[System.Serializable]
public class LevelPassValues{
	public bool passed;
}