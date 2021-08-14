using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace OPGames.NFT
{

public class NFTLoaderCK : INFTLoader
{
	public bool NeedToCallURI { get { return false; } }
	public string Contract { get { return "0x06012c8cf97bead5deae237070f9587f8e7a266d"; } }

	public IEnumerator LoadNFTData(NFTItemData n, System.Action<NFTItemData> onDone)
	{
		string apiUrl = "https://api.cryptokitties.co/kitties/" + n.TokenId;

		using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
		{
			yield return request.SendWebRequest();
			string json = request.downloadHandler.text;

			URIData data = JsonUtility.FromJson<URIData>(json);
			n.Name = data.name;
			n.Description = data.bio;
			n.ImageURL = data.image_url_png;

			if (onDone != null)
				onDone(n);
		}
	}
}

}
