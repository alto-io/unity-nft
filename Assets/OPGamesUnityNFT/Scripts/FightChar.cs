using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

// TODO: This class is so effin huge!
public class FightChar : MonoBehaviour
{
	private static int nextId = 1;

#region Internal types
	public enum State
	{
		Idle,
		Move,
		Attack,
	};

	private class PosDistance
	{
		public Vector3 pos;
		public float distance;
	}
#endregion

#region Public Properties
	public int Id       { get { return id; } }
	public bool Team    { get { return team; } set { team = value; } }
	public bool IsAlive { get { return hpCurr > 0; } }
	public bool IsReady { get { return cooldownACurr <= 0; } }
	public float HP     { get { return hpCurr; } set { hpCurr = value; RefreshHPBar(); } }

	public string Contract { get; private set; }
	public string TokenId { get; private set; }

	public DataClasses.ClassStatsRow ClassInfo { get { return classInfo; } }
#endregion

#region Exposed Variables
	[SerializeField] private DamageTextGroup damageText;
	[SerializeField] private Animator spriteAndUIAnimator;
	[SerializeField] private TextMeshProUGUI textName;
	[SerializeField] private TextMeshProUGUI textStatus;
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar skillBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private Transform view;
	[SerializeField] private SpriteRenderer sprite;
	[SerializeField] private bool team = true;
	[SerializeField] private Ease moveEaseType;
#endregion

#region Private variables

	private int id = 0;
	private bool isMelee;
	private float hpCurr = 0;
	private float stunDuration = 0;
	private float critChance;
	private string charName = "";
	private string vfxAttack = "";

	private DataClasses.ClassStatsRow classInfo;

	// cooldown attack
	private float cooldownACurr = 0;
	private float cooldownA = 0;

	// cooldown skill
	private float cooldownSCurr = 0;
	private float cooldownS = 0;

	public Animator animator;

	private List<FightChar> targets;
	private FightChar targetCurr = null;
	private State stateCurr = State.Idle;
	private const float moveDuration = 0.1f;

	private Vector3 initialPos;
	private Vector3 moveStart;
	private Vector3 moveEnd;
	private Grid    grid;

	private List<DataSkills.SkillsRow> skills = new List<DataSkills.SkillsRow>();
	private List<PosDistance> neighbors = new List<PosDistance>();
	private List<BuffType> buffs = new List<BuffType>();

#endregion

#region Public Methods

	public void SetGrid(Grid g)
	{
		grid = g;
	}

	public void SetTargets(List<FightChar> t)
	{
		targets = t;
	}

	public void SetRandomTempNFT()
	{
		var data = DataNFTTemp.Instance;
		if (data == null)
			return;

		SetNFTTemp(data.GetRandom());
	}

	public void SetNFT(string key)
	{
		//Debug.LogFormat("SetNFT {0}", key);
		var mgr = NFTManager.Instance;
		if (mgr == null)
			return;

		var nft = mgr.GetNFTItemDataById(key);
		if (nft == null)
			return;

		if (string.IsNullOrEmpty(nft.CharClass))
		{
			SetClassRandom();
		}
		else
		{
			SetClass(nft.CharClass);
		}
		charName = nft.Name;
		TokenId = nft.TokenId;
		Contract = nft.Contract;

		RefreshName();

		//if (nft.Spr != null) SetNFTSprite(nft.Spr);
		//else                 SetNFTTexture(nft.Texture);
		SetNFTTexture(nft.Texture);
	}

	public void SetNFT(string contract, string tokenId)
	{
		// actually have to change this anyway
		if (contract == "ArcadiansTemp")
		{
			var data = DataNFTTemp.Instance;
			var info = data.GetByTokenId(tokenId);
			SetNFTTemp(info);
		}
		else
		{
		}
	}

	private void SetNFTTemp(DataNFTTemp.Info info)
	{
		if (info == null)
			return;

		TokenId = info.TokenId;
		Contract = "ArcadiansTemp";

		SetClass(info.CharClass);
		RefreshName();

		//if (info.Spr != null) SetNFTSprite(info.Spr);
		//else                  SetNFTTexture(info.Texture);
		SetNFTTexture(info.Texture);
	}

