using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

//  For serializing to json
[System.Serializable]
public class SaveDataSelectedList
{
	public List<GameGlobals.SelectedInfo> List;
}

[System.Serializable]
public class SaveData
{
	public string Wallet;
	public List<GameGlobals.SelectedInfo> Offense;
	public List<GameGlobals.SelectedInfo> Defense;
}

public class SaveManager : MonoBehaviour
{
	static private SaveManager instance = null;
	static public SaveManager Instance 
	{
		get { return instance; }
	}

	private SaveData data = new SaveData();
	private bool loaded = false;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		Load();

		data.Offense = GameGlobals.Offense;
		data.Defense = GameGlobals.Defense;
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (loaded && isPaused)
			Save();
	}

	private void OnApplicationQuit()
	{
	}

	public void Load()
	{
		//loaded = true;
		//var str = PlayerPrefs.GetString("Save", "");
		//if (str == "")
		//	return;

		//data = JsonUtility.FromJson<SaveData>(str);
		//if (data == null)
		//	return;

		//GameGlobals.Offense = data.Offense;
		//GameGlobals.Defense = data.Defense;
		//Debug.LogFormat("Load: {0}", str);
	}

	public void Save()
	{
		data.Wallet = GameGlobals.Wallet;
		data.Offense = GameGlobals.Offense;
		data.Defense = GameGlobals.Defense;

		var str = JsonUtility.ToJson(data);
		PlayerPrefs.SetString("Save", str);
		Debug.LogFormat("Save: {0}", str);

		if (PlayFabManager.Instance != null)
			PlayFabManager.Instance.SaveToCloud(data);
	}
}

}

