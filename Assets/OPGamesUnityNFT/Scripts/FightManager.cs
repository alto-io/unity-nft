using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace OPGames.NFT
{

public class FightManager : MonoBehaviour
{
	[SerializeField] private List<FightChar> fighters;

	private List<FightChar> teamA = new List<FightChar>();
	private List<FightChar> teamB = new List<FightChar>();

	private void Start()
	{
		foreach (var f in fighters)
		{
			if (f.Team == true) teamA.Add(f);
			else                teamB.Add(f);
		}

		foreach (var f in teamA)
			f.SetTargets(teamB);

		foreach (var f in teamB)
			f.SetTargets(teamA);

		StartCoroutine(TickCR());
		
	}

	private IEnumerator TickCR()
	{
		yield return null;
		while (true)
		{
			foreach (var f in fighters)
			{
				f.Tick();
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
}

}