	public void Init()
	{
		if (id == 0) 
		{
			id = nextId;
			nextId++;
		}

		initialPos = transform.position;
		animator = GetComponent<Animator>();

		if (spriteAndUIAnimator != null)
		{
			spriteAndUIAnimator.Play(0, -1, Random.Range(0.0f, 1.0f));
		}
		RefreshName();
		enabled = false;

		for (int i=0; i<8; i++)
		{
			neighbors.Add(new PosDistance());
		}

		damageText.Reset();
	}

	public void Reset()
	{
		damageText.Reset();
		textStatus.text = "";
		transform.position = initialPos;
		gameObject.SetActive(true);
		hpCurr = classInfo.HpVal;
		RefreshHPBar();
		RefreshSkillBar();
		cooldownACurr = cooldownA;
		animator.Play("FightCharIdle");
	}

	// Triggered by animation of projectile
	public void OnAttackHitRanged()
	{
		if (!isMelee) OnAttackHit();
	}

	// Triggered by animation clip
	public void OnAttackHitMelee()
	{
		if (isMelee) OnAttackHit();
	}

	// When this character is hit
	public void OnAttackHit()
	{
		if (targetCurr == null)
			return;

		bool isCrit = CalcIfCrit();
		float damageFinal = classInfo.DamageVal;
		if (isCrit)
			damageFinal *= 2;

		bool isShowDamageText = true;
		if (cooldownSCurr <= 0 && skills.Count > 0)
		{
			// use skill instead
			int r = Random.Range(0, skills.Count);
			var currSkill = skills[r];
			if (currSkill != null)
			{
				if (currSkill.StunSecs > 0)
				{
					targetCurr.stunDuration = currSkill.StunSecs;
					targetCurr.damageText.ShowMsg("Stun");
					isShowDamageText = false;
				}

				if (currSkill.Heal > 0)
				{
					hpCurr += currSkill.Heal;
					damageText.ShowMsg("Heal");
					RefreshHPBar();
					isShowDamageText = false;
				}

				damageFinal = currSkill.Damage;
				if (isCrit)
					damageFinal *= 2;
			}

			cooldownSCurr = cooldownS;
		}

		if (damageFinal > 0)
		{
			if (isShowDamageText)
			{
				targetCurr.damageText.ShowDamage((int)Mathf.Round(damageFinal), isCrit);
			}

			targetCurr.hpCurr -= damageFinal;
			targetCurr.RefreshHPBar();

			if (targetCurr.IsAlive == false)
			{
				cooldownACurr = 0.1f;
				stateCurr = State.Idle;
				grid.ClearOccupied(targetCurr.transform.position);
				targetCurr.animator.SetTrigger("Dead");
			}
			else
			{
				targetCurr.animator.SetTrigger("Hurt");
			}

			if (isCrit)
				Camera.main.DOShakePosition(0.5f, 0.1f, 30, 45, true);
		}
	}

	public void OnAttackEnd()
	{
		ResetCooldown();
	}

	public void OnDeadEnd()
	{
		gameObject.SetActive(false);
	}

	public void AnimationTrigger(string t)
	{
		animator.SetTrigger(t);
	}

	public void DoEvtMove(ReplayEvtMove evt)
	{
		const float shortenBySecs = 0.1f;
		Vector3 posStart = grid.GridToWorld(evt.From);
		Vector3 posEnd = grid.GridToWorld(evt.To);
		float duration = ((float)evt.NumTicks / (float)Constants.TicksPerSec) - shortenBySecs;

		transform.position = posStart;
		transform.DOMove(posEnd, duration).SetEase(moveEaseType);
	}

