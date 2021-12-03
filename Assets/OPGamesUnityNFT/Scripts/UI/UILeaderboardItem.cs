using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace OPGames.NFT
{

public class UILeaderboardItem : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI textDisplayName;
	[SerializeField] private TextMeshProUGUI textPosition;
	[SerializeField] private TextMeshProUGUI textStatValue;

	public void Fill(int position, string displayName, int statValue)
	{
		position++;
		textPosition.text = position.ToString();
		textDisplayName.text = displayName;
		textStatValue.text = statValue.ToString();
	}
}

}
