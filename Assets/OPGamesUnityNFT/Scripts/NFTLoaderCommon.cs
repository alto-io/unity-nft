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

			try 
			{
				URIData data = JsonUtility.FromJson<URIData>(json);
				if (data != null)
				{
					n.Name = data.name;
					n.Description = data.description;
					n.ImageURL = data.image;
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarningFormat("Error: {0}\nData: {1}", e.ToString(), n.ToString());
			}
		}
		if (onDone != null)
			onDone(n);
	}
}

}
