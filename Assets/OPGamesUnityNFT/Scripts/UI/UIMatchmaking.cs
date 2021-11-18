using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIMatchmaking : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(WaitForMatch());
	}

	private IEnumerator WaitForMatch()
	{
		yield return new WaitForSeconds(2.0f);
		UIManager.Open(UIType.EditSquadGrid);
		//SceneManager.LoadScene(2);
	}

	public void OnBtnBack()
	{
		UIManager.Close();
	}
}

}
