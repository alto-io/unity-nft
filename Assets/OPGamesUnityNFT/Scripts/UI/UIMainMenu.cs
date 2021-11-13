using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class UIMainMenu : MonoBehaviour
{
	public void OnBtnPlay()
	{
		UIManager.Open(UIType.EditSquad);
	}

	public void OnBtnEditSquad()
	{
		UIManager.Open(UIType.EditSquad);
	}

	public void OnBtnLeaderboard()
	{
	}

	public void OnBtnHistory()
	{
	}

	public void OnBtnSettings()
	{
	}
}

}
