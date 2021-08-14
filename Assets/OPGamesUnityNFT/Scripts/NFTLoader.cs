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

public class NFTLoader : MonoBehaviour
{
	static private NFTLoader instance = null;
	static public NFTLoader Instance { get { return instance; } }

	const string contractCK = "0x06012c8cf97bead5deae237070f9587f8e7a266d";

	public RectTransform Content;
	public GameObject NFTPrefab;
	public string Wallet = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";

	private List<NFTItemData> loadedNFTs = new List<NFTItemData>();

	private List<Explorer721Events> eventsList = new List<Explorer721Events>();

	private bool loadNFTURIDone = false;
	private int loadingNFTURICount = 0;

	public System.Action<int> OnNFTItemFound;
	public System.Action<NFTItemData> OnNFTItemLoaded;
	public System.Action<string> OnQueryChainBegin;
	public System.Action<string> OnQueryChainEnd;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(this);
		}
		else
		{
			instance = this;
		}
	}

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
			yield return StartCoroutine(QueryChain(c));
		}

		LoadNFTURI();

		bool isDoneProcessing = false;
		while (isDoneProcessing == false)
		{
			yield return new WaitForSeconds(0.5f);

			isDoneProcessing = true;
			foreach (var e in eventsList)
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

	private IEnumerator QueryChain(ChainData chain)
	{
		if (chain == null)
		{
			Debug.LogError("LoadNFT:QueryChain - chain is null");
			yield break;
		}

		if (OnQueryChainBegin != null)
		{
			OnQueryChainBegin(chain.Chain);
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
			eventsList.Add(events);
		}

		if (OnQueryChainEnd != null)
		{
			OnQueryChainEnd(chain.Chain);
		}
	}

	private void LoadNFTURI()
	{
		foreach (var events in eventsList)
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

		loadedNFTs.Add(item);
		r.isDoneProcessing = true;

		if (OnNFTItemFound != null)
		{
			OnNFTItemFound(loadedNFTs.Count);
		}
	}

	private void LoadURIData()
	{
		Debug.Log("LoadURIData");
		foreach (var n in loadedNFTs)
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
			Debug.LogFormat("LoadNFT:LoadNFTDataCK - Received: {0}", json);

			URIData data = JsonUtility.FromJson<URIData>(json);
			n.Name = data.name;
			n.Description = data.bio;
			n.ImageURL = data.image_url_png;

			if (OnNFTItemLoaded != null)
				OnNFTItemLoaded(n);
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
			if (data != null)
			{
				n.Name = data.name;
				n.Description = data.description;
				n.ImageURL = data.image;

				if (OnNFTItemLoaded != null)
					OnNFTItemLoaded(n);
			}
		}
	}
}

}
