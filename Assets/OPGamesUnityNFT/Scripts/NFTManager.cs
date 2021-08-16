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

// Main class you have to interact with, to load and get NFT info
public class NFTManager : MonoBehaviour
{
	// Implemented as a singleton for easy access
	static private NFTManager instance = null;
	static public NFTManager Instance { get { return instance; } }

	// Temp wallet for testing. To be replaced with the metamask wallet address
	public string Wallet = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";

	// List of custom loaders for NFT metadata
	private List<INFTLoader> loaders = new List<INFTLoader>();

	// Holds the loaded NFT data
	private List<NFTItemData> loadedNFTs = new List<NFTItemData>();

	// Holds the NFT transactions from Etherscan and Polygon Explorer
	private List<Explorer721Events> eventsList = new List<Explorer721Events>();

	// Events: subscribe to these events to get progress of loading the data
	
	// Called when an NFT is found to belong to the Wallet
	public System.Action<int> OnNFTItemFound;

	// Called when we have filled up the metadata of the NFT (i.e. Name, Description, Image URL, etc)
	public System.Action<NFTItemData> OnNFTItemLoaded;
	
	// Called when we start querying Etherscan or Polygon Explorer
	public System.Action<string> OnQueryChainBegin;

	// Called when we are done querying Etherscan or Polygon Explorer
	public System.Action<string> OnQueryChainEnd;

	public System.Action<List<NFTItemData>> OnNFTListComplete;

	// Events: end

	// Make sure to have only one instance of this class.
	// Add the common NFT loaders
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

		// Add more loaders as needed. Client code can also add by calling AddLoader
		AddLoader(new NFTLoaderCommon()); // first should be common
		AddLoader(new NFTLoaderCK());
	}

	// Call this to add a custom loader
	public void AddLoader(INFTLoader loader)
	{
		if (loader == null)
		{
			return;
		}

		loaders.Add(loader);
	}

	// Maybe we can use a public function for this
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

		loadedNFTs.Sort((a,b) => 
		{ 
			int result = string.Compare(a.Chain, b.Chain);
			if (result != 0)
				return result;

			return string.Compare(a.Contract, b.Contract); 
		});


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

		if (OnNFTListComplete != null)
			OnNFTListComplete(loadedNFTs);

		LoadURIData();
    }

	// Query the blockchain explorer for 721 events
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

	// Go through the loaded events and find NFTs that the Wallet still owns
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

	private bool IsNFTInList(string contract, string tokenId)
	{
		var result = loadedNFTs.Find((n) => { return (n.Contract == contract && n.TokenId == tokenId); });
		return result != null;
	}

	// Check if a specific NFT is still owned by the Wallet
	private async Task LoadNFTURIItem(string chain, string network, Explorer721Events.Result r)
	{
		if (IsNFTInList(r.contractAddress, r.tokenID))
			return;

		string owner = await ERC721.OwnerOf(chain, network, r.contractAddress, r.tokenID);
		if (owner != Wallet)
		{
			r.isDoneProcessing = true;
			return;
		}

		string uri = "";
		var loader = GetLoader(r.contractAddress);
		if (loader.NeedToCallURI)
		{
			uri = await ERC721.URI(chain, network, r.contractAddress, r.tokenID);
			Debug.Log(uri);
		}

		NFTItemData item = new NFTItemData();
		item.Chain = chain;
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

	// Returns a INFTLoader that matches the contract address
	private INFTLoader GetLoader(string contract)
	{
		foreach (var l in loaders)
			if (l.Contract == contract)
				return l;

		// default to common
		return loaders[0];
	}

	// Loads the metadata for the NFT
	private void LoadURIData()
	{
		foreach (var n in loadedNFTs)
		{
			var loader = GetLoader(n.Contract);
			if (loader != null)
			{
				StartCoroutine(loader.LoadNFTData(n, (nOut) => { if (OnNFTItemLoaded != null) OnNFTItemLoaded(nOut); }));
			}
		}
	}
}

}
