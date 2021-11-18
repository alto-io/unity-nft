using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIEditSquadOffense : MonoBehaviour
{
	[SerializeField]
	private UIEditSquadGrid grid;

	public void OnBtnConfirm()
	{
		grid.AssignFinalPositions();
		SceneManager.LoadScene(2);
	}
}

}
