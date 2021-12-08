using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace OPGames.NFT
{

public class UIEnterName : MonoBehaviour
{
	[SerializeField] TMP_InputField nameField;

	private PlayFabManager pf;

	private void OnEnable()
	{
		pf = PlayFabManager.Instance;

		if (pf != null && pf.IsLoggedIn)
			nameField.text = pf.DisplayName;
	}

	public void OnBtnOK()
	{
		if (nameField.text != pf.DisplayName)
		{
			pf.SetDisplayName(nameField.text);
		}

		UIManager.Close();
	}
}

}
