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
	public string Wallet;
	public string PlayFabId;
	public string Defense;
	public int MMR;
}

public class PlayFabManager : MonoBehaviour
{
	// Implemented as a singleton for easy access
	static private PlayFabManager instance = null;
	static public PlayFabManager Instance { get { return instance; } }

	public bool IsLoggedIn { get; private set; }
	public string DisplayName { get; private set; }
	public string PlayFabId { get { return playFabId; } }
	public bool IsNewPlayer { get; private set; }
	public int MMR { get; private set; }

	private string playFabId;
	private string entityId;
	private string entityType;

	public List<PlayerLeaderboardEntry> Leaderboard = new List<PlayerLeaderboardEntry>();

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

		string wallet = _Config.Account;
#if UNITY_EDITOR
		wallet = SystemInfo.deviceUniqueIdentifier;
#endif

		var request = new LoginWithCustomIDRequest 
		{ 
			// TODO: replace this. it can't be just the wallet address
			CustomId = wallet,
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

		Debug.Log("Login success");

		if (result.NewlyCreated)
		{
			IsNewPlayer = true;
			SetDisplayName(_Config.Account);

			MMR = 1000;

			UIManager.Close();
			UIManager.Open(UIType.EnterName);
		}
		else
		{
			UIManager.Close();
			UIManager.Open(UIType.MainMenu);
		}

		var payload = result.InfoResultPayload;
		if (payload != null && result.NewlyCreated == false)
		{
			if (payload.PlayerProfile != null)
				DisplayName = payload.PlayerProfile.DisplayName;

			if (string.IsNullOrEmpty(DisplayName))
				SetDisplayName(_Config.Account);

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
			if (payload.PlayerStatistics != null)
			{
				foreach (var s in payload.PlayerStatistics)
				{
					if (s.StatisticName == "MMR")
					{
						MMR = s.Value;
					}
				}
			}
		}

		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
			{
				FunctionName = "setInitialMMR",
			},
			(result) => Debug.Log(result.ToString()),
			OnPlayFabError);

	}

	public void SetDisplayName(string newName)
	{
		if (string.IsNullOrEmpty(newName))
			return;

		if (newName.Length > 25)
			newName = newName.Substring(0,24);

		PlayFabClientAPI.UpdateUserTitleDisplayName(
			new UpdateUserTitleDisplayNameRequest
			{
				DisplayName = newName
			},
			(result) => { Debug.LogFormat("Display name updated to {0}", result.DisplayName); },
			OnPlayFabError);

		DisplayName = newName;
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
			MaxResultsCount = 40
		};

		PlayFabClientAPI.GetLeaderboardAroundPlayer(
			request,
			(result) => 
			{
				var player = result.Leaderboard.Find((x) => x.PlayFabId == this.playFabId);
				if (player != null)
				{
					MMR = player.StatValue;
					result.Leaderboard.Remove(player);
				}

				int len = result.Leaderboard.Count;

				if (len <= 0)
				{
					if (errorCallback != null)
						errorCallback("No other players");

					return;
				}

				int index = UnityEngine.Random.Range(0, len);
				var entry = result.Leaderboard[index];

				Debug.LogFormat("Getting data for {0} {1}", entry.PlayFabId, entry.DisplayName);
				GetUserInternalData(
					entry.PlayFabId, 
					(result) =>
					{
						var model = new PVPPlayerModel
							{
								DisplayName = entry.DisplayName,
								PlayFabId = entry.PlayFabId,
								Defense = result,
								MMR = entry.StatValue
							};

						resultCallback(model);
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

	private List<GameGlobals.SelectedInfo> ParseSquadData(string json)
	{
		var temp = JsonUtility.FromJson<SaveDataSelectedList>(json);
		if (temp != null)
			return temp.List;

		return null;
	}

	public void RequestLeaderboard(Action onDone)
	{
		Leaderboard.Clear();
		RequestLeaderboardAroundPlayer(() =>
		{
			RequestLeaderboardTop(() =>
			{
				if (onDone != null)
					onDone();

				Leaderboard.Sort((a,b) => (a.Position - b.Position));
			});
		});
	}

	private void RequestLeaderboardAroundPlayer(Action onDone)
	{
		var request = new GetLeaderboardAroundPlayerRequest 
		{
			StatisticName = "MMR",
			PlayFabId = playFabId,
			MaxResultsCount = 40
		};

		PlayFabClientAPI.GetLeaderboardAroundPlayer(
			request,
			(result) =>
			{
				foreach (var entry in result.Leaderboard)
				{
					if (entry == null) continue;
					Leaderboard.Add(entry);
				}
				if (onDone != null) onDone();
			},
			(error) => 
			{
				OnPlayFabError(error);
				if (onDone != null) onDone();
			});
	}

	private void RequestLeaderboardTop(Action onDone)
	{
		var request = new GetLeaderboardRequest 
		{
			StatisticName = "MMR",
			StartPosition = 0,
			MaxResultsCount = 50
		};

		PlayFabClientAPI.GetLeaderboard(
			request,
			(result) =>
			{
				foreach (var entry in result.Leaderboard)
				{
					if (entry == null) continue;
					var found = Leaderboard.Find(l => l.PlayFabId == entry.PlayFabId);
					if (found != null) continue;

					Leaderboard.Add(entry);
				}
				if (onDone != null) onDone();
			},
			(error) => 
			{
				OnPlayFabError(error);
				if (onDone != null) onDone();
			});
	}

	public void SetBattleResult(string enemyIdVal, bool isWinVal)
	{
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
			{
				FunctionName = "battleResult",
				FunctionParameter = new { enemyId = enemyIdVal, isWin = isWinVal }
			},
			(result) => Debug.Log(result.ToString()),
			OnPlayFabError);
	}
}

}