	public void DoEvtAttack(ReplayEvtAttack evt)
	{
		AnimationTrigger("Attack" + evt.Dir.ToString());

		var t = targets.Find((c) => (c.Id == evt.Targ));

		var info = DataVFX.Instance.GetByName(vfxAttack);
		if (info != null && info.Prefab != null)
		{
			GameObject clone = Instantiate(info.Prefab);
			var position = t.transform.position;
			position.z -= 0.4f;
			position.y -= 0.3f;
			//clone.transform.LookAt(t.transform.position);

			if (isMelee)
			{
				// don't move, just use dotween to trigger destroy *hack*
				clone.transform.position = position;
				clone.transform.DOMove(position, 1.0f)
					.OnComplete(()=> { Destroy(clone); });
			}
			else
			{
				clone.transform.position = transform.position;
				clone.transform.LookAt(t.transform.position);
				clone.transform.DOMove(position, 0.5f)
					.OnComplete(()=> { Destroy(clone); });
			}
		}
	}

	public void DoEvtDamage(ReplayEvtDamage evt)
	{
		HP -= evt.Dmg;
		damageText.ShowDamage(evt.Dmg, evt.Crit);

		if (HP <= 0)
			AnimationTrigger("Dead");
		else
			AnimationTrigger("Hurt");
	}

	public void DoEvtBuff(ReplayEvtBuff evt)
	{
		if (evt.Stun == true)
		{
			damageText.ShowMsg("Stun");
			buffs.Add(BuffType.Stun);
		}
		RefreshStatus();
	}

	public void RemoveEvtBuff(ReplayEvtBuff evt)
	{
		if (evt.Stun == true)
		{
			buffs.Remove(BuffType.Stun);
		}
		RefreshStatus();
	}

#endregion

#region Private Methods

	private void FollowCameraAngle()
	{
		var cam = Camera.main;
		if (cam != null && view != null)
		{
			float rotX = cam.transform.rotation.eulerAngles.x;
			view.rotation = Quaternion.Euler(rotX, 0, 0);
		}
	}

	private List<PosDistance> GetNeighbors(Vector3 start, Vector3 end)
	{
		neighbors[0].pos.Set(end.x-1, end.y-1, end.z); 
		neighbors[1].pos.Set(end.x-1, end.y-0, end.z); 
		neighbors[2].pos.Set(end.x-1, end.y+1, end.z); 
		neighbors[3].pos.Set(end.x-0, end.y-1, end.z); 
		neighbors[4].pos.Set(end.x-0, end.y+1, end.z); 
		neighbors[5].pos.Set(end.x+1, end.y-1, end.z); 
		neighbors[6].pos.Set(end.x+1, end.y-0, end.z); 
		neighbors[7].pos.Set(end.x+1, end.y+1, end.z); 

		for (int i=0; i<neighbors.Count; i++)
			neighbors[i].distance = Mathf.Infinity;

		foreach (var p in neighbors)
		{
			if (p.pos.x < 0 || p.pos.y < 0)
				continue;

			p.distance = (start-p.pos).sqrMagnitude;
		}

		neighbors.Sort((a, b) => 
				{ 
					if (a.distance < b.distance) return -1;
					else if (a.distance > b.distance) return 1;
					else return 0;
				});

		return neighbors;
	}

	private void DecideAction()
	{
		//Debug.LogFormat("{0} Decide Action", name);
		targetCurr = FindTarget();

		if (targetCurr == null)
			return;

		// snap to destination if current state is moving
		Vector3 targetPos = targetCurr.transform.position;
		if (targetCurr.stateCurr == State.Move)
			targetPos = targetCurr.moveEnd;

		// Check if target is out of range
		Vector3 delta = transform.position - targetPos;
		float distance = delta.magnitude;
		if (distance > (float)classInfo.AttackRange)
		{
			// out of range - move to target
			stateCurr = State.Move;
			cooldownACurr = moveDuration;

			Vector3 dest = targetPos;
			List<PosDistance> list = GetNeighbors(transform.position, targetPos);
			foreach (var l in list)
			{
				if (l.distance == Mathf.Infinity)
					continue;

				if (grid.GetOccupied(l.pos) != 0)
					continue;

				dest = l.pos;
				break;
			}

			List<Vector3> path = grid.FindPath(transform.position, dest);
			if (path != null && path.Count > 0)
			{
				moveStart = transform.position;
				moveEnd = path[0];
				grid.ClearOccupied(moveStart);
				grid.SetOccupied(moveEnd, id);
			}
		}
		else
		{
			// within range - attack
			stateCurr = State.Attack;
			ResetCooldown();
		}
	}

