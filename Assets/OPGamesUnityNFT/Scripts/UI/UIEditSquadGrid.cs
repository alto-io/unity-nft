using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIEditSquadGrid : MonoBehaviour
{
	public void OnBtnConfirm()
	{
		SceneManager.LoadScene(2);
	}
}

}
