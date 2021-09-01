using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class FightChar : MonoBehaviour
{
	[SerializeField] private DamageText damageText;
	[SerializeField] private Animator spriteAndUIAnimator;
	[SerializeField] private TextMeshProUGUI textName;
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar cooldownBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private SpriteRenderer sprite;
	[SerializeField] private bool team = true;

	private int hp;
	private int attackRange;
	private int attackSpeed;
	private int moveSpeed;
	private int damage;
	private int defense;
	private int agility;
	private int statMax;

	private float critMaxChance;
	private float critMult;

	private string className = "";
	private string charName = "";

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

	public void SetRandomTempNFT()
	{
		var data = DataNFTSprites.Instance;
		if (data == null)
			return;

		var info = data.GetRandom();
		if (info == null)
			return;

		SetName(info.Name, className);
		SetNFTTexture(info.Texture);
	}

	public void SetNFT(string key)
	{
		var mgr = NFTManager.Instance;
		if (mgr == null)
			return;

		var nft = mgr.GetNFTItemDataById(key);
		if (nft == null)
			return;

		charName = nft.Name;
		textName.text = string.Format("{0}\n({1})", charName, className);

		SetNFTTexture(nft.Texture);
	}

	private void SetName(string _charName, string _className)
	{
		charName = _charName;
		className = _className;
		textName.text = string.Format("{0}\n({1})", charName, className);
	}

	private void SetNFTTexture(Texture2D tex)
	{
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
		var data = DataClasses.Instance;
		DataClasses.Info classInfo = data.GetRandom();
		
		className     = classInfo.Name;
		hp            = classInfo.HP * data.HPMult;
		attackRange   = classInfo.AttackRange;
		attackSpeed   = classInfo.AttackSpeed;
		moveSpeed     = classInfo.MoveSpeed;
		damage        = classInfo.Damage;
		defense       = classInfo.Defense;
		agility       = classInfo.Agility;
		statMax       = data.StatMax;
		critMaxChance = data.CritMaxChance;
		critMult      = data.CritMult;

		if (spriteAndUIAnimator != null)
		{
			spriteAndUIAnimator.Play(0, -1, Random.Range(0.0f, 1.0f));
		}
		hpCurr = hp;
		cooldown = (statMax - attackSpeed) + 1;
		ResetCooldown();
		SetName(charName, className);
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

		FightEvent e = new FightEvent();
		e.source = this;
		e.target = target;
		e.isCrit = CalcIfCrit();

		int damageFinal = damage;

		if (e.isCrit)
			damageFinal = (int)Mathf.Floor((float)damage * critMult);

		e.damage = damageFinal;

		target.hpCurr -= e.damage;

		if (OnEventTriggered != null)
		{
			OnEventTriggered(e);
		}
	}

	private bool CalcIfCrit()
	{
		float critChance = ((float)agility / (float)statMax) * critMaxChance;
		return Random.Range(0.0f, 1.0f) <= critChance;
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

	public IEnumerator AnimAttack(FightEvent evt)
	{
		FightChar target = evt.target;

		Vector3 forwardVector = transform.forward;
		transform.LookAt(target.transform.position);

		Vector3 startPos = transform.position;
		Vector3[] waypoints = new Vector3[]
		{
			transform.position,
			transform.position + (transform.up * 1.0f),
			transform.position + (transform.forward * 2.0f)
		};

		var forwardTween = transform.DOPath(waypoints, 0.2f, PathType.CatmullRom).SetEase(Ease.InBack);
		yield return forwardTween.WaitForCompletion();

		var targetHit = StartCoroutine(target.AnimHit(evt.damage, evt.isCrit));

		// this object go back to position
		transform.forward = forwardVector;
		var back = transform.DOMove(startPos, 0.1f);
		yield return back.WaitForCompletion();

		yield return targetHit;
	}

	private IEnumerator AnimHit(int damage, bool isCrit)
	{
		hpCurr -= damage;
		RefreshHPBar();

		if (damageText != null)
			damageText.ShowDamage(damage, isCrit);
	
		if (isCrit)
			Camera.main.DOShakePosition(0.5f, 0.2f, 30, 45, true);

		// target flashes red
		var seqColor = DOTween.Sequence();
		seqColor.Append(sprite.DOColor(Color.red, 0.1f));
		seqColor.Append(sprite.DOColor(Color.white, 0.1f));

		if (hpCurr <= 0)
			seqColor.Append(sprite.DOColor(Color.gray, 0.1f));

		var seqBack = DOTween.Sequence();

		Vector3 pos = transform.position;
		Vector3 forward = transform.forward;
		seqBack.Append(transform.DOMove(pos - forward, 0.1f));
		seqBack.Append(transform.DOMove(pos, 0.1f));

		yield return seqBack.WaitForCompletion();
	}
}

}
