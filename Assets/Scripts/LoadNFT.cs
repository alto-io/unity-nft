using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class URIData
{
	public string name;
	public string bio;
	public string description;
	public string image;
	public string image_url_png;
}


[System.Serializable]
public class NFTItemData
{
	public NFTInfo Info;
	public string TokenId;
	public string URI;
	public string ImageURI;
	public string Contract;
	public URIData Metadata;
}

public class LoadNFT : MonoBehaviour
{
	const string contractCK = "0x06012c8cf97bead5deae237070f9587f8e7a266d";

	public RectTransform Content;
	public GameObject NFTPrefab;
    public Image img;
	public string Wallet = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";

	public List<NFTItemData> LoadedNFTs = new List<NFTItemData>();

	public List<PolygonExplorer721Events> EventsList = new List<PolygonExplorer721Events>();

	private bool loadNFTURIDone = false;

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
			yield return StartCoroutine(LoadChainTemp(c));
		}

		LoadNFTURI();

		while (loadNFTURIDone == false)
			yield return null;

		yield return StartCoroutine(LoadURIData());
    }

	private IEnumerator LoadChainTemp(ChainData chain)
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

			PolygonExplorer721Events events = JsonUtility.FromJson<PolygonExplorer721Events>(json);
			events.chain = chain.Chain;
			events.network = chain.Network;
			events.blacklistContracts = chain.BlacklistContracts;
			EventsList.Add(events);
		}
	}

	private async Task LoadNFTURI()
	{
		loadNFTURIDone = false;

		foreach (var events in EventsList)
		{
			foreach (var r in events.result)
			{
				if (events.blacklistContracts.IndexOf(r.contractAddress) >= 0)
					continue;

				string owner = await ERC721.OwnerOf(events.chain, events.network, r.contractAddress, r.tokenID);
				if (owner != Wallet)
					continue;

				string uri = "";
				if (r.contractAddress != contractCK)
				{
					uri = await ERC721.URI(events.chain, events.network, r.contractAddress, r.tokenID);
					Debug.Log(uri);
				}

				NFTItemData item = new NFTItemData();
				//item.Info = info;
				item.TokenId = r.tokenID;
				item.URI = uri;
				item.Contract = r.contractAddress;

				LoadedNFTs.Add(item);
			}
		}

		loadNFTURIDone = true;
	}

	private async Task LoadChain(ChainData chain)
	{
		if (chain == null)
		{
			Debug.LogError("LoadNFT:LoadChain - chain is null");
			return;
		}

		string apiCall = string.Format(chain.Explorer721EventsCall, Wallet, chain.ExplorerAPIKey);

		Debug.Log(apiCall);
		using (var www = UnityWebRequest.Get(apiCall))
		{
			var operation = www.SendWebRequest();
			while (!operation.isDone)
				await Task.Delay(100);

			var json = www.downloadHandler.text;
			Debug.Log(json);

			PolygonExplorer721Events events = JsonUtility.FromJson<PolygonExplorer721Events>(json);
			if (events == null)
				return;

		}
	}

	private async Task LoadNFT721(string chain, string network, NFTInfo info)
	{
		Debug.LogFormat("LoadNFT:LoadNFT721 - {0} Loading", info.Name);
        BigInteger balance = await ERC721.BalanceOf(chain, network, info.Contract, Wallet);
		if (balance <= 0)
		{
			Debug.LogFormat("LoadNFT:LoadNFT721 - {0} 0 balance", info.Name);
			return;
		}

		for (int i=0; i<balance; i++)
		{
			string tokenId = await ERC721.TokenOfOwnerByIndex(chain, network, info.Contract, Wallet, i);
			tokenId = "45042175272649864";
			Debug.LogFormat("LoadNFT:LoadNFT721 - {0} loading index {1} - result {2}", info.Name, i, tokenId);

            string uri = await ERC721.URI(chain, network, info.Contract, tokenId);
            Debug.Log(uri);

			NFTItemData item = new NFTItemData();
			item.Info = info;
			item.TokenId = tokenId;
			item.URI = uri;

			LoadedNFTs.Add(item);
		}
	}

	private async Task LoadNFT1155(string chain, string network, NFTInfo info)
	{
		Debug.LogFormat("LoadNFT:LoadNFT1155 - {0} Loading", info.Name);
        BigInteger balance = await ERC1155.BalanceOf(chain, network, info.Contract, Wallet, "57");
		if (balance <= 0)
		{
			Debug.LogFormat("LoadNFT:LoadNFT1155 - {0} 0 balance", info.Name);
			return;
		}

//		for (int i=0; i<balance; i++)
//		{
//			string tokenId = await ERC721.TokenOfOwnerByIndex(chain, network, info.Contract, Wallet, i);
//			Debug.LogFormat("LoadNFT:LoadNFT1155 - {0} loading index {1} - result {2}", info.Name, i, tokenId);
//
//            string uri = await ERC721.URI(chain, network, info.Contract, tokenId);
//            Debug.Log(uri);
//
//			NFTItemData item = new NFTItemData();
//			item.Info = info;
//			item.TokenId = tokenId;
//			item.URI = uri;
//
//			LoadedNFTs.Add(item);
//		}

	}


	private IEnumerator LoadURIData()
	{
		Debug.Log("LoadURIData");
		foreach (var n in LoadedNFTs)
		{
			if (n.Contract == contractCK)
			{
				yield return StartCoroutine(LoadNFTDataCK(n));
			}
			else
			{
				yield return StartCoroutine(LoadNFTDataCommon(n));
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

				NFTItem item = clone.GetComponent<NFTItem>();
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
			Debug.LogFormat("Received: {0}", json);

			URIData data = JsonUtility.FromJson<URIData>(json);
			n.Metadata = data;

			if (data != null)
			{
				Debug.LogFormat("Image URL: {0}", data.image);

				GameObject clone = Instantiate(NFTPrefab);
				clone.transform.SetParent(Content);
				clone.transform.localScale = UnityEngine.Vector3.one;

				NFTItem item = clone.GetComponent<NFTItem>();
				if (item != null)
				{
					item.Fill(data.name, data.description, data.image);
				}
			}
		}
	}
}
