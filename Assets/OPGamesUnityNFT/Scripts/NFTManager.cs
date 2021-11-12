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

	[Tooltip("Fill with test address")]
	public string TestWallet = "";

	public List<string> NFTWhiteList = new List<string>();

	// Temp wallet for testing. To be replaced with the metamask wallet address
	private string wallet = Constants.BurnAddress;

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
		if (_Config.Account != Constants.BurnAddress)
		{
			wallet = _Config.Account;
		}
		else
		{
#if UNITY_EDITOR
			if (TestWallet.Length == 42 && TestWallet.IndexOf("0x") == 0)
			{
				wallet = TestWallet;
			}
#endif
		}

		LoadTempNFT();

		//yield return StartCoroutine(QueryFakeNFTs());

		if (wallet != Constants.BurnAddress)
			yield return StartCoroutine(QueryOSWallet());

		if (OnNFTListComplete != null)
			OnNFTListComplete(loadedNFTs);

		IsNFTListComplete = true;

		foreach (var n in loadedNFTs)
		{
			if (OnNFTItemLoaded != null)
				OnNFTItemLoaded(n);
		}
	}

	private IEnumerator QueryOSWallet()
	{
		const string osAssetsAPI = "https://api.opensea.io/api/v1/assets";
		string osAssetsRequest = osAssetsAPI + "?owner=" + wallet;

		using (var www = UnityWebRequest.Get(osAssetsRequest))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;

			OpenSeaAssets result = null;
			try
			{
				result = JsonUtility.FromJson<OpenSeaAssets>(json);
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e);
			}

			if (result != null)
			{
				foreach (var a in result.assets)
				{
					if (NFTWhiteList.Find((w) => IsContractAddrEqual(w, a.asset_contract.address)) == null)
						continue;

					Debug.Log(a.ToString());

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
	}

	static public bool IsContractAddrEqual(string c1, string c2)
	{
		return string.Compare(c1, c2, true) == 0;
	}

	private IEnumerator QueryFakeNFTs()
	{
		const string api = "https://api.opensea.io/api/v1/assets?asset_contract_address={0}";

		DataNFTFake[] fakes = Resources.LoadAll<DataNFTFake>("");
		foreach (var f in fakes)
		{
			if (f.Enabled == false)
				continue;

			string url = string.Format(api, f.ContractAddr);

			foreach (string tokenId in f.TokenIds)
			{
				url += "&token_ids=" + tokenId;
			}

			using (var www = UnityWebRequest.Get(url))
			{
				yield return www.SendWebRequest();
				var json = www.downloadHandler.text;

				OpenSeaAssets result = JsonUtility.FromJson<OpenSeaAssets>(json);
				if (result != null)
				{
					foreach (var a in result.assets)
					{
						Debug.Log(a.ToString());

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
		}
	}

	private void LoadTempNFT()
	{
		DataNFTTemp nft = DataNFTTemp.Instance;
		if (nft == null)
			return;

		if (nft.enabled == false)
			return;

		int len = nft.InfoList.Count;
		for (int i=0; i<len; i++)
		{
			var n = nft.InfoList[i];

			NFTItemData item = new NFTItemData();
			item.Chain = "localhost";
			item.TokenId = i.ToString();
			item.URI = "temp";
			item.Contract = "ArcadiansTemp";
			item.Name = n.Name;
			item.Description = n.Description;
			item.Texture = n.Texture;
			item.Spr = n.Spr;
			item.CharClass = n.CharClass;

			loadedNFTs.Add(item);
		}
	}
}

}
