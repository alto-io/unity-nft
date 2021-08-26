using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace OPGames.NFT
{

public class FightChar : MonoBehaviour
{
	[SerializeField] private Text textName;
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar cooldownBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private SpriteRenderer sprite;

	[SerializeField] [Range(100, 500)] private int hp;
	[SerializeField] [Range(1, 10)] private int attackRange;
	[SerializeField] [Range(1, 10)] private int attackSpeed;
	[SerializeField] [Range(1, 10)] private int moveSpeed;
	[SerializeField] [Range(1, 50)] private int damage;
	[SerializeField] [Range(1, 10)] private int defense;

	[SerializeField] private bool team = true;

	private int cooldownCurr = 0;
	private int cooldown = 0;
	private int hpCurr = 0;

	public bool Team { get { return team; } }
	public bool IsAlive { get { return hpCurr > 0; } }
	public bool IsReady { get { return cooldownCurr <= 0; } }
	public int HP { get { return hp; } }

	public System.Action<FightEvent> OnEventTriggered;

	private List<FightChar> targets;

	public void SetTargets(List<FightChar> t)
	{
		targets = t;
	}

	public void Reset()
	{
		hpCurr = hp;
		cooldownCurr = cooldown;
	}

	public void Tick()
	{
		if (IsAlive == false)
			return;

		cooldownCurr--;
		TickAttack();
	}

	public void SetNFT(string key)
	{
		var mgr = NFTManager.Instance;
		if (mgr == null)
			return;

		var nft = mgr.GetNFTItemDataById(key);
		if (nft == null)
			return;

		textName.text = nft.Name;

		var tex = nft.Texture;
		if (tex == null)
			return;

		int max = Mathf.Max(tex.width, tex.height);
		float scale = (float)max / 256.0f;

		Sprite spr = Sprite.Create(tex, 
				new Rect(0.0f, 0.0f, tex.width, tex.height), 
				new UnityEngine.Vector2(0.5f, 0.5f),
				100 * scale);

		sprite.sprite = spr;

	}

	private void Start()
	{
		hpCurr = hp;
		cooldown = (10 - attackSpeed) + 1;
		ResetCooldown();
	}

	private void ResetCooldown()
	{
		cooldownCurr += cooldown;
	}

	private void TickAttack()
	{
		if (IsReady == false) return;

		FightChar target = null;

		int r = Random.Range(0, targets.Count);
		for (int i=0; i<targets.Count; i++)
		{
			int index = (r+i) % targets.Count;
			if (targets[index].IsAlive)
			{
				target = targets[index];
				break;
			}
		}

		if (target == null) return;

		ResetCooldown();
		target.OnAttacked(damage);

		if (OnEventTriggered != null)
		{
			FightEvent e = new FightEvent();
			e.source = this;
			e.target = target;
			e.damage = damage;

			OnEventTriggered(e);
		}
	}

	private void OnAttacked(int damage)
	{
		hpCurr -= damage;
	}

	private void RefreshHPBar()
	{
		float v = (float)hpCurr / (float)hp;
		hpBar.SetValue(v);
	}

	private void RefreshCooldownBar()
	{
		float v = (float)cooldownCurr / (float)cooldown;
		cooldownBar.SetValue(1.0f - v);
	}

	public IEnumerator AnimAttack(FightChar target, int damage)
	{
		Vector3 startPos = transform.position;
		Vector3[] waypoints = new Vector3[]
		{
			transform.position,
			transform.position + (transform.up * 1.0f),
			transform.position + (transform.forward * 2.0f)
		};

		var forward = transform.DOPath(waypoints, 0.2f, PathType.CatmullRom).SetEase(Ease.InBack);
		yield return forward.WaitForCompletion();

		target.hpCurr -= damage;
		target.RefreshHPBar();

		// this object go back to position
		var back = transform.DOMove(startPos, 0.1f);
		//yield return back.WaitForCompletion();

		// target flashes red
		var seqColor = DOTween.Sequence();
		seqColor.Append(target.sprite.DOColor(Color.red, 0.1f));
		seqColor.Append(target.sprite.DOColor(Color.white, 0.1f));

		if (target.hpCurr <= 0)
		{
			seqColor.Append(target.sprite.DOColor(Color.gray, 0.1f));
		}
	}
}

}
