using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

[CreateAssetMenu(fileName = "DataClasses", menuName = "UnityNFT/DataClasses", order = 1)]
public class DataClasses : ScriptableObject
{
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

		public bool IsMelee;
	}

	public List<Info> Classes = new List<Info>();
}

}
