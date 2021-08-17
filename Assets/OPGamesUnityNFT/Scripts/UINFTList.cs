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

	private Dictionary<string, GameObject> listItems = new Dictionary<string, GameObject>();

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
		{
			if (string.IsNullOrEmpty(n.Name) == false)
			{
				item.Fill(n.Name, n.Description, n.ImageURL);
			}
			else
			{
				// can't load metadata, just show token id and contract address
				item.Fill(n.TokenId, n.Contract, "");
			}
		}
	}
}

}
