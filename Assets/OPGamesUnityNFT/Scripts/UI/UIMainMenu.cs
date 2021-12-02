using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class UIMainMenu : MonoBehaviour
{
	[SerializeField] private GameObject uiSettings;
	[SerializeField] private GameObject uiLeaderboard;
	[SerializeField] private GameObject uiHistory;

	private void Start()
	{
		if (uiSettings != null) uiSettings.SetActive(false);
		if (uiLeaderboard != null) uiLeaderboard.SetActive(false);
		if (uiHistory != null) uiHistory.SetActive(false);
	}

	public void OnBtnPlay()
	{
		UIManager.Open(UIType.NFTList);
	}

	public void OnBtnEditSquad()
	{
		UIManager.Open(UIType.EditSquadDef);
	}

	public void OnBtnLeaderboard()
	{
		if (uiLeaderboard != null)
			uiLeaderboard.SetActive(true);
	}

	public void OnBtnHistory()
	{
		if (uiHistory != null)
			uiHistory.SetActive(true);
	}

	public void OnBtnSettings()
	{
		if (uiSettings != null)
			uiSettings.SetActive(true);
	}
}

}
