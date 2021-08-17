using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFight : MonoBehaviour
{
	public void OnBtnBack()
	{
		SceneManager.LoadScene(1);
	}
}
