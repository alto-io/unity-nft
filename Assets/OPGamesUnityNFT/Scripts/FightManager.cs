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

	private void Start()
	{
		// Set teams
		foreach (var f in fighters)
		{
			if (f.Team == true) teamA.Add(f);
			else                teamB.Add(f);
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

		grid = GameObject.FindObjectOfType<Grid>();
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

//		yield return null;
//		yield return null;
//
//		// Run the battle simulaton
//		bool teamAAlive = true;
//		bool teamBAlive = true;
//
//		while (teamAAlive && teamBAlive)
//		{
//			foreach (var f in fighters)
//			{
//			}
//
//			teamAAlive = IsTeamAlive(teamA);
//			teamBAlive = IsTeamAlive(teamB);
//		}
//		
//		// Reset everything
//		foreach (var f in fighters)
//		{
//			f.Reset();
//		}
//
		StartCoroutine(PlayFightCR());
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
		fightChar.transform.position = grid.GridToWorld(evt.To);
	}

	private void TickEvtAttack(ReplayEvtAttack evt)
	{
		// do attack animation
	}

	private void TickEvtDamage(ReplayEvtDamage evt)
	{
		FightChar fightChar = fighters.Find((x) => (x.Id == evt.Char));
		fightChar.HP -= evt.Dmg;

		if (fightChar.HP <= 0)
		{
			fightChar.animator.SetTrigger("Dead");
		}
	}

	private IEnumerator PlayFightCR()
	{
		yield return new WaitForSeconds(1.0f);
		ui.SetTextAnimationTrigger("Fight", strFight);
		yield return new WaitForSeconds(1.0f);

		foreach (var f in fighters)
		{
			f.enabled = true;
		}

		int evtIndex = 0;
		int tick = 0;
		var evtCurr = events[evtIndex];
		while (evtCurr != null)
		{
			tick++;
			while (evtCurr != null && evtCurr.Tick == tick)
			{
				Debug.Log($"{tick} - {evtCurr.ToString()}");
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
			yield return new WaitForSeconds(0.02f);
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
}

}
