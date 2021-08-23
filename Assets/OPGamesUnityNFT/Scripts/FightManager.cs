using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class FightManager : MonoBehaviour
{
	[SerializeField] private List<FightChar> fighters;
	[SerializeField] private TextMeshProUGUI textFight;

	private List<FightChar> teamFalse = new List<FightChar>();
	private List<FightChar> teamTrue = new List<FightChar>();

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

    private IEnumerator Start()
    {
		InitLists();

		yield return new WaitForSeconds(1.0f);

		bool isGameOver = false;
		while (!isGameOver)
		{
			FightChar currChar = null;
			FightChar target = null;

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
			yield return StartCoroutine(currChar.Attack(target));

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
			}
		}
    }

}

}
