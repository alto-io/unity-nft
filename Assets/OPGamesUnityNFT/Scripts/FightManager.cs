using UnityEngine;
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

			if (GameGlobals.Selected.Count > i)
				f.SetNFT(GameGlobals.Selected[i]);
			else
				f.SetRandomTempNFT();
		}

		foreach (var f in teamB)
		{
			f.SetTargets(teamA);
			f.SetRandomTempNFT();
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

		PlayFight();
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
		foreach (var f in fighters)
		{
			f.Reset();
			f.enabled = true;
		}

		yield return new WaitForSeconds(1.0f);
		ui.SetTextAnimationTrigger("Fight", strFight);
		yield return new WaitForSeconds(1.0f);

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
		
		if (IsTeamAlive(teamA))
		{
			ui.SetTextAnimationTrigger("End", strWin);
		}
		else
		{
			ui.SetTextAnimationTrigger("End", strLose);
		}
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
				Debug.Log($"Remove stun from {evt.Char}");

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
