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

	public List<ModelChar>       Chars     = new List<ModelChar>();
	public List<ModelChar>       TeamA     = new List<ModelChar>();
	public List<ModelChar>       TeamB     = new List<ModelChar>();

	public List<ReplayEvtMove>   EvtMove   = new List<ReplayEvtMove>();
	public List<ReplayEvtAttack> EvtAttack = new List<ReplayEvtAttack>();
	public List<ReplayEvtDamage> EvtDamage = new List<ReplayEvtDamage>();
	public List<ReplayEvtBuff>   EvtBuff   = new List<ReplayEvtBuff>();

	public List<ReplayEvt>       EvtAll    = new List<ReplayEvt>();

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

	public List<ReplayEvt> Simulate()
	{
		int tickId = 0;

		while (IsTeamAliveFunc(TeamA) && IsTeamAliveFunc(TeamB))
		{
			tickId++;

			foreach (var c in TeamA)
			{
				c.CdAttack--;
				SimulateChar(tickId, c, TeamB);
			}

			foreach (var c in TeamB)
			{
				c.CdAttack--;
				SimulateChar(tickId, c, TeamA);
			}
		}

		EvtAll.Sort((a, b) => { return a.Tick - b.Tick; });

		return EvtAll;
	}

	private void SimulateChar(int tickId, ModelChar src, List<ModelChar> enemies)
	{
		switch (src.CurrState)
		{
			case ModelChar.State.Idle:
				DecideAction(tickId, src, enemies);
				break;

			case ModelChar.State.Move:
				src.CdMove--;
				if (src.CdMove <= 0)
				{
					src.Pos = src.PosDest;
					DecideAction(tickId, src, enemies);
				}

				break;

			case ModelChar.State.Attack:
				if (src.CdAttack > 0)
					break;

				ModelChar target = enemies.Find((c) => c.Id == src.TargetId);
				if (target != null)
				{
					target.Hp -= src.Damage;

					//Debug.Log($"Attack target {target.Id}, damage {src.Damage}");

					ReplayEvtAttack evt = new ReplayEvtAttack();
					evt.Tick = tickId;
					evt.Char = src.Id;
					evt.Targ = target.Id;

					EvtAttack.Add(evt);
					EvtAll.Add(evt);

					if (target.Hp <= 0)
					{
						//Debug.Log($"Dead target {target.Id}");
						src.CurrState = ModelChar.State.Idle;
					}

					ReplayEvtDamage evt2 = new ReplayEvtDamage();
					evt2.Tick = tickId + 2;
					evt2.Char = src.Id;
					evt2.Dmg = src.Damage;

					EvtDamage.Add(evt2);
					EvtAll.Add(evt2);

					src.CdAttack = src.CdAttackFull;
				}
				break;
		}
	}

	private void DecideAction(int tickId, ModelChar src, List<ModelChar> enemies)
	{
		var target = FindTargetFunc(src, enemies);
		if (target == null)
			return;

		src.TargetId = target.Id;

		float distance = (src.Pos - target.Pos).magnitude;
		if (distance > (float)src.ClassInfo.AttackRange)
		{
			src.PosDest = GetNextNodeFunc(src, target.Pos);
			//if (src.PosDest != src.Pos)
			//	src.CdMove = 2;

			ReplayEvtMove evt = new ReplayEvtMove();
			evt.Tick = tickId;
			evt.Char = src.Id;
			evt.From = src.Pos;
			evt.To = src.PosDest;
			EvtMove.Add(evt);
			EvtAll.Add(evt);

			src.Pos = src.PosDest;
		}
		else
		{
			src.CurrState = ModelChar.State.Attack;
		}
	}

	// Pure function, don't modify inputs
	static private Vector2Int GetNextNodeFunc(ModelChar src, Vector2Int dest)
	{
		// find path
		List<PosDistance> list = GetNeighborsFunc(src.Pos, dest);
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

			return end;
		}

		return src.Pos;
	}

	static private ModelChar FindTargetFunc(ModelChar src, List<ModelChar> enemies)
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

	static private bool IsTeamAliveFunc(List<ModelChar> team)
	{
		int hpTotal = 0;
		foreach (var c in team)
			hpTotal += c.Hp;

		return hpTotal > 0;
	}

	static private List<PosDistance> neighbors = null;
	static private List<PosDistance> GetNeighborsFunc(Vector2Int start, Vector2Int end)
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
