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
	private static int nextId = 1;
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

	[SerializeField] private DamageTextGroup damageText;
	[SerializeField] private Animator spriteAndUIAnimator;
	[SerializeField] private TextMeshProUGUI textName;
	[SerializeField] private HPBar hpBar;
	[SerializeField] private HPBar cooldownBar;
	[SerializeField] private Transform attackPos;
	[SerializeField] private Transform hitPos;
	[SerializeField] private SpriteRenderer sprite;
	[SerializeField] private bool team = true;

	private int id = 0;
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
	private string projectilePrefab = "";

	private float cooldownCurr = 0;
	private float cooldown = 0;
	private int hpCurr = 0;

	public bool Team { get { return team; } }
	public bool IsAlive { get { return hpCurr > 0; } }
	public bool IsReady { get { return cooldownCurr <= 0; } }
	public int HP { get { return hp; } }

	public System.Action<FightEvent> OnEventTriggered;
	private Animator animator;

	private List<FightChar> targets;
	private FightChar targetCurr = null;
	private State stateCurr = State.Idle;
	private const float moveDuration = 0.1f;

	private Vector3 moveStart;
	private Vector3 moveEnd;
	private Grid grid;

	private void Start()
	{
		grid = GameObject.FindObjectOfType<Grid>();
		grid.SetOccupied(transform.position, id);
	}

	private void Update()
	{
		switch (stateCurr)
		{
			case State.Idle: UpdateStateIdle(); break;
			case State.Move: UpdateStateMove(); break;
			case State.Attack: UpdateStateAttack(); break;
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

		Vector3 targetPos = targetCurr.transform.position;
		if (targetCurr.stateCurr == State.Move)
			targetPos = targetCurr.moveEnd;

		Vector3 delta = transform.position - targetPos;
		float distance = delta.magnitude;
		if (distance > (float)attackRange)
		{
			// move to target
			stateCurr = State.Move;
			cooldownCurr = moveDuration;

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

			//Debug.LogFormat("from={0} to={1}; distance={2}, attackrange={3}", transform.position, dest, (transform.position - dest).magnitude, attackRange);

			List<Vector3> path = grid.FindPath(transform.position, dest);
			if (path != null && path.Count > 0)
			{
				moveStart = transform.position;
				moveEnd = path[0];
				grid.ClearOccupied(moveStart);
				grid.SetOccupied(moveEnd, id);
				//Debug.LogFormat("{0} move to {1}", id, moveEnd);
			}
			else
			{
				//Debug.LogFormat("Path not found");
			}
		}
		else
		{
			// attack
			// Debug.LogFormat("{0} Pick Attack {1}", name, targetCurr.name);
			stateCurr = State.Attack;
			ResetCooldown();
		}
	}

	private void UpdateStateIdle()
	{
		cooldownCurr -= Time.deltaTime;
		if (cooldownCurr <= 0)
			DecideAction();
	}

	private void UpdateStateMove()
	{
		//grid.ClearOccupied(transform.position);

		transform.position = Vector3.Lerp(moveStart, moveEnd, 1.0f - (cooldownCurr / moveDuration));

		//grid.SetOccupied(transform.position, id);

		cooldownCurr -= Time.deltaTime;
		if (cooldownCurr <= 0)
		{
			transform.position = moveEnd;
			moveStart = moveEnd;
			cooldownCurr = 0.05f;
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

		cooldownCurr -= Time.deltaTime;
		if (cooldownCurr <= 0)
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

	public void SetTargets(List<FightChar> t)
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

	private void RefreshName()
	{
		textName.text = string.Format("{0}\n{1}", charName, className);
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
		hp            = classInfo.HP * data.HPMult;
		attackRange   = classInfo.AttackRange;
		attackSpeed   = classInfo.AttackSpeed;
		moveSpeed     = classInfo.MoveSpeed;
		damage        = classInfo.Damage;
		defense       = classInfo.Defense;
		agility       = classInfo.Agility;
		isMelee       = classInfo.IsMelee;
		statMax       = data.StatMax;
		critMaxChance = data.CritMaxChance;
		critMult      = data.CritMult;
		hpCurr        = hp;

		projectilePrefab = classInfo.ProjectilePrefab;

		float ratio   = (float)(attackSpeed-1)/(float)(statMax-1);
		cooldown      = Mathf.Lerp(1.5f, 0.5f, ratio);
		cooldownCurr  = cooldown + Random.Range(0.0f, 0.05f);

		//Debug.LogFormat("Class {0}; Speed {1}; ratio {2}; cooldown {3}",
		//		className, attackSpeed, ratio, cooldown);
	}

	private void ResetCooldown()
	{
		cooldownCurr = cooldown;
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

	public void OnAttackHitRanged()
	{
		if (!isMelee) OnAttackHit();
	}

	public void OnAttackHitMelee()
	{
		if (isMelee) OnAttackHit();
	}

	public void OnAttackHit()
	{
		if (targetCurr == null)
			return;

		bool isCrit = CalcIfCrit();
		int damageFinal = damage;
		if (isCrit)
			damageFinal *= 2;

		targetCurr.damageText.ShowDamage(damage, isCrit);
		targetCurr.hpCurr -= damage;
		targetCurr.RefreshHPBar();

		if (targetCurr.IsAlive == false)
		{
			cooldownCurr = 0.1f;
			stateCurr = State.Idle;
			grid.ClearOccupied(targetCurr.transform.position);
			//Debug.LogFormat("{0} Go to Idle", name);
			targetCurr.animator.SetTrigger("Dead");
		}
		else
		{
			targetCurr.animator.SetTrigger("Hurt");
		}

		if (isCrit)
			Camera.main.DOShakePosition(0.5f, 0.1f, 30, 45, true);
	}

	public void OnAttackEnd()
	{
		ResetCooldown();
	}

	public void OnDeadEnd()
	{
		gameObject.SetActive(false);
	}
}

}
