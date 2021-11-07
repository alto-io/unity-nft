using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataSkills", menuName = "UnityNFT/DataSkills")]
public class DataSkills : ScriptableObject
{
#region Classes
	[System.Serializable]
	public class SkillsRow
	{
		public string Name;
		public string Description;
		public string VFXPrefab;
		public string Icon;
		public float CooldownSecs;
		public int Damage;
		public int Heal;
		public float StunSecs;
		public int DamageAdd;
		public int DefenseAdd;
		public float AttackSpeedAdd;
	}

	[System.Serializable]
	public class SkillsSheet
	{
		public SkillsRow[] Data;
	}
#endregion

#region Static
	static private DataSkills instance;
	static public DataSkills Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = Resources.Load<DataSkills>("DataSkills");
			}
			return instance;
		}
	}
#endregion

	[Tooltip("Paste here what you got from the DataSkills sheet")]
	[TextArea(5, 10)]
	public string Json;

	public List<SkillsRow> Skills;

	public void LoadJson()
	{
		SkillsSheet temp = JsonUtility.FromJson<SkillsSheet>(Json);
		if (temp == null)
			return;

		Skills.Clear();
		foreach (var r in temp.Data)
		{
			Skills.Add(r);
		}
	}

	private void OnValidate()
	{
		LoadJson();
	}

	public SkillsRow GetByName(string name)
	{
		return Skills.Find((s) => s.Name == name);
	}
}

}
