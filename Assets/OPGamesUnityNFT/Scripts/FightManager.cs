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

		foreach (var f in fighters)
		{
			f.Init();
		}

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

	private IEnumerator PlayFightCR()
	{
		yield return new WaitForSeconds(1.0f);
		ui.SetTextAnimationTrigger("Fight", strFight);
		yield return new WaitForSeconds(1.0f);

		foreach (var f in fighters)
		{
			f.enabled = true;
		}

		while (IsTeamAlive(teamA) && IsTeamAlive(teamB))
		{
			yield return new WaitForSeconds(1.0f);
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
