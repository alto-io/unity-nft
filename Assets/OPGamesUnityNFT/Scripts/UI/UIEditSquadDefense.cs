using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIEditSquadDefense : MonoBehaviour
{
	[SerializeField] private GameObject goSelect;
	[SerializeField] private GameObject goGrid;

	private void Start()
	{
		OnBtnEditSquad(true);
	}

	public void OnBtnBack()
	{
		UIManager.Close();
	}

	public void OnBtnSave()
	{
		var grid = goGrid.GetComponentInChildren<UIEditSquadGrid>();
		if (grid != null)
			grid.AssignFinalPositions();

		if (GameGlobals.Offense.Count == 0)
		{
			GameGlobals.CopyList(GameGlobals.Offense, GameGlobals.Defense);
		}

		SaveManager.Instance.Save();
		UIManager.Close();
	}

	public void OnBtnEditSquad(bool isActive)
	{
		goSelect.SetActive(isActive);
		goGrid.SetActive(!isActive);
	}
} 
}
