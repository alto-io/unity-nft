using UnityEngine;
using System;
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
		int index = UnityEngine.Random.Range(0, InfoList.Length);
		return InfoList[index];
	}

	public Info GetByName(string name)
	{
		for (int i=0; i<InfoList.Length; i++)
		{
			var info = InfoList[i];
			int comparison = string.Compare(info.Name, name, comparisonType: StringComparison.OrdinalIgnoreCase);
			if (comparison == 0)
				return info;
		}

		return null;
	}
}

}
