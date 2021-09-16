using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class FightCharDND : MonoBehaviour
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

	private bool isMelee;

	private float critMaxChance;
	private float critMult;

	private string className = "";
	private string charName = "";

	private int cooldownCurr = 0;
	private int cooldown = 0;
	private int hpCurr = 0;

	public bool Team { get { return team; } }
	public bool IsAlive { get { return hpCurr > 0; } }
	public int HP { get { return hp; } }

	public int Initiative;

	private List<FightCharDND> targets;
	public System.Action<FightEventDND> OnEventTriggered;

	public void SetTargets(List<FightCharDND> t)
	{
		targets = t;
	}

	public void Reset()
	{
		hpCurr = hp;
		cooldownCurr = cooldown;
	}

	public void SetRandomTempNFT()
	{
		var data = DataNFTTemp.Instance;
		if (data == null)
			return;

		var info = data.GetRandom();
		if (info == null)
			return;

		SetClass(info.CharClass);
		RefreshName();

		if (info.Spr != null) SetNFTSprite(info.Spr);
		else                  SetNFTTexture(info.Texture);
	}

	public void SetNFT(string key)
	{
		var mgr = NFTManager.Instance;
		if (mgr == null)
			return;

		var nft = mgr.GetNFTItemDataById(key);
		if (nft == null)
			return;

		SetClass(nft.CharClass);
		charName = nft.Name;
		RefreshName();

		if (nft.Spr != null) SetNFTSprite(nft.Spr);
		else                 SetNFTTexture(nft.Texture);
	}

	private void RefreshName()
	{
		textName.text = string.Format("{0}\n{1} {2}", charName, Initiative, className);
	}

	private void SetNFTSprite(Sprite spr)
	{
		sprite.sprite = spr;
	}

	private void SetNFTTexture(Texture2D tex)
	{
		if (tex != null)
		{
			int max = Mathf.Max(tex.width, tex.height);
			float scale = (float)max / 256.0f;

			Sprite spr = Sprite.Create(tex, 
					new Rect(0.0f, 0.0f, tex.width, tex.height), 
					new UnityEngine.Vector2(0.5f, 0.5f),
					100 * scale);

			sprite.sprite = spr;
		}
	}

	private void Start()
	{
		var data = DataClasses.Instance;
		SetClassRandom();

		if (spriteAndUIAnimator != null)
		{
			spriteAndUIAnimator.Play(0, -1, Random.Range(0.0f, 1.0f));
		}
		ResetCooldown();
		RefreshName();
	}

	private void SetClassRandom()
	{
		var dataClass = DataClasses.Instance;
		if (dataClass == null)
			return;

		SetClassStats(dataClass.GetRandom());
	}

	private void SetClass(string className)
	{
		if (string.IsNullOrEmpty(className))
			return;

		var dataClass = DataClasses.Instance;
		if (dataClass == null)
			return;

		var classInfo = dataClass.GetByName(className);
		SetClassStats(classInfo);
	}

	private void SetClassStats(DataClasses.Info classInfo)
	{
		var data = DataClasses.Instance;
		if (data == null)
			return;

		if (classInfo == null)
			return;

		className     = classInfo.Name;
		hp            = classInfo.HP;
		attackRange   = classInfo.AttackRange;
		attackSpeed   = classInfo.AttackSpeed;
		moveSpeed     = classInfo.MoveSpeed;
		damage        = 1;
		defense       = classInfo.Defense;
		agility       = classInfo.Agility;
		isMelee       = classInfo.IsMelee;
		statMax       = data.StatMax;
		critMaxChance = data.CritMaxChance;
		critMult      = data.CritMult;

		hpCurr        = hp;
		cooldown      = (statMax - attackSpeed) + 1;
	}

	private void ResetCooldown()
	{
		cooldownCurr += cooldown;
	}

	public void TickAttack()
	{
		FightCharDND target = null;

		// Pick a target
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

		int rollAttack = Random.Range(1,21);
		int rollDefend = Random.Range(1,21);

		FightEventDND e = new FightEventDND();
		e.source = this;
		e.target = target;
		e.damage = damage;

		if (rollAttack == 20)
		{
			e.isCrit = true;
			e.damage = damage * 2;
		}
		//else if (rollAttack+damage < rollDefend+target.defense)
		//{
		//	e.damage = 0;
		//}

		target.hpCurr -= e.damage;

		if (OnEventTriggered != null)
		{
			OnEventTriggered(e);
		}
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

	public IEnumerator AnimAttack(FightEventDND evt)
	{
		const float TIME = 1.0f;

		FightCharDND target = evt.target;

		Vector3 startPos = transform.position;
		Vector3 endPos   = startPos;
		Vector3 dir      = target.transform.position - startPos;

		float forwardDuration = TIME * 0.2f;

		if (isMelee)
		{
			float travelMagnitude = dir.magnitude - 0.25f;
			endPos = startPos + (dir.normalized * travelMagnitude);
			forwardDuration = TIME * 0.4f;
		}
		else
		{
			endPos = startPos + (dir.normalized * 0.2f);
		}

		Vector3[] waypoints = new Vector3[]
		{
			startPos,
			startPos - (dir.normalized * 0.15f),
			endPos
		};

		var forwardTween = transform.DOPath(waypoints, forwardDuration, PathType.CatmullRom).SetEase(Ease.InBack);
		yield return forwardTween.WaitForCompletion();

		if (isMelee == false)
		{
			var info = DataVFX.Instance.GetByName("VFXFireball");
			if (info != null && info.Prefab != null)
			{
				GameObject clone = Instantiate(info.Prefab);
				clone.transform.position = endPos;
				var projectile = clone.transform.DOMove(target.transform.position, TIME * 0.1f)
					.OnComplete(()=> Destroy(clone));

				yield return new WaitForSeconds((TIME * 0.1f) - 0.05f);
			}
		}

		var targetHit = StartCoroutine(target.AnimHit(evt.damage, evt.isCrit, this));

		// this object go back to position
		var back = transform.DOMove(startPos, TIME * 0.1f);
		yield return back.WaitForCompletion();
		yield return targetHit;
	}

	private IEnumerator AnimHit(int damage, bool isCrit, FightCharDND attacker)
	{
		const float TIME = 0.5f;

		hpCurr -= damage;
		RefreshHPBar();

		if (damageText != null)
			damageText.ShowDamage(damage, isCrit);
	
		if (isCrit)
			Camera.main.DOShakePosition(0.5f, 0.2f, 30, 45, true);

		// target flashes red
		var seqColor = DOTween.Sequence();
		seqColor.Append(sprite.DOColor(Color.red, TIME * 0.3f));
		seqColor.Append(sprite.DOColor(Color.white, TIME * 0.1f));

		if (hpCurr <= 0)
			seqColor.Append(sprite.DOColor(Color.gray, TIME * 0.1f));

		var seqBack = DOTween.Sequence();

		Vector3 pos = transform.position;
		Vector3 back = pos;

		if (attacker.transform.position.y < pos.y)
		{
			back.y += 0.1f;
		}
		else
		{
			back.y -= 0.1f;
		}
		seqBack.Append(transform.DOMove(back, TIME * 0.3f).SetEase(Ease.OutBounce));
		seqBack.Append(transform.DOMove(pos, TIME * 0.1f));

//		var seqScale = DOTween.Sequence();
//		seqScale.Append(transform.DOScale(0.8f, TIME * 0.3f));
//		seqScale.Append(transform.DOScale(1.0f, TIME * 0.1f));

		yield return seqBack.WaitForCompletion();
	}
}

}
