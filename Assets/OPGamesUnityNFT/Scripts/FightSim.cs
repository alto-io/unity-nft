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

	public List<ReplayEvtDamage> DelayedDamage = new List<ReplayEvtDamage>();

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
		model.Damage    = (int)info.DamageVal;
		model.Defense   = (int)info.DefenseVal;
		model.CurrState = ModelChar.State.Idle;
		model.ClassInfo = c.ClassInfo;
		model.CritChance= (int)Mathf.Round(c.ClassInfo.CritChance * 100.0f);

		int cdAttack = (int) Mathf.Floor(info.AttackSpeedSecs * (float)Constants.TicksPerSec);
		model.CdAttack = cdAttack;
		model.CdAttackFull = cdAttack;

		Chars.Add(model);

		if (c.Team)
		{
			TeamA.Add(model);
		}
		else
		{
			TeamB.Add(model);
		}

		// So they don't attack all in the same tick?
		// But won't it be not fair if we do this?
		 Chars.Sort((a, b) => { return a.CdAttack - b.CdAttack; });
		 for (int i=0; i<Chars.Count; i++)
		 	Chars[i].CdAttack += i;
	}

	private List<int> indexToDeleteInDelayed = new List<int>();
	private void ApplyDelayedDamage(int tickId)
	{
		int len = DelayedDamage.Count;
		if (len <= 0)
			return;

		var listTemp = DelayedDamage.FindAll((d) => (d.Tick == tickId));
		foreach (var d in listTemp)
		{
			ModelChar target = Chars.Find((c) => c.Id == d.Char);
			if (target != null)
			{
				target.Hp -= d.Dmg;

				if (target.Hp <= 0)
				{
					//Debug.Log($"{tickId} = {target.Id} is dead");
					target.CurrState = ModelChar.State.Dead;
				}
			}
		}

		DelayedDamage.RemoveAll((d) => (d.Tick == tickId));
	}

	public List<ReplayEvt> Simulate()
	{
		int tickId = 0;

		foreach (var c in Chars)
		{
			if (c.Team == true) c.Enemies = TeamB;
			else                c.Enemies = TeamA;
		}

		while (IsTeamAliveFunc(TeamA) && IsTeamAliveFunc(TeamB))
		{
			tickId++;

			ApplyDelayedDamage(tickId);

			foreach (var c in TeamA)
			{
				c.CdAttack--;
				SimulateChar(tickId, c);
			}

			foreach (var c in TeamB)
			{
				c.CdAttack--;
				SimulateChar(tickId, c);
			}
		}

		EvtAll.Sort((a, b) => { return a.Tick - b.Tick; });

		return EvtAll;
	}

	private void SimulateChar(int tickId, ModelChar src)
	{
		switch (src.CurrState)
		{
			case ModelChar.State.Idle:   SimulateIdle(tickId, src);   break;
			case ModelChar.State.Move:   SimulateMove(tickId, src);   break;
			case ModelChar.State.Attack: SimulateAttack(tickId, src); break;
		}
	}

	private void SimulateIdle(int tickId, ModelChar src)
	{
		DecideAction(tickId, src);
	}

	private void SimulateMove(int tickId, ModelChar src)
	{
		src.CdMove--;
		if (src.CdMove <= 0)
		{
			src.Pos = src.PosDest;
			src.CurrState = ModelChar.State.Idle;
		}
	}

	private void SimulateAttack(int tickId, ModelChar src)
	{
		if (src.CdAttack > 0)
			return;

		ModelChar target = src.Enemies.Find((c) => c.Id == src.TargetId);
		if (target == null)
			return;

		//Debug.Log($"Attack target {target.Id}, damage {src.Damage}");

		ReplayEvtAttack evt = new ReplayEvtAttack();
		evt.Tick = tickId;
		evt.Char = src.Id;
		evt.Targ = target.Id;

		if (src.Pos.y < target.Pos.y)
		{
			evt.Dir = AttackDir.North;
		}
		else if (src.Pos.y > target.Pos.y)
		{
			evt.Dir = AttackDir.South;
		}
		else if (src.Pos.x < target.Pos.x)
		{
			evt.Dir = AttackDir.East;
		}
		else
		{
			evt.Dir = AttackDir.West;
		}

		EvtAttack.Add(evt);
		EvtAll.Add(evt);

		if (target.Hp-src.Damage <= 0)
		{
			src.CurrState = ModelChar.State.Idle;
		}

		ReplayEvtDamage evt2 = new ReplayEvtDamage();
		evt2.Tick = tickId + (Constants.TicksPerSec / 4);
		evt2.Char = target.Id;
		evt2.Dmg = src.Damage;

		if (Random.Range(0, 100) <= src.CritChance)
		{
			evt2.Dmg = src.Damage * 2;
			evt2.Crit = true;
		}

		DelayedDamage.Add(evt2);
		EvtDamage.Add(evt2);
		EvtAll.Add(evt2);

		src.CdAttack = src.CdAttackFull;
	}

	private void DecideAction(int tickId, ModelChar src)
	{
		var target = FindTargetFunc(src);
		if (target == null)
			return;

		src.TargetId = target.Id;

		float distance = (src.Pos - target.Pos).magnitude;
		if (distance > (float)src.ClassInfo.AttackRange)
		{
			src.PosDest = GetNextNodeFunc(src, target.Pos);
			if (src.PosDest != src.Pos)
				src.CdMove = Constants.MoveTicks;

			ReplayEvtMove evt = new ReplayEvtMove();
			evt.Tick = tickId;
			evt.Char = src.Id;
			evt.From = src.Pos;
			evt.To = src.PosDest;
			evt.NumTicks = Constants.MoveTicks;

			EvtMove.Add(evt);
			EvtAll.Add(evt);

			src.Pos = src.PosDest;
			src.CurrState = ModelChar.State.Move;
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

	static private ModelChar FindTargetFunc(ModelChar src)
	{
		float nearestDist = Mathf.Infinity;
		ModelChar nearest = null;

		for (int i=0; i<src.Enemies.Count; i++)
		{
			ModelChar e = src.Enemies[i];
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
		foreach (var c in team)
			if (c.Hp > 0)
				return true;

		return false;
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
