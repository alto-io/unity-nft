using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Collections.Generic;


// TODO
// Clean up this shit code

namespace OPGames.NFT
{

[System.Serializable]
public class PVPPlayerModel
{
	public string DisplayName;
	public string PlayFabId;
	public string Defense;
}

public class PlayFabManager : MonoBehaviour
{
	// Implemented as a singleton for easy access
	static private PlayFabManager instance = null;
	static public PlayFabManager Instance { get { return instance; } }

	public bool IsLoggedIn { get; private set; }
	public string DisplayName { get; private set; }

	private string playFabId;
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
		var parameters = new GetPlayerCombinedInfoRequestParams
		{
			GetPlayerStatistics = true,
			GetPlayerProfile = true,
			GetUserData = true,
		};

		var request = new LoginWithCustomIDRequest 
		{ 
			// TODO: replace this. it can't be just the wallet address
			CustomId = _Config.Account, 
			CreateAccount = true,
			InfoRequestParameters = parameters,
		};
		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnPlayFabError);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		IsLoggedIn = true;

		playFabId = result.PlayFabId;
		entityId = result.EntityToken.Entity.Id;
        entityType = result.EntityToken.Entity.Type;

		if (result.NewlyCreated)
		{
			PlayFabClientAPI.UpdateUserTitleDisplayName(
				new UpdateUserTitleDisplayNameRequest
				{
					DisplayName = _Config.Account
				},
				(result) => { Debug.LogFormat("Display name updated to {0}", result.DisplayName); },
				OnPlayFabError);

			SetInitialMMR();

			DisplayName = _Config.Account;
		}

		var payload = result.InfoResultPayload;
		if (payload != null && result.NewlyCreated == false)
		{
			DisplayName = payload.PlayerProfile.DisplayName;

			if (payload.UserData.ContainsKey("Offense"))
			{
				var list = ParseSquadData(payload.UserData["Offense"].Value);
				if (list != null)
					GameGlobals.Offense = list;
			}
			if (payload.UserData.ContainsKey("Defense"))
			{
				var list = ParseSquadData(payload.UserData["Defense"].Value);
				if (list != null)
					GameGlobals.Defense = list;
			}
		}
	}

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogError(error.GenerateErrorReport());
	}

	public void SaveToCloud(SaveData data)
	{
		if (IsLoggedIn == false)
			return;

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

	public void RequestMatchmaking(Action<PVPPlayerModel> resultCallback, Action<string> errorCallback)
	{
		var request = new GetLeaderboardAroundPlayerRequest 
		{
				StatisticName = "MMR",
				PlayFabId = playFabId,
				MaxResultsCount = 20
		};

		PlayFabClientAPI.GetLeaderboardAroundPlayer(
			request,
			(result) => 
			{
				result.Leaderboard.RemoveAll((x) => x.PlayFabId == this.playFabId);
				int len   = result.Leaderboard.Count;
				int index = UnityEngine.Random.Range(0, len);
				var entry = result.Leaderboard[index];

				Debug.LogFormat("Getting data for {0} {1}", entry.PlayFabId, entry.DisplayName);
				GetUserInternalData(
					entry.PlayFabId, 
					(result) =>
					{
						if (string.IsNullOrEmpty(result) == false)
						{
							resultCallback(new PVPPlayerModel
								{
									DisplayName = entry.DisplayName,
									PlayFabId = entry.PlayFabId,
									Defense = result
								});
						}
						else
						{
							errorCallback("No defensive lineup");
						}
					});
			}, 
			OnPlayFabError);
	}

	public void GetUserInternalData(string id, Action<string> defenseResult) 
	{
		string defenseVal = "";
		PlayFabClientAPI.GetUserData(
			new GetUserDataRequest() { PlayFabId = id },
			result => 
			{
				if (result.Data == null)
					return;

				foreach (var kvp in result.Data)
				{
					if (kvp.Key != "Defense")
						continue;

					defenseVal = kvp.Value.Value;
					break;
				}

				if (defenseResult != null)
					defenseResult(defenseVal);
			},
			OnPlayFabError);
	}

	private void SetInitialMMR()
	{
		List<StatisticUpdate> list = new List<StatisticUpdate>();
		list.Add(new StatisticUpdate
			{
				StatisticName = "MMR",
				Value = 1000
			});

		PlayFabClientAPI.UpdatePlayerStatistics(
			new UpdatePlayerStatisticsRequest { Statistics = list },
			null,
			null);
	}

	private List<GameGlobals.SelectedInfo> ParseSquadData(string json)
	{
		var temp = JsonUtility.FromJson<SaveDataSelectedList>(json);
		if (temp != null)
			return temp.List;

		return null;
	}
}

}
