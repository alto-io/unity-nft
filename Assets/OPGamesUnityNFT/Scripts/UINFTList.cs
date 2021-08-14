using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OPGames.NFT
{

public class UINFTList : MonoBehaviour
{
	[SerializeField] private GameObject loadingParent;
	[SerializeField] private Text loadingStatus;

	[SerializeField] private RectTransform contentParent;
	[SerializeField] private GameObject prefabNFT;

	private void Start()
	{
		NFTLoader nft = NFTLoader.Instance;
		if (nft != null)
		{
			nft.OnQueryChainBegin += OnQueryChainBegin;
			nft.OnQueryChainEnd += OnQueryChainEnd;
			nft.OnNFTItemFound += OnNFTItemFound;
			nft.OnNFTItemLoaded += OnNFTItemLoaded;
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

	private void OnNFTItemLoaded(NFTItemData n)
	{
		if (loadingParent != null)
		{
			loadingParent.SetActive(false);
		}

		GameObject clone = Instantiate(prefabNFT);
		clone.transform.SetParent(contentParent);
		clone.transform.localScale = UnityEngine.Vector3.one;

		UINFTItem item = clone.GetComponent<UINFTItem>();
		if (item != null)
		{
			item.Fill(n.Name, n.Description, n.ImageURL);
		}
	}
}

}
