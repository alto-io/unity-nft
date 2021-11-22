using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

static public class GameGlobals
{
	[System.Serializable]
	public class SelectedInfo
	{
		public string Id;
		public Vector2Int Pos = new Vector2Int(-1,-1);
	}

	static public List<SelectedInfo> Offense = new List<SelectedInfo>();
	static public List<SelectedInfo> Defense = new List<SelectedInfo>();
}

}
