using System.Collections;
using UnityEngine;

namespace OPGames.NFT
{

public interface INFTLoader
{
	public bool NeedToCallURI { get; }
	public string Contract { get; }
	public IEnumerator LoadNFTData(NFTItemData n, System.Action<NFTItemData> onDone);
}

}
