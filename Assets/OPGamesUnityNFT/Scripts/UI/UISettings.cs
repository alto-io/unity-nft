using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace OPGames.NFT
{

public class UISettings : MonoBehaviour
{
	[SerializeField] TMP_InputField nameField;

	private PlayFabManager pf;

	private void OnEnable()
	{
		pf = PlayFabManager.Instance;

		if (pf != null && pf.IsLoggedIn)
			nameField.text = pf.DisplayName;
	}

	public void OnClose()
	{
		if (nameField.text != pf.DisplayName)
		{
			pf.SetDisplayName(nameField.text);
		}

		gameObject.SetActive(false);
	}

	public void OnFAQ()
	{

	}

	public void OnCredits()
	{

	}

	public void OnMusic(bool isOn)
	{

	}

	public void OnSFX(bool isOn)
	{

	}
}

}
