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
	public TextMeshProUGUI playerName;
	public TextMeshProUGUI enemyName;

	public UINFTItem[] playerCards;
	public UINFTItem[] enemyCards;

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
			(error) => errorMsg = error);

		while (model == null && errorMsg == null)
			yield return new WaitForSeconds(0.1f);

		if (model != null)
		{
			GameGlobals.EnemyModel = model;
			FillEnemy(model);
			yield return new WaitForSeconds(1.0f);
		}
		else
		{
			// TODO: load random
		}

		UIManager.Open(UIType.EditSquadOff);
		//SceneManager.LoadScene(2);
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

			playerCards[i].Fill(nft.GetNFTItemDataById(s.Id));
		}
	}

	private void FillEnemy(PVPPlayerModel model)
	{
		// TODO if using real nft, need to download images
		var nft = NFTManager.Instance;

		enemyName.text = model.DisplayName;

		var def = JsonUtility.FromJson<SaveDataSelectedList>(model.Defense);
		if (def == null || def.List == null)
			return;

		GameGlobals.Enemy.Clear();

		for (int i=0; i<enemyCards.Length; i++)
		{
			if (i >= def.List.Count)
				continue;

			var s = def.List[i];
			enemyCards[i].Fill(nft.GetNFTItemDataById(s.Id));

			// mirror the pos
			s.Pos.y = Constants.GridRows - s.Pos.y - 1;
			s.Pos.x = Constants.GridCols - s.Pos.x - 1;

			GameGlobals.Enemy.Add(s);
		}
	}
}

}
