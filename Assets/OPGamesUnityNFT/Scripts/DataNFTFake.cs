using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataNFTFake", menuName = "UnityNFT/DataNFTFake")]
public class DataNFTFake : ScriptableObject
{
	static private DataNFTFake instance;
	static public DataNFTFake Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataNFTFake>("DataNFTFake");
			}
			return instance;
		}
	}

	public bool Enabled = true;
	public string Chain;
	public string Network;
	public string ContractAddr;
	public string[] TokenIds;
}

}
