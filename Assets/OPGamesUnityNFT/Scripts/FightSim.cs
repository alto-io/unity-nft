using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OPGames.NFT
{


// Simulate the whole fight and generate events as they happen
public class FightSim
{
	private class PosDistance
	{
		public Vector2Int Pos;
		public float Distance;
	}

	static public Grid GridObj;
	static private List<PosDistance> neighbors = null;

	public List<ModelChar>       Chars     = new List<ModelChar>();
	public List<ModelChar>       TeamA     = new List<ModelChar>();
	public List<ModelChar>       TeamB     = new List<ModelChar>();

	public List<ReplayEvtMove>   EvtMove   = new List<ReplayEvtMove>();
	public List<ReplayEvtAttack> EvtAttack = new List<ReplayEvtAttack>();
	public List<ReplayEvtDamage> EvtDamage = new List<ReplayEvtDamage>();
	public List<ReplayEvtBuff>   EvtBuff   = new List<ReplayEvtBuff>();


	public void Init()
	{
		if (neighbors == null)
		{
			neighbors = new List<PosDistance>();
			for (int i=0; i<8; i++)
				neighbors.Add(new PosDistance());
		}
	}

	public void AddChar(FightChar c)
	{
		var     info    = c.ClassInfo;
		Vector3 p       = c.transform.position;

		ModelChar model = new ModelChar();
		model.Id        = c.Id;
		model.Team      = c.Team;
		model.Pos       = GridObj.WorldToGrid(p);
		model.Hp        = (int)info.HpVal;
		model.CdAttack  = model.CdAttackFull = (int)(info.SkillSpeedSecs * Constants.TicksPerSec);
		model.Damage    = (int)info.DamageVal;
		model.Defense   = (int)info.DefenseVal;
		model.CurrState = ModelChar.State.Idle;
		model.ClassInfo = c.ClassInfo;

		Chars.Add(model);

		if (c.Team)
		{
			TeamA.Add(model);
		}
		else
		{
			TeamB.Add(model);
		}
	}

	public void Simulate()
	{
		int tickId = 0;

		while (IsTeamAlive(TeamA) && IsTeamAlive(TeamB) && tickId < 10)
		{
			tickId++;

			foreach (var c in Chars)
			{
				c.CdAttack--;
				c.CdAttack--;
			}

			foreach (var c in TeamA)
			{
				if (c.CurrState == ModelChar.State.Idle)
				{
					DecideAction(c, TeamB);
				}
			}

			foreach (var c in TeamB)
			{
				if (c.CurrState == ModelChar.State.Idle)
				{
					DecideAction(c, TeamA);
				}
			}
		}
	}

	static private void DecideAction(ModelChar src, List<ModelChar> enemies)
	{
		var target = FindTarget(src, enemies);
		if (target == null)
			return;

		src.TargetId = target.Id;

		float distance = (src.Pos - target.Pos).magnitude;
		if (distance > (float)src.ClassInfo.AttackRange)
		{
			// find path
			Vector2Int dest = target.Pos;
			List<PosDistance> list = GetNeighbors(src.Pos, target.Pos);
			foreach (var l in list)
			{
				if (l.Distance == Mathf.Infinity)
					continue;

				if (GridObj.GetOccupied(l.Pos) != 0)
					continue;

				dest = l.Pos;
				break;
			}

			var path = GridObj.FindPath(src.Pos, dest);
			if (path != null && path.Count > 0)
			{
				var start = src.Pos;
				var end = path[0].pos;
				GridObj.ClearOccupied(start);
				GridObj.SetOccupied(end, src.Id);

				Debug.Log($"Char {src.Id} will move to {end.ToString()}");

				src.Pos = end;
			}
		}
		else
		{
			src.CurrState = ModelChar.State.Attack;
		}
	}

	static private ModelChar FindTarget(ModelChar src, List<ModelChar> enemies)
	{
		float nearestDist = Mathf.Infinity;
		ModelChar nearest = null;

		for (int i=0; i<enemies.Count; i++)
		{
			ModelChar e = enemies[i];
			if (e.Hp <= 0) continue;

			float distance = (src.Pos - e.Pos).sqrMagnitude;
			if (nearestDist > distance)
			{
				nearest = e;
				nearestDist = distance;
			}
		}
		return nearest;
	}

	static private bool IsTeamAlive(List<ModelChar> team)
	{
		int hpTotal = 0;
		foreach (var c in team)
			hpTotal += c.Hp;

		return hpTotal > 0;
	}

	static private List<PosDistance> GetNeighbors(Vector2Int start, Vector2Int end)
	{
		neighbors[0].Pos.Set(end.x-1, end.y-1); 
		neighbors[1].Pos.Set(end.x-1, end.y-0); 
		neighbors[2].Pos.Set(end.x-1, end.y+1); 
		neighbors[3].Pos.Set(end.x-0, end.y-1); 
		neighbors[4].Pos.Set(end.x-0, end.y+1); 
		neighbors[5].Pos.Set(end.x+1, end.y-1); 
		neighbors[6].Pos.Set(end.x+1, end.y-0); 
		neighbors[7].Pos.Set(end.x+1, end.y+1); 

		for (int i=0; i<neighbors.Count; i++)
			neighbors[i].Distance = Mathf.Infinity;

		foreach (var p in neighbors)
		{
			if (p.Pos.x < 0 || p.Pos.y < 0)
				continue;

			p.Distance = (start-p.Pos).sqrMagnitude;
		}

		neighbors.Sort((a, b) => 
		{ 
			if      (a.Distance < b.Distance) return -1;
			else if (a.Distance > b.Distance) return 1;
			else return 0;
		});

		return neighbors;
	}

}

}
