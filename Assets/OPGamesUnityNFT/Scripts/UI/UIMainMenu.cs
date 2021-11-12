using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMainMenu : MonoBehaviour
{
	public void OnBtnPlay()
	{
		UIManager.Show(UIType.EditSquad);
	}

	public void OnBtnEditSquad()
	{
		UIManager.Show(UIType.EditSquad);
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
