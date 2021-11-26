using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

namespace OPGames.NFT
{

public class UINFTItem : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private TextMeshProUGUI textName;
	[SerializeField] private TextMeshProUGUI textDesc;

	public Sprite GetSprite()
	{
		return image.sprite;
	}

	public void Fill(string id)
	{
		var nftMgr = NFTManager.Instance;
		var nft = nftMgr.GetNFTItemDataById(id);
		if (nft != null)
		{
			Fill(nft);
			return;
		}

		// try to get it from opensea
		string[] s = id.Split('-');
		if (s.Length >= 2)
		{
			string contract = s[0];
			string token = s[1];

			nftMgr.QueryNFT(contract, new string[] { token }, () =>
				{
					Fill(nftMgr.GetNFTItemDataById(id));
				});
		}

	}

	public void Fill(NFTItemData nft)
	{
		if (nft == null)
			return;

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
