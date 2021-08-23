using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace OPGames.NFT
{

public class FightChar : MonoBehaviour
{
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar cooldownBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private SpriteRenderer sprite;
	[SerializeField] private bool team;

	private float cooldown = 0;
	private float cooldownLeft = 0;

	private float damage = 0;
	private float hpMax = 0;
	private float hp = 0;

	public bool Team    { get { return team; } }
	public bool IsReady { get { return cooldownLeft <= 0.0f; } }
	public bool IsAlive { get { return hp > 0.0f; } }

	private void Start()
	{
		damage = 20 + (Random.Range(0, 5) * 5);

		cooldown = Random.Range(1.0f, 2.0f);
		cooldownLeft = cooldown;

		hpMax = 100.0f + (Random.Range(1, 5) * 10);
		hp = hpMax;
	}

	public void UpdateCooldown()
	{
		cooldownLeft -= Time.deltaTime;

		float barValue = 1.0f - (cooldownLeft / cooldown);
		cooldownBar.SetValue(barValue, false);
	}

	public IEnumerator Attack(FightChar target)
	{
		Vector3[] waypoints = new Vector3[]
		{
			transform.position,
			transform.position + (transform.up * 2.0f),
			attackPos.position
		};

		var seqAttack = DOTween.Sequence();
		seqAttack.Append(
				transform.DOPath(waypoints, 0.5f, PathType.CatmullRom)
				.SetEase(Ease.InBack));
		seqAttack.Append(transform.DOMove(transform.position, 0.25f));

		yield return new WaitForSeconds(0.5f);
		StartCoroutine(target.OnAttacked(damage));

		yield return seqAttack.WaitForCompletion();
		yield return new WaitForSeconds(0.5f);

		cooldownLeft = cooldown;
	}

	public IEnumerator OnAttacked(float d)
	{
		var seqColor = DOTween.Sequence();
		seqColor.Append(sprite.DOColor(Color.red, 0.1f));
		seqColor.Append(sprite.DOColor(Color.white, 0.1f));
		
		var seqOnAttacked = DOTween.Sequence();
		seqOnAttacked.Append(transform.DOMove(hitPos.position, 0.1f));
		seqOnAttacked.Append(transform.DOMove(transform.position, 0.1f));
		yield return seqOnAttacked.WaitForCompletion();

		hp -= d;
		hpBar.SetValue(hp/hpMax);
	}
}

}
