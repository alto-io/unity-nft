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

	public bool IsNFTListComplete { get; private set; }

	public List<NFTItemData> LoadedNFTs { get { return loadedNFTs; } }

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
	
	public NFTItemData GetNFTItemDataById(string uniqueId)
	{
		return loadedNFTs.Find((n) => (n.UniqueId == uniqueId));
	}

	// Make sure to have only one instance of this class.
	// Add the common NFT loaders
	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
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

    private IEnumerator Start()
	{
		if (_Config.Account != "0x0000000000000000000000000000000000000001")
			Wallet = _Config.Account;

		const string osAssetsAPI = "https://api.opensea.io/api/v1/assets";
		string osAssetsRequest = osAssetsAPI + "?owner=" + Wallet;
		
		using (var www = UnityWebRequest.Get(osAssetsRequest))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;

			OpenSeaAssets result = JsonUtility.FromJson<OpenSeaAssets>(json);
			if (result != null)
			{
				foreach (var a in result.assets)
				{
					Debug.LogFormat("{0}, {1}, {2}, {3}",
							a.token_id,
							a.name,
							a.description,
							a.image_preview_url);

					NFTItemData item = new NFTItemData();
					item.TokenId = a.token_id;
					item.Contract = a.asset_contract.address;
					item.Name = a.name;
					item.Description = a.description;
					item.ImageURL = a.image_preview_url;

					loadedNFTs.Add(item);

				}
			}
		}

		LoadTempNFT1();

		if (OnNFTListComplete != null)
			OnNFTListComplete(loadedNFTs);

		IsNFTListComplete = true;

		foreach (var n in loadedNFTs)
		{
			if (OnNFTItemLoaded != null)
				OnNFTItemLoaded(n);
		}
	}

//	// Maybe we can use a public function for this
//    private IEnumerator StartOld()
//    {
//		if (_Config.Account != "0x0000000000000000000000000000000000000001")
//			Wallet = _Config.Account;
//
//		DataChain[] chains = Resources.LoadAll<DataChain>("");
//		foreach (var c in chains)
//		{
//			if (c.Enabled == false)
//				continue;
//			
//			Debug.LogFormat("Found DataChain: {0}", c.Chain);
//			yield return StartCoroutine(QueryChain(c));
//		}
//
//		DataNFTFake[] fakes = Resources.LoadAll<DataNFTFake>("");
//		foreach (var f in fakes)
//		{
//			if (f.Enabled == false)
//				continue;
//			
//			yield return StartCoroutine(QueryChainFake(f));
//		}
//
//		LoadNFTURI();
//
//		bool isDoneProcessing = false;
//		while (isDoneProcessing == false)
//		{
//			yield return new WaitForSeconds(0.5f);
//
//			isDoneProcessing = true;
//			foreach (var e in eventsList)
//			{
//				if (e.IsDoneProcessing() == false)
//				{
//					isDoneProcessing = false;
//					break;
//				}
//			}
//		}
//
//		loadedNFTs.Sort((a,b) => 
//		{ 
//			int result = string.Compare(a.Chain, b.Chain);
//			if (result != 0)
//				return result;
//
//			return string.Compare(a.Contract, b.Contract); 
//		});
//
//		LoadTempNFT1();
//
//		if (OnNFTListComplete != null)
//			OnNFTListComplete(loadedNFTs);
//
//		IsNFTListComplete = true;
//
//		StartCoroutine(LoadURIData());
//		LoadTempNFT2();
//    }

