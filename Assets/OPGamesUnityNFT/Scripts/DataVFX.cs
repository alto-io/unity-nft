using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataVFX", menuName = "UnityNFT/DataVFX", order = 1)]
public class DataVFX : ScriptableObject
{
	[System.Serializable]
	public class Info
	{
		public string Name;
		public GameObject Prefab;
	}

	static private DataVFX instance;
	static public DataVFX Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataVFX>("DataVFX");
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
