using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class DamageTextGroup : MonoBehaviour
{
	private DamageText[] texts;
	private int nextIndex = 0;

	private void Start()
	{
		texts = GetComponentsInChildren<DamageText>(true);
	}

	public void ShowDamage(int damage, bool isCrit)
	{
		if (texts == null)
			return;

		var t = texts[nextIndex];
		t.ShowDamage(damage, isCrit);

		nextIndex = (nextIndex + 1) % texts.Length;
	}
}

}
