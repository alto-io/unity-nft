using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class FightManagerOld : MonoBehaviour
{
	private class Step
	{
		public FightCharOld source;
		public float sourceHP;

		public FightCharOld target;
		public float targetHP;

		public float damage;
	}

	[SerializeField] private List<FightCharOld> fighters;
	[SerializeField] private TextMeshProUGUI textFight;

	private List<FightCharOld> teamFalse = new List<FightCharOld>();
	private List<FightCharOld> teamTrue = new List<FightCharOld>();


	private void InitLists()
	{
		foreach (var f in fighters)
		{
			if (f.Team == true)
			{
				teamTrue.Add(f);
			}
			else
			{
				teamFalse.Add(f);
			}
		}
	}

	private IEnumerator TextFightSequence()
	{
		var seq = DOTween.Sequence();
		seq.Append(textFight.transform.DOScale(Vector3.one * 1.2f, 0.5f));
		seq.AppendInterval(0.25f);
		seq.Append(textFight.transform.DOScale(Vector3.zero, 0.5f));

		textFight.gameObject.SetActive(true);
		yield return seq.WaitForCompletion();
		textFight.gameObject.SetActive(false);
	}

	private IEnumerator TextWinSequence(bool team)
	{
		textFight.text = team == false ? "You Win!" : "You Lose!";

		var seq = DOTween.Sequence();
		seq.Append(textFight.transform.DOScale(Vector3.one * 1.2f, 0.5f));
		seq.AppendInterval(0.25f);
		seq.Append(textFight.transform.DOScale(Vector3.zero, 0.5f));

		textFight.gameObject.SetActive(true);
		yield return seq.WaitForCompletion();
	}

	private void ResolveFight()
	{
	}

    private IEnumerator Start()
    {
		InitLists();

		yield return StartCoroutine(TextFightSequence());
		
		bool isGameOver = false;
		while (!isGameOver)
		{
			FightCharOld currChar = null;
			FightCharOld target = null;

			// find ready char
			foreach (var p in fighters)
			{
				if (p.IsReady)
				{
					currChar = p;
					break;
				}
			}

			// if no one is ready, wait for next frame
			if (currChar == null)
			{
				yield return null;
				foreach (var p in fighters)
				{
					p.UpdateCooldown();
				}
				continue;
			}

			// find target if curr char is ready
			int r = Random.Range(0, currChar.Team ? teamFalse.Count : teamTrue.Count);
			target = currChar.Team ? teamFalse[r] : teamTrue[r];

			// attack that shit
			StartCoroutine(currChar.Attack(target));

			// check if target died and remove from lists
			if (target.IsAlive == false)
			{
				fighters.Remove(target);
				if (target.Team)
				{
					teamTrue.Remove(target);
				}
				else
				{
					teamFalse.Remove(target);
				}

				if (teamTrue.Count == 0)
				{
					yield return StartCoroutine(TextWinSequence(true));
				}
				else if (teamFalse.Count == 0)
				{
					yield return StartCoroutine(TextWinSequence(false));
				}
			}
		}
    }

}

}
