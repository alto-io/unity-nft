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

	static public string Wallet = "";
	static public List<SelectedInfo> Offense = new List<SelectedInfo>();
	static public List<SelectedInfo> Defense = new List<SelectedInfo>();

	static public List<SelectedInfo> Enemy = new List<SelectedInfo>();
	static public PVPPlayerModel EnemyModel = null;

	static public void CopyList(List<SelectedInfo> dst, List<SelectedInfo> src)
	{
		dst.Clear();
		for (int i=0; i<src.Count; i++)
		{
			var srcInfo = src[i];
			var dstInfo = new SelectedInfo();
			dstInfo.Id  = srcInfo.Id;
			dstInfo.Pos = srcInfo.Pos;
			dst.Add(srcInfo);
		}
	}
}

}
