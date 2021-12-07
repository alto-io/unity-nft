using UnityEngine;

namespace OPGames.NFT
{

static public class Env
{
	static public string Wallet = "0x0000000000000000000000000000000000000000";
	static public void Load()
	{
		var asset = Resources.Load<TextAsset>("env");
		if (asset != null)
		{
			var data = JsonUtility.FromJson<EnvData>(asset.text);
			if (data != null)
			{
				Wallet = data.wallet;
				Debug.LogFormat("Loaded wallet {0}", Wallet);
			}
		}
	}
}

}
