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
	public class ClassStatsRow
	{
		public string Name;

		public int Hp;
		public float HpVal;

		public int AttackRange;

		public int AttackSpeed;
		public float AttackSpeedSecs;

		public int SkillSpeed;
		public float SkillSpeedSecs;

		public int Damage;
		public float DamageVal;

		public int Defense;
		public float DefenseVal;

		public int Agility;
		public float CritChance;

		public string VFXAttack;
		public string Skills;
	}

	[System.Serializable]
	public class ClassStats
	{
		public ClassStatsRow[] Data;
	}

	[Tooltip("Paste here what you got from the DataClassStats sheet")]
	[TextArea(5, 10)]
	public string JsonClassStats;

	public List<ClassStatsRow> Classes;

	public void LoadJson()
	{
		ClassStats temp = JsonUtility.FromJson<ClassStats>(JsonClassStats);
		if (temp == null)
			return;

		Classes.Clear();
		foreach (var r in temp.Data)
		{
			Classes.Add(r);
		}
	}

	public ClassStatsRow GetRandom()
	{
		int index = Random.Range(0, Classes.Count);
		return Classes[index];
	}

	public ClassStatsRow GetByName(string name)
	{
		return Classes.Find((i) => (i.Name == name));
	}

	private void OnValidate()
	{
		LoadJson();
	}
}

}
