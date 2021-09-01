using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataNFTSprites", menuName = "UnityNFT/DataNFTSprites", order = 1)]
public class DataNFTSprites : ScriptableObject
{
	[System.Serializable]
	public class Info
	{
		public string Name;
		public Texture2D Texture;
	}

	static private DataNFTSprites instance;
	static public DataNFTSprites Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataNFTSprites>("DataNFTSprites");
			}
			return instance;
		}
	}

	public Info[] InfoList;

	public Info GetRandom()
	{
		int index = Random.Range(0, InfoList.Length);
		return InfoList[index];
	}
}

}
