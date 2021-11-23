using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class PlayFabManager : MonoBehaviour
{
	// Implemented as a singleton for easy access
	static private PlayFabManager instance = null;
	static public PlayFabManager Instance { get { return instance; } }

	private string entityId;
	private string entityType;

	// Make sure to have only one instance of this class.
	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	public void Start()
	{
		var request = new LoginWithCustomIDRequest { CustomId = _Config.Account, CreateAccount = true};
		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnPlayFabError);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		Debug.Log("Login success");

		entityId = result.EntityToken.Entity.Id;
        entityType = result.EntityToken.Entity.Type;
	}

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogError(error.GenerateErrorReport());
	}

	public void SaveToCloud(SaveData data)
	{
		var offense = new SaveDataSelectedList() { List = data.Offense };
		var defense = new SaveDataSelectedList() { List = data.Defense };

		// private
		PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
			Data = new Dictionary<string, string>() {
				{"Offense", JsonUtility.ToJson(offense)},
			},
		},
		result => Debug.Log("Successfully updated user data"),
		OnPlayFabError);

		// public
		PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
			Data = new Dictionary<string, string>() {
				{"Defense", JsonUtility.ToJson(defense)},
			},
			Permission = UserDataPermission.Public
		},
		result => Debug.Log("Successfully updated user data"),
		OnPlayFabError);
	}
}

}
