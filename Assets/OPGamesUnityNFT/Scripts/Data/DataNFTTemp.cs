using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataNFTTemp", menuName = "UnityNFT/DataNFTTemp", order = 1)]
public class DataNFTTemp : ScriptableObject
{
	[System.Serializable]
	public class Info
	{
		public string Name;
		public string Description;
		public string TokenId;
		public string CharClass;
		public Texture2D Texture;
		public Sprite Spr;
	}

	static private DataNFTTemp instance;
	static public DataNFTTemp Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataNFTTemp>("DataNFTTemp");
			}
			return instance;
		}
	}

	public List<Info> InfoList;
	public bool enabled = true;

	public Info GetRandom()
	{
		int index = Random.Range(0, InfoList.Count);
		return InfoList[index];
	}

	public Info GetByTokenId(string token)
	{
		return InfoList.Find((i) => i.TokenId == token);
	}
}

}
