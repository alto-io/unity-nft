using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

public enum NFTType
{
	ERC721,
	ERC1155
}

[System.Serializable]
public class NFTInfo
{
	public string Name;
	public string Contract;
	public NFTType Type;
	public bool Enabled = true;
}

[System.Serializable]
public class Explorer721Events
{
	[System.Serializable]
	public class Result
	{
		public string contractAddress;
		public string tokenID;
		public bool isDoneProcessing = false; // not from JSON
	}
	public string message;
	public Result[] result;

	public string chain;
	public string network;
	public List<string> blacklistContracts;

	public bool IsDoneProcessing()
	{
		foreach (Result r in result)
			if (r.isDoneProcessing == false)
				return false;

		return true;
	}
}

[System.Serializable]
public class URIData
{
	public string name;
	public string bio;
	public string description;
	public string image;
	public string image_url_png;
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

	public string UniqueId
	{
		get { return Contract + "-" + TokenId; }
	}
}


}
