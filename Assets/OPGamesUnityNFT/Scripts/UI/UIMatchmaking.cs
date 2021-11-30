using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace OPGames.NFT
{

public class UIMatchmaking : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI playerName;
	[SerializeField] private TextMeshProUGUI enemyName;

	[SerializeField] private UINFTItem[] playerCards;
	[SerializeField] private UINFTItem[] enemyCards;

	private void OnEnable()
	{
		StartCoroutine(WaitForMatch());
	}

	private IEnumerator WaitForMatch()
	{
		yield return null;

		var pf = PlayFabManager.Instance;

		FillPlayer();

		PVPPlayerModel model = null;
		string errorMsg = null;

		pf.RequestMatchmaking(
			(result) =>
			{
				Debug.LogFormat("Got opponent {0}, {1}\n{2}",
						result.PlayFabId, result.DisplayName, result.Defense);
				model = result;
			},
			(error) => 
			{
				Debug.LogError(error);
				errorMsg = error;
			});

		while (model == null && errorMsg == null)
			yield return new WaitForSeconds(0.1f);

		if (model == null)
		{
			model = new PVPPlayerModel();
			model.DisplayName = "Bot";
		}

		GameGlobals.EnemyModel = model;
		FillEnemy(model);

		yield return new WaitForSeconds(3.0f);
		UIManager.Open(UIType.EditSquadOff);
		//SceneManager.LoadScene(2);
	}

	private string CreateRandomLineup()
	{
		var list = NFTManager.Instance.LoadedNFTs;
		int count = list.Count;

		List<int> indices = new List<int>();
		for (int i=0; i<count; i++)
		{
			indices.Add(i);
		}

		while (indices.Count > 3)
		{
			indices.RemoveAt(Random.Range(0,indices.Count));
		}

		var temp = new SaveDataSelectedList();
		temp.List = new List<GameGlobals.SelectedInfo>();

		for (int i=0; i<indices.Count; i++)
		{
			string id = list[indices[i]].UniqueId;
			Vector2Int pos = new Vector2Int(1+i, 0);

			var info = new GameGlobals.SelectedInfo();
			info.Id = id;
			info.Pos = pos;

			temp.List.Add(info);
		}

		return JsonUtility.ToJson(temp);
	}

	public void OnBtnBack()
	{
		UIManager.Close();
	}

	private void FillPlayer()
	{
		var pf = PlayFabManager.Instance;
		var nft = NFTManager.Instance;

		playerName.text = pf.DisplayName;

		for (int i=0; i<playerCards.Length; i++)
		{
			if (i >= GameGlobals.Offense.Count)
				break;

			var s = GameGlobals.Offense[i];

			playerCards[i].Fill(s.Id);
		}
	}

	private void FillEnemy(PVPPlayerModel model)
	{
		// TODO if using real nft, need to download images
		var nft = NFTManager.Instance;

		enemyName.text = model.DisplayName;

		var def = JsonUtility.FromJson<SaveDataSelectedList>(model.Defense);
		if (def == null || def.List == null || def.List.Count == 0)
		{
			model.Defense = CreateRandomLineup();
			def = JsonUtility.FromJson<SaveDataSelectedList>(model.Defense);
			if (def == null || def.List == null || def.List.Count == 0)
				return;
		}

		GameGlobals.Enemy.Clear();

		for (int i=0; i<enemyCards.Length; i++)
		{
			if (i >= def.List.Count)
				continue;

			var s = def.List[i];
			enemyCards[i].Fill(s.Id);

			if (s.Pos.x < 0) s.Pos.x = i+1;
			if (s.Pos.y < 0) s.Pos.y = 0;

			// mirror the pos
			s.Pos.y = Constants.GridRows - s.Pos.y - 1;
			s.Pos.x = Constants.GridCols - s.Pos.x - 1;

			GameGlobals.Enemy.Add(s);
		}
	}
}

}
