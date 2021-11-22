using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UINFTList : MonoBehaviour
{
	public void OnBtnNext()
	{
		UIManager.Open(UIType.Matchmaking);
	}
	
	public void OnBtnBack()
	{
		UIManager.Close();
	}
}

}
