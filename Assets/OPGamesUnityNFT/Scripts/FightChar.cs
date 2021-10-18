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
	public bool Team    { get { return team; } }
	public bool IsAlive { get { return hpCurr > 0; } }
	public bool IsReady { get { return cooldownACurr <= 0; } }
	public float HP     { get { return hpCurr; } }
#endregion

#region Exposed Variables
	[SerializeField] private DamageTextGroup damageText;
	[SerializeField] private Animator spriteAndUIAnimator;
	[SerializeField] private TextMeshProUGUI textName;
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar skillBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private Transform view;
	[SerializeField] private SpriteRenderer sprite;
	[SerializeField] private bool team = true;
#endregion

#region Private variables

	private int id = 0;
	private bool isMelee;
	private float hpCurr = 0;
	private float stunDuration = 0;
	private float critChance;
	private string charName = "";
	private string projectilePrefab = "";

	private DataClasses.ClassStatsRow classInfo;

	// cooldown attack
	private float cooldownACurr = 0;
	private float cooldownA = 0;

	// cooldown skill
	private float cooldownSCurr = 0;
	private float cooldownS = 0;

	private Animator animator;

	private List<FightChar> targets;
	private FightChar targetCurr = null;
	private State stateCurr = State.Idle;
	private const float moveDuration = 0.1f;

	private Vector3 moveStart;
	private Vector3 moveEnd;
	private Grid    grid;

	private List<DataSkills.SkillsRow> skills = new List<DataSkills.SkillsRow>();

#endregion

#region Public Methods

	public void SetTargets(List<FightChar> t)
	{
		targets = t;
	}

	public void Reset()
	{
		hpCurr = classInfo.HpVal;
		cooldownACurr = cooldownA;
	}

	public void SetRandomTempNFT()
	{
		Debug.LogFormat("SetRandomTempNFT");
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
		Debug.LogFormat("SetNFT {0}", key);
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
		RefreshName();

		if (nft.Spr != null) SetNFTSprite(nft.Spr);
		else                 SetNFTTexture(nft.Texture);
	}

	public void Init()
	{
		id = nextId;
		nextId++;

		animator = GetComponent<Animator>();

		if (spriteAndUIAnimator != null)
		{
			spriteAndUIAnimator.Play(0, -1, Random.Range(0.0f, 1.0f));
		}
		RefreshName();
		enabled = false;
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
		List<PosDistance> list = new List<PosDistance>();
		list.Add(new PosDistance() { pos = new Vector3(end.x-1, end.y-1, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x-1, end.y-0, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x-1, end.y+1, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x-0, end.y-1, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x-0, end.y+1, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x+1, end.y-1, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x+1, end.y-0, end.z), distance = Mathf.Infinity });
		list.Add(new PosDistance() { pos = new Vector3(end.x+1, end.y+1, end.z), distance = Mathf.Infinity });

		foreach (var p in list)
		{
			if (p.pos.x < 0 || p.pos.y < 0)
				continue;

			p.distance = (start-p.pos).sqrMagnitude;
		}

		list.Sort((a, b) => 
				{ 
					if (a.distance < b.distance) return -1;
					else if (a.distance > b.distance) return 1;
					else return 0;
				});

		return list;
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

	private void UpdateStateIdle()
	{
		if (cooldownACurr <= 0)
			DecideAction();
	}

	private void UpdateStateMove()
	{
		transform.position = Vector3.Lerp(moveStart, moveEnd, 1.0f - (cooldownACurr / moveDuration));

		if (cooldownACurr <= 0)
		{
			transform.position = moveEnd;
			moveStart = moveEnd;
			cooldownACurr = 0.05f;
			stateCurr = State.Idle;
		}
	}

	private void UpdateStateAttack()
	{
		if (targetCurr == null || targetCurr.IsAlive == false)
		{
			DecideAction();
			return;
		}

		if (cooldownACurr <= 0)
		{
			string trigger = "AttackNorth";
			Vector3 targetPos = targetCurr.transform.position;
			Vector3 currPos = transform.position;
			if (targetPos.x < currPos.x)
			{
				trigger = "AttackWest";
			}
			else if (targetPos.x > currPos.x)
			{
				trigger = "AttackEast";
			}
			else if (targetPos.y < currPos.y)
			{
				trigger = "AttackSouth";
			}

			animator.SetTrigger(trigger);

			if (isMelee == false)
			{
				var info = DataVFX.Instance.GetByName(projectilePrefab);
				if (info != null && info.Prefab != null)
				{
					GameObject clone = Instantiate(info.Prefab);
					clone.transform.position = transform.position;
					clone.transform.LookAt(targetCurr.transform.position);
					var projectile = clone.transform.DOMove(targetCurr.transform.position, 0.5f)
						.OnComplete(()=> { Destroy(clone); OnAttackHitRanged(); });
				}
			}
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

		isMelee       = string.IsNullOrEmpty(classInfo.ProjectileName);
		critChance    = classInfo.CritChance;
		hpCurr        = classInfo.HpVal;

		projectilePrefab = classInfo.ProjectileName;

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
		grid = GameObject.FindObjectOfType<Grid>();
		grid.SetOccupied(transform.position, id);

		FollowCameraAngle();
	}

	private void Update()
	{
		if (stunDuration > 0)
		{
			stunDuration -= Time.deltaTime;
		}
		else
		{
			cooldownSCurr -= Time.deltaTime;
			cooldownACurr -= Time.deltaTime;
		}

		switch (stateCurr)
		{
			case State.Idle: UpdateStateIdle(); break;
			case State.Move: UpdateStateMove(); break;
			case State.Attack: UpdateStateAttack(); break;
		}

		RefreshSkillBar();
	}

#endregion

}

}