	private FightChar FindTarget()
	{
		if (targets == null)
			return null;

		//Debug.LogFormat("{0} Find target", name);

		float nearestDist = Mathf.Infinity;
		FightChar nearest = null;

		for (int i=0; i<targets.Count; i++)
		{
			FightChar t = targets[i];
			if (t.IsAlive == false)
				continue;

			Vector3 delta = t.transform.position - transform.position;
			float magnitude = delta.magnitude;

			if (nearestDist > magnitude)
			{
				nearest = t;
				nearestDist = magnitude;
			}
		}
		return nearest;
	}

	private void RefreshStatus()
	{
		string status = "";
		foreach (BuffType b in buffs)
		{
			switch (b)
			{
				case BuffType.Stun:
					status = status + "<sprite name=\"Stun\">"; break;
				case BuffType.Atk:
					status = status + "<sprite name=\"Atk\">"; break;
			}
		}

		textStatus.text = status;
	}

	private void RefreshName()
	{
		textName.text = string.Format("{0}\n{1}", charName, classInfo.Name);
		if (team == true)
		{
			textName.color = Color.blue;
		}
		else
		{
			textName.color = Color.red;
		}
	}

	private void SetNFTSprite(Sprite spr)
	{
		sprite.sprite = spr;
	}

	private void SetNFTTexture(Texture2D tex)
	{
		if (tex == null)
			return;

		float targetSize = 70.0f;
		int max = Mathf.Max(tex.width, tex.height);
		float scale = (float)max / targetSize;

		Sprite spr = Sprite.Create(tex, 
				new Rect(0.0f, 0.0f, tex.width, tex.height), 
				new UnityEngine.Vector2(0.5f, 0.0f),
				100 * scale);

		sprite.sprite = spr;
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

		SetClassStats(dataClass.GetByName(className));
	}

	private void SetClassStats(DataClasses.ClassStatsRow info)
	{
		classInfo = info;
		if (info == null)
			return;

		isMelee       = classInfo.AttackRange == 1;
		critChance    = classInfo.CritChance;
		hpCurr        = classInfo.HpVal;

		vfxAttack     = classInfo.VFXAttack;

		cooldownA     = classInfo.AttackSpeedSecs;
		cooldownACurr = cooldownA + Random.Range(0.0f, 0.5f); // add some randomness at the very start

		cooldownS     = classInfo.SkillSpeedSecs;
		cooldownSCurr = cooldownS;

		var dataSkills = DataSkills.Instance;
		char[] seps = new char[] { ' ', ',' };
		string[] skillNames = classInfo.Skills.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);
		foreach (string s in skillNames)
		{
			var skillRow = dataSkills.GetByName(s);
			if (skillRow != null)
			{
				skills.Add(skillRow);
			}
		}

		RefreshSkillBar();
		hpBar.SetSolidColor(team ? Color.green : Color.red);

		//Debug.LogFormat("Class {0}; Speed {1}; ratio {2}; cooldown {3}",
		//		classInfo.Name, classInfo.AttackSpeedSecs, ratio, cooldown);
	}

	private void ResetCooldown()
	{
		cooldownACurr = cooldownA;
	}

	private bool CalcIfCrit()
	{
		return Random.Range(0.0f, 1.0f) <= critChance;
	}

	private void RefreshHPBar()
	{
		float v = hpCurr / classInfo.HpVal;
		hpBar.SetValue(v);
	}

	private void RefreshSkillBar()
	{
		float v = (float)cooldownSCurr / (float)cooldownS;
		skillBar.SetValue(1.0f - v, false);
	}

#endregion

#region Unity Methods

	private void Start()
	{
		//grid = GameObject.FindObjectOfType<Grid>();
		//grid.SetOccupied(transform.position, id);

		FollowCameraAngle();
	}

	private void Update()
	{
	}

#endregion

}

}
