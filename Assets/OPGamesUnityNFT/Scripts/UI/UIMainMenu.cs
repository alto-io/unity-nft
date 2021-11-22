using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class UIMainMenu : MonoBehaviour
{
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
	}

	public void OnBtnHistory()
	{
	}

	public void OnBtnSettings()
	{
	}
}

}
