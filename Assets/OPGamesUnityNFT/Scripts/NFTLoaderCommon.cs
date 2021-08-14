using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace OPGames.NFT
{

public class NFTLoaderCommon : INFTLoader
{
	public bool NeedToCallURI { get { return true; } }
	public string Contract { get { return ""; } }

	public IEnumerator LoadNFTData(NFTItemData n, System.Action<NFTItemData> onDone)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(n.URI))
		{
			yield return request.SendWebRequest();
			string json = request.downloadHandler.text;

			URIData data = JsonUtility.FromJson<URIData>(json);
			if (data != null)
			{
				n.Name = data.name;
				n.Description = data.description;
				n.ImageURL = data.image;

				if (onDone != null)
					onDone(n);
			}
		}
	}
}

}
