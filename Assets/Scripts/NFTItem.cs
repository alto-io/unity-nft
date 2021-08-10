using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NFTItem : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private Text textName;
	[SerializeField] private Text textDesc;

	public void Fill(string name, string desc, string imageUrl)
	{
		if (textName != null) textName.text = name;
		if (textDesc != null) textDesc.text = desc;

		StartCoroutine(GetTextureAndSetImage(imageUrl));
	}

	private IEnumerator GetTextureAndSetImage(string url)
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

        image.sprite = spr;
	}
}
