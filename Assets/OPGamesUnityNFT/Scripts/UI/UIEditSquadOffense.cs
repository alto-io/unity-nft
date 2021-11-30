using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

namespace OPGames.NFT
{

public class UIEditSquadOffense : MonoBehaviour
{
	private float timeLeft = Constants.OffenseSetupTime;

	[SerializeField] private UIEditSquadGrid grid;
	[SerializeField] private TextMeshProUGUI textTime;

	private void OnEnable()
	{
		timeLeft = Constants.OffenseSetupTime;
	}

	private void Update()
	{
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f)
			OnBtnConfirm();

		int displayTime = (int)Mathf.Clamp(timeLeft, 0.0f, Constants.OffenseSetupTime);
		textTime.text = displayTime.ToString();
	}

	public void OnBtnConfirm()
	{
		grid.AssignFinalPositions();
		if (GameGlobals.Defense.Count == 0)
		{
			GameGlobals.CopyList(GameGlobals.Defense, GameGlobals.Offense);
		}
		SaveManager.Instance.Save();
		SceneManager.LoadScene(2);
	}
}

}
