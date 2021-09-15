using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

[System.Serializable]
public class FightEventDND
{
	public FightCharDND source;
	public FightCharDND target;
	public int damage = 0;
	public bool isCrit = false;

	public override string ToString()
	{
		return string.Format("source: {0}, target: {1}, damage: {2}", 
				source.name, target.name, damage);
	}
}

public class FightManagerDND : MonoBehaviour
{
	[SerializeField] private List<FightCharDND> fighters;
	[SerializeField] private TextMeshProUGUI textFight;

	private List<FightCharDND> teamA = new List<FightCharDND>();
	private List<FightCharDND> teamB = new List<FightCharDND>();
	public List<FightEventDND> events = new List<FightEventDND>();

	private IEnumerator Start()
	{
		ComputeInitiative();

		// Set teams
		foreach (var f in fighters)
		{
			if (f.Team == true) teamA.Add(f);
			else                teamB.Add(f);

			f.OnEventTriggered += OnEventTriggered;
		}

		yield return null;

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

		yield return null;
		yield return null;

		// Run the battle simulaton
		bool teamAAlive = true;
		bool teamBAlive = true;

		while (teamAAlive && teamBAlive)
		{
			// 1 round
			foreach (var f in fighters)
			{
				if (f.IsAlive == false)
					continue;

				f.TickAttack();
			}

			teamAAlive = IsTeamAlive(teamA);
			teamBAlive = IsTeamAlive(teamB);
		}

		// Reset everything
		foreach (var f in fighters)
		{
			f.Reset();
		}

		StartCoroutine(PlayFightCR());
	}

	private bool IsTeamAlive(List<FightCharDND> team)
	{
		foreach (var f in team)
			if (f.IsAlive)
				return true;

		return false;
	}

	private IEnumerator PlayFightCR()
	{
		yield return StartCoroutine(TextFightSequence());

		int len = events.Count;
		for (int i=0; i<len; i++)
		{
			FightEventDND e = events[i];
			yield return StartCoroutine(e.source.AnimAttack(e));
		}

		if (IsTeamAlive(teamA))
		{
			yield return StartCoroutine(TextWinSequence(true));
		}
		else
		{
			yield return StartCoroutine(TextWinSequence(false));
		}
	}


	private IEnumerator TextFightSequence()
	{
		yield return new WaitForSeconds(1.0f);

		var seq = DOTween.Sequence();
		seq.Append(textFight.transform.DOScale(Vector3.one * 1.2f, 0.1f));
		seq.AppendInterval(0.25f);
		seq.Append(textFight.transform.DOScale(Vector3.zero, 0.2f));

		textFight.gameObject.SetActive(true);
		yield return seq.WaitForCompletion();
		textFight.gameObject.SetActive(false);
	}

	private IEnumerator TextWinSequence(bool team)
	{
		textFight.text = team == true ? "You Win!" : "You Lose!";

		var seq = DOTween.Sequence();
		seq.Append(textFight.transform.DOScale(Vector3.one * 1.2f, 0.1f));
		seq.AppendInterval(0.25f);
		seq.Append(textFight.transform.DOScale(Vector3.zero, 0.2f));

		textFight.gameObject.SetActive(true);
		yield return seq.WaitForCompletion();
	}

	private void ComputeInitiative()
	{
		foreach (FightCharDND f in fighters)
		{
			f.Initiative = Random.Range(1,21);
		}

		fighters.Sort((a, b) => { return b.Initiative - a.Initiative; } );
	}

	private void OnEventTriggered(FightEventDND e)
	{
		//Debug.LogFormat("Add event {0}", e);
		events.Add(e);
	}
}

}
