using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OPGames.NFT
{


// Simulate the whole fight and generate events as they happen
public class FightSim
{
	static public Grid grid;

	public List<ModelChar>       Chars     = new List<ModelChar>();
	public List<ModelChar>       TeamA     = new List<ModelChar>();
	public List<ModelChar>       TeamB     = new List<ModelChar>();

	public List<ReplayEvtMove>   EvtMove   = new List<ReplayEvtMove>();
	public List<ReplayEvtAttack> EvtAttack = new List<ReplayEvtAttack>();
	public List<ReplayEvtDamage> EvtDamage = new List<ReplayEvtDamage>();
	public List<ReplayEvtBuff>   EvtBuff   = new List<ReplayEvtBuff>();

	public void AddChar(FightChar c)
	{
		var     info    = c.ClassInfo;
		Vector3 p       = c.transform.position;

		ModelChar model = new ModelChar();
		model.Id        = c.Id;
		model.Team      = c.Team;
		model.Pos       = grid.WorldToGrid(p);
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
		while (IsTeamAlive(TeamA) && IsTeamAlive(TeamB))
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

}

}
