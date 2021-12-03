using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace OPGames.NFT
{

public class UILeaderboard : MonoBehaviour
{
	[SerializeField] private GameObject itemPrefab;
	[SerializeField] private Transform content;

	private List<UILeaderboardItem> used = new List<UILeaderboardItem>();
	private List<UILeaderboardItem> free = new List<UILeaderboardItem>();

	public void OnEnable()
	{
		PlayFabManager pf = PlayFabManager.Instance;
		if (pf != null)
			pf.RequestLeaderboard(Populate);
	}

	public void OnClose()
	{
		gameObject.SetActive(false);

		foreach (var i in used)
		{
			free.Add(i);
			i.gameObject.SetActive(false);
		}

		used.Clear();
	}

	private void Populate()
	{
		PlayFabManager pf = PlayFabManager.Instance;
		if (pf == null) return;

		Debug.LogFormat("Leadboard");
		foreach (var p in pf.Leaderboard)
		{
			if (free.Count > 0)
			{
				var i = free[free.Count-1];
				free.RemoveAt(free.Count-1);
				i.Fill(p.Position, p.DisplayName, p.StatValue);
				i.gameObject.SetActive(true);
				i.transform.SetAsLastSibling();
				used.Add(i);
			}
			else
			{
				GameObject clone = Instantiate(itemPrefab, content);
				var i = clone.GetComponent<UILeaderboardItem>();
				i.Fill(p.Position, p.DisplayName, p.StatValue);
				i.gameObject.SetActive(true);
				used.Add(i);
			}
		}
	}
}

}
