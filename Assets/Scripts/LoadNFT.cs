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
	public string description;
	public string image;
}


[System.Serializable]
public class NFTItemData
{
	public NFTInfo Info;
	public string TokenId;
	public string URI;
	public string ImageURI;
	public URIData Metadata;
}

public class LoadNFT : MonoBehaviour
{
	public RectTransform Content;
	public GameObject NFTPrefab;
    public Image img;
	public string Wallet = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";

	public List<NFTItemData> LoadedNFTs = new List<NFTItemData>();

    async void Start()
    {

		ChainData[] chains = Resources.LoadAll<ChainData>("");
		foreach (var c in chains)
		{
			if (c.Enabled == false)
				continue;
			
			Debug.LogFormat("Found ChainData: {0}", c.Chain);
			await LoadChain(c);
		}

//        string chain = "ethereum";
//        string network = "rinkeby";
//        string contract1155 = "0x0ECb00e4f1A671AB36fDe83EA5bEF66928993FD3";
//        string contract721 = "0x98e2A72A4d222E748550F3526530B0C2c535A2E5";
//        string tokenId = "1";
//
//        BigInteger balanceOf = await ERC721.BalanceOf(chain, network, contract721, Wallet);
//        print(balanceOf);
//
//        if (balanceOf > 0)
//        {
//            string uri = await ERC721.URI(chain, network, contract721, tokenId);
//            print(uri);
//
//            StartCoroutine(GetImage(uri));
//
//        }

//        BigInteger balanceOf = await ERC1155.BalanceOf(chain, network, contract1155, Wallet, tokenId);
//        print(balanceOf);
//
//        if (balanceOf > 0)
//        {
//            sprite.GetComponent<SpriteRenderer>().color = Color.red;
//
//            string uri = await ERC1155.URI(chain, network, contract, tokenId);
//            print(uri);
//        }
    }

	private async Task LoadChain(ChainData chain)
	{
		if (chain == null)
		{
			Debug.LogError("LoadNFT:LoadChain - chain is null");
			return;
		}

		foreach (var info in chain.NFTList)
		{
			if (info.Enabled == false)
				continue;

			string apiCall = string.Format(chain.Explorer721EventsCall, Wallet, chain.ExplorerAPIKey);

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

				foreach (var r in events.result)
				{
					string owner = await ERC721.OwnerOf(chain.Chain, chain.Network, r.contractAddress, r.tokenID);
					if (owner != Wallet)
						continue;

					string uri = await ERC721.URI(chain.Chain, chain.Network, r.contractAddress, r.tokenID);
					Debug.Log(uri);

					NFTItemData item = new NFTItemData();
					//item.Info = info;
					item.TokenId = r.tokenID;
					item.URI = uri;

					LoadedNFTs.Add(item);
				}
			}

			StartCoroutine(LoadURIData());

			//if (info.Type == NFTType.ERC721)
			//{
			//	await LoadNFT721(chain.Chain, chain.Network, info);
			//}
			//else if (info.Type == NFTType.ERC1155)
			//{
			//	await LoadNFT1155(chain.Chain, chain.Network, info);
			//}
			//else
			//{
			//	Debug.LogErrorFormat("LoadNFT:LoadChain - nft type not supported: {0}-{1}", info.Name, info.Type);
			//}
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
			using (UnityWebRequest request = UnityWebRequest.Get(n.URI))
			{
				yield return request.SendWebRequest();
				string json = request.downloadHandler.text;
				Debug.LogFormat("Received: {0}", json);

				URIData data = JsonUtility.FromJson<URIData>(json);
				n.Metadata = data;

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
