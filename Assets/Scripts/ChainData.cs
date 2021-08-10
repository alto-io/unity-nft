using UnityEngine;
using System.Collections.Generic;

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

[CreateAssetMenu(fileName = "ChainData", menuName = "UnityNFT/ChainData", order = 1)]
public class ChainData : ScriptableObject
{
	public string Chain = "ethereum";
	public string Network = "rinkeby";
	public string ExplorerAPIKey = "";

	[TextArea]
	public string Explorer721EventsCall = "";
	public bool Enabled = true;

	public List<NFTInfo> NFTList;

	public List<string> BlacklistContracts;
}


[System.Serializable]
public class PolygonExplorer721Events
{
	[System.Serializable]
	public class Result
	{
		public string contractAddress;
		public string tokenID;
	}
	public string message;
	public Result[] result;
}
