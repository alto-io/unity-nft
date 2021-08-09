using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LoadNFT : MonoBehaviour
{
    public Image img;

    async void Start()
    {
        img = GetComponent<Image>();
        if (img == null)
            return;

        string chain = "ethereum";
        string network = "rinkeby";
        string contract1155 = "0x0ECb00e4f1A671AB36fDe83EA5bEF66928993FD3";
        string contract721 = "0x98e2A72A4d222E748550F3526530B0C2c535A2E5";
        string account = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";
        string tokenId = "1";

        BigInteger balanceOf = await ERC721.BalanceOf(chain, network, contract721, account);
        print(balanceOf);

        if (balanceOf > 0)
        {
            string uri = await ERC721.URI(chain, network, contract721, tokenId);
            print(uri);

            StartCoroutine(GetImage(uri));

        }

//        BigInteger balanceOf = await ERC1155.BalanceOf(chain, network, contract1155, account, tokenId);
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

    public class URIData
    {
        public string image;
    }

    IEnumerator GetImage(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            string json = request.downloadHandler.text;
            Debug.LogFormat("Received: {0}", json);

            URIData data = JsonUtility.FromJson<URIData>(json);
            Debug.LogFormat("Image URL: {0}", data.image);

            yield return GetTextureAndSetImage(data.image);
        }
    }

	IEnumerator GetTextureAndSetImage(string url)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();

		Texture2D tex = (Texture2D)DownloadHandlerTexture.GetContent(www);
        if (tex == null)
        {
            Debug.LogError("Invalid texture");
            yield break;
        }

		Sprite spr = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
        if (spr == null)
        {
            Debug.LogError("Cannot create sprite");
            yield break;
        }

        img.sprite = spr;
	}
}