using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OPGames.NFT
{

public enum ReplayEventType
{
	Null = 0,
	Move,
	Attack,
	Skill,
}

// Replay event
public class ReplayEvent
{
	public ReplayEventType Type;
	public int Tick;
	public int Src;
	public int Targ;
	public int Dmg;
	public string Skl;
}

public class ReplayEvt
{
	public int Tick;
}

public class ReplayEvtMove : ReplayEvt
{
	public int Char;
	public Vector2Int From;
	public Vector2Int To;

	public override string ToString()
	{
		return string.Format("EvtMove: {0} from:{1}, to:{2}", Char, From.ToString(), To.ToString());
	}
}

public class ReplayEvtAttack : ReplayEvt
{
	public int Char;
	public int Targ;
	public int Id; // attack Id: 0=normal, 1=skill1, 2=skill2, etc

	public override string ToString()
	{
		return string.Format("EvtAttack: {0} target:{1}", Char, Targ);
	}
}

public class ReplayEvtDamage : ReplayEvt
{
	public int Char;
	public int Dmg;
	public bool Crit;
}

public class ReplayEvtBuff : ReplayEvt
{
	public int Char;
	public int TCnt; // tick count
	public int Dmg;  // +/- Damage
	public int Atk;  // +/- Attack
	public int Spd;  // +/- Speed
	public bool Stun;
}

public class ReplayEvtHeal : ReplayEvt
{
	public int Char;
	public int Heal;
	public bool Crit;
}

public class ReplayChar
{
	public string Cont;
	public string Tok;
	public bool Team;
	public Vector2Int Pos;
}

public class Replay
{
	public int Seed;
	public ReplayChar[] Chars;
	public ReplayEvent[] Events;
}

public class ModelBuff
{
	public int Attack;
	public int Defense;
	public int Speed;
}


public class ModelChar
{
	public enum State
	{
		Idle,
		Move,
		Attack,
		Skill,
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

	public List<int> Skills;
	public List<int> CdSkill;
	public List<ModelBuff> Buffs;
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
