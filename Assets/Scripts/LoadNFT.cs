using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace OPGames.NFT
{

public class LoadNFT : MonoBehaviour
{
	const string contractCK = "0x06012c8cf97bead5deae237070f9587f8e7a266d";

	public RectTransform Content;
	public GameObject NFTPrefab;
	public string Wallet = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";

	public List<NFTItemData> LoadedNFTs = new List<NFTItemData>();

	public List<Explorer721Events> EventsList = new List<Explorer721Events>();

	private bool loadNFTURIDone = false;
	private int loadingNFTURICount = 0;

    private IEnumerator Start()
    {
		if (_Config.Account != "0x0000000000000000000000000000000000000001")
			Wallet = _Config.Account;

		ChainData[] chains = Resources.LoadAll<ChainData>("");
		foreach (var c in chains)
		{
			if (c.Enabled == false)
				continue;
			
			Debug.LogFormat("Found ChainData: {0}", c.Chain);
			yield return StartCoroutine(LoadChain(c));
		}

		LoadNFTURI();

		bool isDoneProcessing = false;
		while (isDoneProcessing == false)
		{
			yield return new WaitForSeconds(0.5f);

			isDoneProcessing = true;
			foreach (var e in EventsList)
			{
				if (e.IsDoneProcessing() == false)
				{
					isDoneProcessing = false;
					break;
				}
			}
		}

		LoadURIData();
    }

	private IEnumerator LoadChain(ChainData chain)
	{
		if (chain == null)
		{
			Debug.LogError("LoadNFT:LoadChain - chain is null");
			yield break;
		}

		string apiCall = string.Format(chain.Explorer721EventsCall, Wallet, chain.ExplorerAPIKey);

		Debug.Log(apiCall);
		using (var www = UnityWebRequest.Get(apiCall))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;
			Debug.Log(json);

			Explorer721Events events = JsonUtility.FromJson<Explorer721Events>(json);
			events.chain = chain.Chain;
			events.network = chain.Network;
			events.blacklistContracts = chain.BlacklistContracts;
			EventsList.Add(events);
		}
	}

	private void LoadNFTURI()
	{
		foreach (var events in EventsList)
		{
			foreach (var r in events.result)
			{
				if (events.blacklistContracts.IndexOf(r.contractAddress) >= 0)
				{
					r.isDoneProcessing = true;
					continue;
				}
				LoadNFTURIItem(events.chain, events.network, r);
			}
		}
	}

	private async Task LoadNFTURIItem(string chain, string network, Explorer721Events.Result r)
	{
		string owner = await ERC721.OwnerOf(chain, network, r.contractAddress, r.tokenID);
		if (owner != Wallet)
		{
			r.isDoneProcessing = true;
			return;
		}

		string uri = "";
		if (r.contractAddress != contractCK)
		{
			uri = await ERC721.URI(chain, network, r.contractAddress, r.tokenID);
			Debug.Log(uri);
		}

		NFTItemData item = new NFTItemData();
		item.TokenId = r.tokenID;
		item.URI = uri;
		item.Contract = r.contractAddress;

		LoadedNFTs.Add(item);
		r.isDoneProcessing = true;
	}

	private void LoadURIData()
	{
		Debug.Log("LoadURIData");
		foreach (var n in LoadedNFTs)
		{
			if (n.Contract == contractCK)
			{
				StartCoroutine(LoadNFTDataCK(n));
			}
			else
			{
				StartCoroutine(LoadNFTDataCommon(n));
			}
		}
	}

	private IEnumerator LoadNFTDataCK(NFTItemData n)
	{
		string apiUrl = "https://api.cryptokitties.co/kitties/" + n.TokenId;
		using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
		{
			yield return request.SendWebRequest();
			string json = request.downloadHandler.text;
			Debug.LogFormat("Received: {0}", json);

			URIData data = JsonUtility.FromJson<URIData>(json);
			n.Metadata = data;

			if (data != null)
			{
				GameObject clone = Instantiate(NFTPrefab);
				clone.transform.SetParent(Content);
				clone.transform.localScale = UnityEngine.Vector3.one;

				UINFTItem item = clone.GetComponent<UINFTItem>();
				if (item != null)
				{
					item.Fill(data.name, data.bio, data.image_url_png);
				}
			}
		}
	}

	private IEnumerator LoadNFTDataCommon(NFTItemData n)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(n.URI))
		{
			yield return request.SendWebRequest();
			string json = request.downloadHandler.text;
			Debug.LogFormat("LoadNFT:LoadNFTDataCommon - Received: {0}", json);

			URIData data = JsonUtility.FromJson<URIData>(json);
			n.Metadata = data;

			if (data != null)
			{
				Debug.LogFormat("LoadNFT:LoadNFTDataCommon - Image URL: {0}", data.image);

				GameObject clone = Instantiate(NFTPrefab);
				clone.transform.SetParent(Content);
				clone.transform.localScale = UnityEngine.Vector3.one;

				UINFTItem item = clone.GetComponent<UINFTItem>();
				if (item != null)
				{
					item.Fill(data.name, data.description, data.image);
				}
			}
		}
	}
}

}
