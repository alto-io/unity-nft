using UnityEngine;
using System.Collections.Generic;

namespace OPGames.NFT
{

static public class GameGlobals
{
	public class SelectedInfo
	{
		public string Id;
		public Vector2Int Pos;
	}

	static public List<SelectedInfo> Selected = new List<SelectedInfo>();
}

}
