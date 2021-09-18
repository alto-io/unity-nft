using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataClasses", menuName = "UnityNFT/DataClasses", order = 1)]
public class DataClasses : ScriptableObject
{
	static private DataClasses instance;
	static public DataClasses Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataClasses>("DataClasses");
			}
			return instance;
		}
	}

	[System.Serializable]
	public class Info
	{
		public string Name;
		[Range(1,5)] public int HP;
		[Range(1,3)] public int AttackRange;
		[Range(1,5)] public int AttackSpeed;
		[Range(1,5)] public int MoveSpeed;
		[Range(1,5)] public int Damage;
		[Range(1,5)] public int Defense;
		[Range(1,5)] public int Agility;

		public bool IsMelee;
	}

	public List<Info> Classes = new List<Info>();

	public int HPMin = 100;
	public int HPMult = 15;
	public int StatMax = 5;
	public float CritMaxChance = 0.3f;
	public float CritMult = 1.5f;

	public Info GetRandom()
	{
		int index = Random.Range(0, Classes.Count);
		return Classes[index];
	}

	public Info GetByName(string name)
	{
		return Classes.Find((i) => (i.Name == name));
	}
}

}
