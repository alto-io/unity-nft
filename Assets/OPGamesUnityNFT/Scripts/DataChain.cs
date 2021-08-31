using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataChain", menuName = "UnityNFT/DataChain", order = 1)]
public class DataChain : ScriptableObject
{
	public string Chain = "ethereum";
	public string Network = "rinkeby";
	public string ExplorerAPIKey = "";

	[TextArea]
	public string Explorer721EventsCall = "";
	public bool Enabled = true;

	public List<string> BlacklistContracts;
}

}
