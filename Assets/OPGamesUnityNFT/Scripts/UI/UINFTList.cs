using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class UINFTList : MonoBehaviour
{
	[SerializeField] private GameObject loadingParent;
	[SerializeField] private Text loadingStatus;

	[SerializeField] private RectTransform contentParent;
	[SerializeField] private GameObject prefabNFT;

	[SerializeField] private Button btnNext;
	[SerializeField] private int numNFTs = 3;

	[SerializeField] private UINFTItem[] selectedUI;

	private Dictionary<string, GameObject> listItems = new Dictionary<string, GameObject>();
	private Dictionary<string, Toggle> listToggles = new Dictionary<string, Toggle>();

	public void OnBtnNext()
	{
		SceneManager.LoadScene(2);
	}

	private void Start()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft != null)
		{
			nft.OnQueryChainBegin += OnQueryChainBegin;
			nft.OnQueryChainEnd   += OnQueryChainEnd;
			nft.OnNFTItemFound    += OnNFTItemFound;
			nft.OnNFTItemLoaded   += OnNFTItemLoaded;
			nft.OnNFTListComplete += OnNFTListComplete;
		}

		LoadFromManager();

		foreach (var s in selectedUI)
		{
			s.gameObject.SetActive(false);
		}
	}

	private void LoadFromManager()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft == null)
			return;

		if (nft.IsNFTListComplete == false)
			return;

		var list = nft.LoadedNFTs;
		OnNFTListComplete(list);

		foreach (var n in list)
			OnNFTItemLoaded(n);
	}

	private void OnQueryChainBegin(string chain)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Query {0} begin", chain);
		}
	}

	private void OnQueryChainEnd(string chain)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Query {0} end", chain);
		}
	}

	private void OnNFTItemFound(int count)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Found {0} NFTs", count);
		}
	}

	private void OnNFTListComplete(List<NFTItemData> list)
	{
		foreach (var n in list)
		{
			GameObject clone = Instantiate(prefabNFT);
			clone.transform.SetParent(contentParent);
			clone.transform.localScale = UnityEngine.Vector3.one;

			listItems.Add(n.UniqueId, clone);

			var toggle = clone.GetComponentInChildren<Toggle>();
			if (toggle != null)
			{
				listToggles.Add(n.UniqueId, toggle);
				toggle.onValueChanged.AddListener(RefreshTogglesAndNext);
			}

			Debug.LogFormat("OnNFTListComplete - add to list {0}", n.UniqueId);
		}
	}

	private void OnNFTItemLoaded(NFTItemData n)
	{
		if (n == null)
			return;

		if (loadingParent != null)
			loadingParent.SetActive(false);

		if (listItems.ContainsKey(n.UniqueId) == false)
		{
			Debug.LogErrorFormat("UI list items does not have this key {0}", n.UniqueId);
			return;
		}

		GameObject clone = listItems[n.UniqueId];
		if (clone == null)
		{
			Debug.LogErrorFormat("UI list items gameobject is null {0}", n.UniqueId);
			return;
		}

		UINFTItem item = clone.GetComponent<UINFTItem>();
		if (item != null)
			item.Fill(n);

		RefreshTogglesAndNext();
	}

	private void RefreshTogglesAndNext(bool dontCare = false)
	{
		int count = 0;
		GameGlobals.Selected.Clear();
		foreach (var kvp in listToggles)
		{
			if (kvp.Value.isOn) 
			{
				count++;
				GameGlobals.Selected.Add(kvp.Key);
			}
		}

		if (count >= numNFTs)
		{
			foreach (var t in listToggles.Values)
			{
				if (t.isOn == false) 
				{
					t.interactable = false;
				}
			}
			btnNext.interactable = true;
		}
		else
		{
			foreach (var t in listToggles.Values)
			{
				t.interactable = true;
			}

			btnNext.interactable = false;
		}

		RefreshSelection();
	}

	private void RefreshSelection()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft == null) return;

		int selectedCount = 0;
		foreach (string key in GameGlobals.Selected)
		{
			if (selectedUI[selectedCount] != null)
			{
				NFTItemData d = nft.GetNFTItemDataById(key);
				selectedUI[selectedCount].Fill(d);
				selectedUI[selectedCount].gameObject.SetActive(true);
				selectedCount++;
			}
		}

		for (int i=selectedCount; i<selectedUI.Length; i++)
			selectedUI[i].gameObject.SetActive(false);
	}
}

}
