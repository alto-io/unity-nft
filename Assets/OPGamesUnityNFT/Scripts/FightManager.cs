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
	[SerializeField] private FightChar[] p1;
	[SerializeField] private FightChar[] p2;

	[SerializeField] private TextMeshProUGUI textFight;

    private IEnumerator Start()
    {
		yield return StartCoroutine( p1[0].Attack(p2[0], Random.Range(0.1f, 0.3f)) );
		yield return StartCoroutine( p1[1].Attack(p2[1], Random.Range(0.1f, 0.3f)) );
		yield return StartCoroutine( p1[2].Attack(p2[2], Random.Range(0.1f, 0.3f)) );

		yield return StartCoroutine( p2[0].Attack(p1[0], Random.Range(0.1f, 0.3f)) );
		yield return StartCoroutine( p2[1].Attack(p1[1], Random.Range(0.1f, 0.3f)) );
		yield return StartCoroutine( p2[2].Attack(p1[2], Random.Range(0.1f, 0.3f)) );
    }

}

}
