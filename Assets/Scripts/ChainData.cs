using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "ChainData", menuName = "UnityNFT/ChainData", order = 1)]
public class ChainData : ScriptableObject
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
