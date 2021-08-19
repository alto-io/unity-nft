using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace OPGames.NFT
{

public class UINFTItem : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private Text textName;
	[SerializeField] private Text textDesc;

	public void Fill(NFTItemData nft)
	{
		if (textName != null) 
		{
			if (string.IsNullOrEmpty(nft.Name))
			{
				textName.text = nft.TokenId;
			}
			else
			{
				textName.text = nft.Name;
			}
			
		}
		if (textDesc != null) 
		{
			if (string.IsNullOrEmpty(nft.Description))
			{
				textDesc.text = nft.Contract;
			}
			else
			{
				textDesc.text = nft.Description;
			}
		}

		if (nft.Texture == null)
		{
			StartCoroutine(GetTextureAndSetImage(nft));
		}
		else
		{
			Utils.SetImageTexture(image, nft.Texture);
		}
	}

	private IEnumerator GetTextureAndSetImage(NFTItemData nft)
	{
		if (string.IsNullOrEmpty(nft.ImageURL))
			yield break;

		UnityWebRequest www = UnityWebRequestTexture.GetTexture(nft.ImageURL);
		yield return www.SendWebRequest();

		Texture2D tex = (Texture2D)DownloadHandlerTexture.GetContent(www);
		nft.Texture = tex;

		Utils.SetImageTexture(image, tex);
	}
}

}