//	// Query the blockchain explorer for 721 events
//	private IEnumerator QueryChain(DataChain chain)
//	{
//		if (chain == null)
//		{
//			Debug.LogError("LoadNFT:QueryChain - chain is null");
//			yield break;
//		}
//
//		if (OnQueryChainBegin != null)
//		{
//			OnQueryChainBegin(chain.Chain);
//		}
//
//		string apiCall = string.Format(chain.Explorer721EventsCall, Wallet, chain.ExplorerAPIKey);
//
//		Debug.Log(apiCall);
//		using (var www = UnityWebRequest.Get(apiCall))
//		{
//			yield return www.SendWebRequest();
//			var json = www.downloadHandler.text;
//			Debug.Log(json);
//
//			Explorer721Events events = JsonUtility.FromJson<Explorer721Events>(json);
//			events.chain = chain.Chain;
//			events.network = chain.Network;
//			events.blacklistContracts = chain.BlacklistContracts;
//			eventsList.Add(events);
//		}
//
//		if (OnQueryChainEnd != null)
//		{
//			OnQueryChainEnd(chain.Chain);
//		}
//	}
//
//	private IEnumerator QueryChainFake(DataNFTFake fake)
//	{
//		if (fake == null)
//			yield break;
//
//		foreach (var events in eventsList)
//		{
//			if (events.chain != fake.Chain || events.network != fake.Network)
//				continue;
//
//			foreach (var token in fake.TokenIds)
//			{
//				events.result.Add(new Explorer721Events.Result() 
//					{
//						contractAddress = fake.ContractAddr,
//						tokenID = token,
//						isFake = true
//					});
//			}
//		}
//
//		yield return null;
//				
//	}
//
//	// Go through the loaded events and find NFTs that the Wallet still owns
//	private void LoadNFTURI()
//	{
//		foreach (var events in eventsList)
//		{
//			foreach (var r in events.result)
//			{
//				if (events.blacklistContracts.IndexOf(r.contractAddress) >= 0)
//				{
//					r.isDoneProcessing = true;
//					continue;
//				}
//				LoadNFTURIItem(events.chain, events.network, r);
//			}
//		}
//	}
//
//	private bool IsNFTInList(string contract, string tokenId)
//	{
//		var result = loadedNFTs.Find((n) => { return (n.Contract == contract && n.TokenId == tokenId); });
//		return result != null;
//	}
//
//	// Check if a specific NFT is still owned by the Wallet
//	private async Task LoadNFTURIItem(string chain, string network, Explorer721Events.Result r)
//	{
//		if (IsNFTInList(r.contractAddress, r.tokenID))
//			return;
//
//		if (r.isFake == false)
//		{
//			string owner = await ERC721.OwnerOf(chain, network, r.contractAddress, r.tokenID);
//			if (owner != Wallet)
//			{
//				r.isDoneProcessing = true;
//				return;
//			}
//		}
//
//		string uri = "";
//		var loader = GetLoader(r.contractAddress);
//		if (loader.NeedToCallURI)
//		{
//			uri = await ERC721.URI(chain, network, r.contractAddress, r.tokenID);
//			Debug.Log(uri);
//		}
//
//		NFTItemData item = new NFTItemData();
//		item.Chain = chain;
//		item.TokenId = r.tokenID;
//		item.URI = uri;
//		item.Contract = r.contractAddress;
//
//		loadedNFTs.Add(item);
//		r.isDoneProcessing = true;
//
//		if (OnNFTItemFound != null)
//		{
//			OnNFTItemFound(loadedNFTs.Count);
//		}
//	}
//
//	// Returns a INFTLoader that matches the contract address
//	private INFTLoader GetLoader(string contract)
//	{
//		foreach (var l in loaders)
//			if (l.Contract == contract)
//				return l;
//
//		// default to common
//		return loaders[0];
//	}
//
//	// Loads the metadata for the NFT
//	private IEnumerator LoadURIData()
//	{
//		foreach (var n in loadedNFTs)
//		{
//			var loader = GetLoader(n.Contract);
//			if (loader != null)
//			{
//				yield return StartCoroutine(loader.LoadNFTData(n, (nOut) => { if (OnNFTItemLoaded != null) OnNFTItemLoaded(nOut); }));
//			}
//		}
//	}

	private void LoadTempNFT1()
	{
		DataNFTTemp nft = DataNFTTemp.Instance;
		if (nft == null)
			return;

		if (nft.enabled == false)
			return;

		int len = nft.InfoList.Length;
		for (int i=0; i<len; i++)
		{
			var n = nft.InfoList[i];

			NFTItemData item = new NFTItemData();
			item.Chain = "localhost";
			item.TokenId = i.ToString();
			item.URI = "temp";
			item.Contract = "0x0000";
			item.Name = n.Name;
			item.Description = n.Description;
			item.Texture = n.Texture;
			item.Spr = n.Spr;
			item.CharClass = n.CharClass;

			loadedNFTs.Add(item);
		}
	}

	private void LoadTempNFT2()
	{
		DataNFTTemp nft = DataNFTTemp.Instance;
		if (nft == null)
			return;

		if (nft.enabled == false)
			return;

		int len = nft.InfoList.Length;
		for (int i=0; i<len; i++)
		{
			var tokenId = i.ToString();
			var n = loadedNFTs.Find((n) => (n.Chain == "localhost" && n.TokenId == tokenId));

			if (n != null && OnNFTItemLoaded != null)
			{
				OnNFTItemLoaded(n);
			}
		}
	}
}

}
