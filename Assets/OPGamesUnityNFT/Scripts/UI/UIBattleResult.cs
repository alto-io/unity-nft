using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace OPGames.NFT
{

public class UIBattleResult : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI playerName;
	[SerializeField] private TextMeshProUGUI playerBP;
	[SerializeField] private TextMeshProUGUI playerDelta;
	[SerializeField] private TextMeshProUGUI enemyName;
	[SerializeField] private TextMeshProUGUI enemyBP;
	[SerializeField] private TextMeshProUGUI enemyDelta;

	[SerializeField] private UINFTItem[] playerCards;
	[SerializeField] private UINFTItem[] enemyCards;

	[SerializeField] private GameObject goWin;
	[SerializeField] private GameObject goLose;

	private bool isPlayerWin = true;

	private void Start()
	{
		FillPlayer();
		FillEnemy();
	}

	public void SetBattleResult(bool _isPlayerWin)
	{
		isPlayerWin = _isPlayerWin;

		if (goWin != null) goWin.SetActive(isPlayerWin);
		if (goLose != null) goLose.SetActive(!isPlayerWin);

		if (isPlayerWin)
		{
			playerDelta.text = "+50";
			enemyDelta.text = "-20";
		}
		else
		{
			playerDelta.text = "-35";
			enemyDelta.text = "+35";
		}
	}

	public void OnContinue()
	{
		Time.timeScale = 1.0f;
		SceneManager.LoadScene(1);
	}

	private void FillPlayer()
	{
		var pf = PlayFabManager.Instance;

		playerName.text = pf.DisplayName;
		playerBP.text = pf.MMR.ToString();

		for (int i=0; i<playerCards.Length; i++)
		{
			if (i >= GameGlobals.Offense.Count)
				break;

			var s = GameGlobals.Offense[i];
			playerCards[i].Fill(s.Id);
		}
	}

	private void FillEnemy()
	{
		var pf = PlayFabManager.Instance;

		var enemyModel = GameGlobals.EnemyModel;
		if (enemyModel != null)
		{
			enemyName.text = enemyModel.DisplayName;
			enemyBP.text = enemyModel.MMR.ToString();
		}

		for (int i=0; i<enemyCards.Length; i++)
		{
			if (i >= GameGlobals.Enemy.Count)
				break;

			var s = GameGlobals.Enemy[i];
			enemyCards[i].Fill(s.Id);
		}
	}
}

}
