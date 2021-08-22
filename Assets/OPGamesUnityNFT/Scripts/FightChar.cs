using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace OPGames.NFT
{

public class FightChar : MonoBehaviour
{
	[SerializeField] private HPBar hp;
	[SerializeField] private Transform hitPos;
	[SerializeField] private Transform backPos;
	[SerializeField] private SpriteRenderer sprite;

	private void Start()
	{
		hp = GetComponentInChildren<HPBar>();
		sprite = GetComponentInChildren<SpriteRenderer>();
		hitPos = transform.Find("Hit");
		backPos = transform.Find("Back");
	}

	public IEnumerator Attack(FightChar target, float damage = 0.1f)
	{
		Vector3[] waypoints = new Vector3[]
		{
			transform.position,
			transform.position + (transform.up * 2.0f),
			target.hitPos.position
		};

		var seqAttack = DOTween.Sequence();
		seqAttack.Append(
				transform.DOPath(waypoints, 1.0f, PathType.CatmullRom)
				.SetEase(Ease.InBack));
		seqAttack.Append(transform.DOMove(transform.position, 0.5f));

		yield return new WaitForSeconds(1.0f);
		StartCoroutine(target.OnAttacked(damage));

		yield return seqAttack.WaitForCompletion();
		yield return new WaitForSeconds(0.5f);
	}

	public IEnumerator OnAttacked(float damage)
	{
		var seqColor = DOTween.Sequence();
		seqColor.Append(sprite.DOColor(Color.red, 0.1f));
		seqColor.Append(sprite.DOColor(Color.white, 0.1f));
		
		var seqOnAttacked = DOTween.Sequence();
		seqOnAttacked.Append(transform.DOMove(backPos.position, 0.1f));
		seqOnAttacked.Append(transform.DOMove(transform.position, 0.1f));
		yield return seqOnAttacked.WaitForCompletion();

		hp.Subtract(damage);
	}
}

}
