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
		public int HP;
		public int AttackRange;
		public int AttackSpeed;
		public int MoveSpeed;
		public int Damage;
		public int Defense;
		public int Agility;

		public bool IsMelee;
	}

	public List<Info> Classes = new List<Info>();

	public int HPMult = 15;
	public int StatMax = 9;
	public float CritMaxChance = 0.3f;
	public float CritMult = 1.5f;

	public Info GetRandom()
	{
		int index = Random.Range(0, Classes.Count);
		return Classes[index];
	}
}

}
