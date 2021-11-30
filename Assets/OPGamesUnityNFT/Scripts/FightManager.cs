using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class FightManager : MonoBehaviour
{
	[SerializeField] private List<FightChar> fighters;
	[SerializeField] private UIFight ui;

	[Header("Strings")]
	[SerializeField] private string strFight;
	[SerializeField] private string strWin;
	[SerializeField] private string strLose;

	private List<FightChar> teamA = new List<FightChar>();
	private List<FightChar> teamB = new List<FightChar>();

	private	FightSim sim = new FightSim();
	private Grid grid = null;

	private Replay replayFull = new Replay();
	private List<ReplayEvt> events;
	private List<ReplayEvtBuff> activeBuffs = new List<ReplayEvtBuff>();

	private void Start()
	{
		grid = GameObject.FindObjectOfType<Grid>();

		// Set teams
		foreach (var f in fighters)
		{
			if (f.Team == true) teamA.Add(f);
			else                teamB.Add(f);

			f.SetGrid(grid);
		}

		// set nft and targets
		for (int i=0; i<teamA.Count; i++)
		{
			var f = teamA[i];
			f.SetTargets(teamB);

			if (GameGlobals.Offense.Count > i)
			{
				var info = GameGlobals.Offense[i];
				f.SetNFT(info.Id);
				f.transform.position = grid.GridToWorld(info.Pos);
			}
			else
			{
				f.SetRandomTempNFT();
			}
		}

		for (int i=0; i<teamB.Count; i++)
		{
			var f = teamB[i];

			f.SetTargets(teamA);

			if (GameGlobals.Enemy.Count > i)
			{
				var info = GameGlobals.Enemy[i];
				f.SetNFT(info.Id);
				f.transform.position = grid.GridToWorld(info.Pos);
			}
			else
			{
				f.SetRandomTempNFT();
			}
				
		}

		FightSim.GridObj = grid;
		sim.Init();

		foreach (var f in fighters)
		{
			f.Init();
			sim.AddChar(f);
		}

		events = sim.Simulate();
		if (events == null || events.Count == 0)
			return;

		Debug.Log($"Num Events {events.Count}");
		SaveReplayFull();

		PlayFight();
	}

	private void SaveReplayFull()
	{
		foreach (var f in fighters)
		{
			ReplayChar c = new ReplayChar();

			c.Id = f.Id;
			c.Cont = f.Contract;
			c.Tok = f.TokenId;
			c.Team = f.Team;
			c.Pos = grid.WorldToGrid(f.transform.position);

			replayFull.Chars.Add(c);
		}

		foreach (var e in events)
		{
			if (e is ReplayEvtMove)
			{
				replayFull.Move.Add((ReplayEvtMove) e);
			}
			else if (e is ReplayEvtAttack)
			{
				replayFull.Attack.Add((ReplayEvtAttack) e);
			}
			else if (e is ReplayEvtDamage)
			{
				replayFull.Damage.Add((ReplayEvtDamage) e);
			}
			else if (e is ReplayEvtBuff)
			{
				replayFull.Buff.Add((ReplayEvtBuff) e);
			}
		}

		var json = JsonUtility.ToJson(replayFull);
		//Debug.Log(json);
	}

	public void LoadReplayFromFile()
	{
		TextAsset text = Resources.Load<TextAsset>("Replay");
		if (text == null)
			return;

		Replay replay = JsonUtility.FromJson<Replay>(text.text);
		if (replay == null)
			return;

		StopCoroutine("PlayFightCR");

		events.Clear();
		activeBuffs.Clear();
		foreach (var e in replay.Move)
			events.Add(e);

		foreach (var e in replay.Damage)
			events.Add(e);

		foreach (var e in replay.Attack)
			events.Add(e);

		foreach (var e in replay.Buff)
			events.Add(e);

		events.Sort((a, b) => { return a.Tick - b.Tick; });

		// set chars in place
		foreach (var c in replay.Chars)
		{
			var f = fighters.Find((f) => f.Id == c.Id);
			if (f == null) continue;

			f.SetNFT(c.Cont, c.Tok);
			f.Team = c.Team;
			f.transform.position = grid.GridToWorld(c.Pos);
			f.Init();
			f.Reset();
		}

		StartCoroutine("PlayFightCR");
	}

	public void PlayFight()
	{
		StopCoroutine("PlayFightCR");
		StartCoroutine("PlayFightCR");
	}

	private bool IsTeamAlive(List<FightChar> team)
	{
		foreach (var f in team)
			if (f.IsAlive)
				return true;

		return false;
	}

	private void TickEvtMove(ReplayEvtMove evt)
	{
		// do the event
		FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
		if (fightChar != null)
		{
			fightChar.DoEvtMove(evt);
		}
	}

	private void TickEvtAttack(ReplayEvtAttack evt)
	{
		FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
		if (fightChar != null)
		{
			fightChar.DoEvtAttack(evt);
		}
	}

	private void TickEvtDamage(ReplayEvtDamage evt)
	{
		FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
		if (fightChar != null)
		{
			fightChar.DoEvtDamage(evt);
		}
	}

	private void TickEvtBuff(ReplayEvtBuff evt)
	{
		FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
		if (fightChar != null)
		{
			fightChar.DoEvtBuff(evt);

			evt.TLeft = evt.TCnt;
			activeBuffs.Add(evt);
		}
	}

	private IEnumerator PlayFightCR()
	{
		// reset data first
		foreach (var f in fighters)
		{
			f.Reset();
			f.enabled = true;
		}
		activeBuffs.Clear();

		// Animate start of fight
		yield return new WaitForSeconds(1.0f);
		ui.SetTextAnimationTrigger("Fight", strFight);
		yield return new WaitForSeconds(1.0f);

		// Run the simulation
		int evtIndex = 0;
		int tick = 0;
		var evtCurr = events[evtIndex];
		while (evtCurr != null)
		{
			tick++;
			TickBuffs();
			while (evtCurr != null && evtCurr.Tick == tick)
			{
				//Debug.Log($"{tick} - {evtCurr.ToString()}");
				if (evtCurr is ReplayEvtMove)
				{
					TickEvtMove((ReplayEvtMove) evtCurr);
				}
				else if (evtCurr is ReplayEvtAttack)
				{
					TickEvtAttack((ReplayEvtAttack) evtCurr);
				}
				else if (evtCurr is ReplayEvtDamage)
				{
					TickEvtDamage((ReplayEvtDamage) evtCurr);
				}
				else if (evtCurr is ReplayEvtBuff)
				{
					TickEvtBuff((ReplayEvtBuff) evtCurr);
				}

				evtIndex++;
				if (evtIndex < events.Count)
				{
					evtCurr = events[evtIndex];
				}
				else
				{
					evtCurr = null;
				}
			}
			yield return new WaitForSeconds(Constants.TickDt);
		}

		foreach (var f in fighters)
		{
			f.enabled = false;
		}
		
		// Animate result
		if (IsTeamAlive(teamA))
		{
			ui.SetTextAnimationTrigger("End", strWin);
		}
		else
		{
			ui.SetTextAnimationTrigger("End", strLose);
		}

		yield return new WaitForSeconds(2.0f);
		SceneManager.LoadScene(1);
	}

	private void TickBuffs()
	{
		if (activeBuffs.Count <= 0)
			return;

		for (int i=0; i<activeBuffs.Count; i++)
		{
			ReplayEvtBuff evt = activeBuffs[i];
			evt.TLeft--;
			if (evt.TLeft <= 0)
			{
				FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
				if (fightChar != null)
				{
					fightChar.RemoveEvtBuff(evt);
				}
			}
		}

		activeBuffs.RemoveAll((b) => (b.TLeft <= 0));
	}
}

}
