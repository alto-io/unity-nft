using UnityEngine;
using UnityEngine.UI;

namespace OPGames.NFT
{

static public class Utils
{
	static public void SetImageTexture(Image img, Texture2D tex)
	{
		if (img == null || tex == null)
			return;

		Sprite spr = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
        if (spr == null)
        {
            Debug.LogError("Cannot create sprite");
			return;
        }

        img.sprite = spr;
		img.preserveAspect = true;
	}
}

}
