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

	public List<ReplayEvt>       EvtAll    = new List<ReplayEvt>();
	public List<ReplayEvt>       DelayedEffects = new List<ReplayEvt>();

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
		// Save basic info
		var       info   = c.ClassInfo;
		Vector3   p      = c.transform.position;
		ModelChar model  = new ModelChar();

		model.Id         = c.Id;
		model.Team       = c.Team;
		model.Pos        = GridObj.WorldToGrid(p);
		model.Hp         = (int)info.HpVal;
		model.Damage     = (int)info.DamageVal;
		model.Defense    = (int)info.DefenseVal;
		model.CurrState  = ModelChar.State.Idle;
		model.ClassInfo  = c.ClassInfo;
		model.CritChance = (int)Mathf.Round(c.ClassInfo.CritChance * 100.0f);

		// Set cooldowns
		int cdAttack       = SecsToTicksFunc(info.AttackSpeedSecs);
		model.CdAttack     = cdAttack;
		model.CdAttackFull = cdAttack;

		// Add to the appropriate lists
		Chars.Add(model);
		if (c.Team) TeamA.Add(model);
		else        TeamB.Add(model);

		// Skills
		char[] seps         = new char[] { ' ', ',' };
		var dataSkills      = DataSkills.Instance;
		string[] skillNames = c.ClassInfo.Skills.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);
		foreach (string s in skillNames)
		{
			var skillRow = dataSkills.GetByName(s);
			if (skillRow == null) continue;

			var newSkill  = new ModelSkill();
			newSkill.Info = skillRow;
			newSkill.Cd   = SecsToTicksFunc(skillRow.CooldownSecs);
			model.Skills.Add(newSkill);
		}

		// So they don't attack all in the same tick?
		// But won't it be not fair if we do this?
		 Chars.Sort((a, b) => { return a.CdAttack - b.CdAttack; });
		 for (int i=0; i<Chars.Count; i++)
		 	Chars[i].CdAttack += i;
	}

	private void TickCooldowns(ModelChar m)
	{
		bool hasBuffToRemove = false;
		for (int i=0; i<m.Buffs.Count; i++)
		{
			m.Buffs[i].Cd--;
			if (m.Buffs[i].Cd <= 0)
				hasBuffToRemove = true;
		}

		if (hasBuffToRemove)
			m.Buffs.RemoveAll((b) => b.Cd <= 0);

		var stun = m.Buffs.Find((b) => b.Stun == true);
		if (stun != null)
			return;

		m.CdAttack--;
		m.CdMove--;
		for (int i=0; i<m.Skills.Count; i++)
		{
			m.Skills[i].Cd--;
		}
	}

	private void ApplyDelayedEffects(int tickId)
	{
		int len = DelayedEffects.Count;
		if (len <= 0) return;

		var listTemp = DelayedEffects.FindAll((e) => (e.Tick == tickId));
		foreach (var e in listTemp)
		{
			if (e is ReplayEvtDamage)
			{
				var damage = (ReplayEvtDamage)e;
				ModelChar target = Chars.Find((c) => c.Id == damage.Char);
				target.Hp -= damage.Dmg;
				if (target.Hp <= 0)
				{
					//Debug.Log($"{tickId} = {target.Id} is dead");
					target.CurrState = ModelChar.State.Dead;
				}
			}
			else if (e is ReplayEvtBuff)
			{
				var buff = (ReplayEvtBuff)e;
				ModelChar target = Chars.Find((c) => c.Id == buff.Char);
				ModelBuff buffModel = new ModelBuff();

				buffModel.Stun = buff.Stun;
				buffModel.Cd = buff.TCnt;
				buffModel.Attack = buff.Atk;
				buffModel.Defense = buff.Def;
				buffModel.Speed = buff.Spd;
				target.Buffs.Add(buffModel);
			}
		}

		DelayedEffects.RemoveAll((d) => (d.Tick == tickId));
	}

	public List<ReplayEvt> Simulate()
	{
		// just to be sure we're not in an infinite loop
		const int MAX_TICKS = 100000;
		int tickId = 0;

		foreach (var c in Chars)
		{
			if (c.Team == true) c.Enemies = TeamB;
			else                c.Enemies = TeamA;
		}

		while (IsTeamAliveFunc(TeamA) && IsTeamAliveFunc(TeamB) && tickId < MAX_TICKS)
		{
			tickId++;
			ApplyDelayedEffects(tickId);
			foreach (var c in Chars)
				TickChar(tickId, c);
		}

		EvtAll.Sort((a, b) => { return a.Tick - b.Tick; });
		return EvtAll;
	}

	private void TickChar(int tickId, ModelChar src)
	{
		TickCooldowns(src);
		switch (src.CurrState)
		{
			case ModelChar.State.Idle:   TickIdle(tickId, src);   break;
			case ModelChar.State.Move:   TickMove(tickId, src);   break;
			case ModelChar.State.Attack: TickAttack(tickId, src); break;
		}
	}

	private void TickIdle(int tickId, ModelChar src)
	{
		DecideAction(tickId, src);
	}

	private void TickMove(int tickId, ModelChar src)
	{
		if (src.CdMove <= 0)
		{
			src.Pos = src.PosDest;
			src.CurrState = ModelChar.State.Idle;
		}
	}

	private void TickAttack(int tickId, ModelChar src)
	{
		ModelChar target = src.Enemies.Find((c) => c.Id == src.TargetId);
		if (target == null)
			return;

		int skillsCount = src.Skills.Count;
		for (int i=0; i<skillsCount; i++)
		{
			if (src.Skills[i].Cd <= 0)
			{
				DoSkillAttack(tickId, src, target, i);
				src.Skills[i].Cd = SecsToTicksFunc(src.Skills[i].Info.CooldownSecs);
				return;
			}
		}

		if (src.CdAttack <= 0)
		{
			DoNormalAttack(tickId, src, target);
			return;
		}
	}

	private void DoNormalAttack(int tickId, ModelChar src, ModelChar target)
	{
		ReplayEvtAttack evt = new ReplayEvtAttack();
		evt.Tick = tickId;
		evt.Char = src.Id;
		evt.Targ = target.Id;
		evt.Id   = 0;
		evt.Dir  = GetAttackDir(src, target);

		EvtAll.Add(evt);

		if (target.Hp-src.Damage <= 0)
		{
			src.CurrState = ModelChar.State.Idle;
		}

		// Apply the damage later
		int applyAddTick = (Constants.TicksPerSec / 4);
		ReplayEvtDamage evt2 = new ReplayEvtDamage();
		evt2.Tick = tickId + applyAddTick;
		evt2.Char = target.Id;
		evt2.Dmg  = src.Damage;

		if (Random.Range(0, 100) <= src.CritChance)
		{
			evt2.Dmg = src.Damage * 2;
			evt2.Crit = true;
		}

		evt2.Dmg -= target.Defense;
		if (evt2.Dmg < 1)
		{
			evt2.Dmg = 1; // at least give one damage
		}

		DelayedEffects.Add(evt2);
		EvtAll.Add(evt2);
		src.CdAttack = src.CdAttackFull;
	}

	private void DoSkillAttack(int tickId, ModelChar src, ModelChar target, int skillIndex)
	{
		var skill = src.Skills[skillIndex];
		var info = skill.Info;

		ReplayEvtAttack evt = new ReplayEvtAttack();
		evt.Tick = tickId;
		evt.Char = src.Id;
		evt.Targ = target.Id;
		evt.Id   = skillIndex + 1;
		evt.Dir  = GetAttackDir(src, target);

		EvtAll.Add(evt);

		int applyAddTick = (Constants.TicksPerSec / 4);

		if (info.Heal != 0)
		{
			ReplayEvtDamage evt2 = new ReplayEvtDamage();
			evt2.Tick = tickId + applyAddTick;
			evt2.Char = src.Id;
			evt2.Dmg  = -info.Heal;
			DelayedEffects.Add(evt2);
			EvtAll.Add(evt2);

			//Debug.Log($"HEAL {src.Id} for {info.Heal}");
		}

		if (info.StunSecs != 0)
		{
			ReplayEvtBuff evtBuff = new ReplayEvtBuff();
			evtBuff.Tick = tickId + applyAddTick;
			evtBuff.Char = target.Id;
			evtBuff.TCnt = SecsToTicksFunc(info.StunSecs);
			evtBuff.Stun = true;

			DelayedEffects.Add(evtBuff);
			EvtAll.Add(evtBuff);

			//Debug.Log($"STUN {target.Id} for {evtBuff.TCnt} ticks");
		}
	}

	private AttackDir GetAttackDir(ModelChar src, ModelChar target)
	{
		if      (src.Pos.y < target.Pos.y) return AttackDir.North;
		else if (src.Pos.y > target.Pos.y) return AttackDir.South;
		else if (src.Pos.x < target.Pos.x) return AttackDir.East;
		else                               return AttackDir.West;
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
			evt.Tick          = tickId;
			evt.Char          = src.Id;
			evt.From          = src.Pos;
			evt.To            = src.PosDest;
			evt.NumTicks      = Constants.MoveTicks;

			EvtAll.Add(evt);

			src.Pos       = src.PosDest; // so pathfinding knows
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

	static public int SecsToTicksFunc(float secs)
	{
		return (int) Mathf.Round(secs * (float)Constants.TicksPerSec);
	}


}

}
