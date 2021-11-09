using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OPGames.NFT
{

public enum ReplayEventType
{
	Null   = 0,
	Move   = 1,
	Attack = 2,
	Skill  = 3,
}

public enum AttackDir
{
	North = 0,
	South = 1,
	East  = 2,
	West  = 3,
}

public enum BuffType
{
	Null = 0,
	Stun = 1,
	Atk  = 2,
	Def  = 3,
	Spd  = 4,
}

[System.Serializable]
public class ReplayEvt
{
	public int Tick;
}

[System.Serializable]
public class ReplayEvtMove : ReplayEvt
{
	public int Char;
	public Vector2Int From;
	public Vector2Int To;
	public int NumTicks;

	public override string ToString()
	{
		return string.Format("EvtMove: {0} from:{1}, to:{2}", Char, From.ToString(), To.ToString());
	}
}

[System.Serializable]
public class ReplayEvtAttack : ReplayEvt
{
	public int Char;
	public int Targ;
	public int Id; // attack Id: 0=normal, 1=skill1, 2=skill2, etc
	public AttackDir Dir;

	public override string ToString()
	{
		return string.Format("EvtAttack: {0} target:{1}", Char, Targ);
	}
}

[System.Serializable]
public class ReplayEvtDamage : ReplayEvt
{
	public int Char;
	public int Dmg;
	public bool Crit;

	public override string ToString()
	{
		return string.Format("EvtDamage: {0} damage:{1}", Char, Dmg);
	}
}

[System.Serializable]
public class ReplayEvtBuff : ReplayEvt
{
	public int Char;
	public int TCnt; // tick count
	public int TLeft;// temp value used by FightManager
	public int Atk;  // +/- Attack
	public int Def;  // +/- Attack
	public int Spd;  // +/- Speed
	public bool Stun;
}

[System.Serializable]
public class ReplayChar
{
	public int Id;
	public string Cont;
	public string Tok;
	public bool Team;
	public Vector2Int Pos;
}

[System.Serializable]
public class Replay
{
	public int Seed;
	public List<ReplayChar>      Chars  = new List<ReplayChar>();
	public List<ReplayEvtMove>   Move   = new List<ReplayEvtMove>();
	public List<ReplayEvtDamage> Damage = new List<ReplayEvtDamage>();
	public List<ReplayEvtAttack> Attack = new List<ReplayEvtAttack>();
	public List<ReplayEvtBuff>   Buff   = new List<ReplayEvtBuff>();
}

// Use this for both Buff and Debuff
public class ModelBuff
{
	public int Attack;
	public int Defense;
	public int Speed;
	public bool Stun;

	public int Cd;
}

public class ModelSkill
{
	public DataSkills.SkillsRow Info;
	public int Cd;
}

public class ModelChar
{
	public enum State
	{
		Idle,
		Move,
		Attack,
		Skill,
		Dead,
	}

	public int Id;
	public bool Team;
	public Vector2Int Pos;
	public Vector2Int PosDest;
	public int Hp;
	public int CdAttack;
	public int CdAttackFull;
	public int CdMove;
	public int Damage;
	public int Defense;
	public int CritChance; // in percent 50 = 50%
	public State CurrState;
	public int TargetId;

	public DataClasses.ClassStatsRow ClassInfo;

	public List<ModelSkill> Skills = new List<ModelSkill>();
	public List<ModelBuff> Buffs   = new List<ModelBuff>();
	public List<ModelChar> Enemies; // will assign a ref here
}


/* How to implement fight resolution?
 * Try to code this such that its easy to generate the events server side
 *
 * 10 ticks per second
 * cooldown is int
 * each char has initiative who will go first
 *
 * Generate random numbers as needed. Ref: Pseudo Random Number Generator
 * InitChars();
 * while (teamA.Alive() && teamB.Alive)
 * {
 * 		foreach char
 * 		{
 * 			if (char.target == null)
 * 			{
 * 				find target
 * 			}
 *
 * 			if (char.target in range)
 * 			{
 * 				if (skill cooldown ready)
 * 				{
 * 					do skill
 * 					apply effects of skill
 * 				}
 * 				else
 * 				{
 * 					do regular attack
 * 				}
 *
 * 				if (target is dead)
 * 					char.target = null
 * 			}
 * 			esle
 * 			{
 * 				get path to target
 * 				move towards target
 * 			}
 * 		}
 * }
 *
 *
 */

// Replay events
//
// Move
// Do attack
// Apply damage
// Do skill
// Apply buff/debuff
// Apply heal
//
// This way, we can use JsonUtility
// Json
// {
// 		Players: [ ... ],
// 		EvtMove: [ ... ],
// 		EvtAtk: [ ... ],
// 		EvtDmg: [ ... ],
// 		EvtHeal: [ ... ],
// 		EvtBuff: [ ... ],
// }
//
// Just check each event array each tick when something needs to execute




}
