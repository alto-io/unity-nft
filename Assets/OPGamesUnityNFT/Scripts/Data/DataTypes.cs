using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

static public class Constants
{
	static public string BurnAddress = "0x0000000000000000000000000000000000000000";
	static public int TicksPerSec = 20;
	static public float TickDt = 1.0f / TicksPerSec;
	static public int MoveTicks = 5;
}

[System.Serializable]
public class NFTItemData
{
	public string Chain;
	public string TokenId;
	public string Contract;
	public string URI;

	public string Name;
	public string Description;
	public string ImageURL;
	public string CharClass;

	public Texture2D Texture;
	public Sprite Spr;

	public string UniqueId
	{
		get { return Contract + "-" + TokenId; }
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2}", Chain, Contract, TokenId);
	}
}

[System.Serializable]
public class OpenSeaAssetItemAssetContract
{
	public string address;
	public string name;
}

[System.Serializable]
public class OpenSeaAssetItem
{
	public string token_id;
	public string image_preview_url;
	public string name;
	public string description;

	public OpenSeaAssetItemAssetContract asset_contract;
}

[System.Serializable]
public class OpenSeaAssets
{
	public OpenSeaAssetItem[] assets;
}



}
