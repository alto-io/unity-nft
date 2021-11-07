using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

public class DamageTextGroup : MonoBehaviour
{
	private struct Info
	{
		public string Msg;
		public string AnimTrigger;
	}

	const int MAX_QUEUE = 20;

	[SerializeField]
	private float cooldown = 0.5f;

	private DamageText[] texts;
	private int nextIndex = 0;

	private Info[] queue = new Info[MAX_QUEUE];
	private int queueCurr = 0;
	private int queueNext = 0;
	private float cooldownCurr = 0.0f;

	private void Start()
	{
	}

	public void Reset()
	{
		nextIndex = 0;
		queueCurr = 0;
		queueNext = 0;
		cooldownCurr = 0;

		if (texts == null)
			texts = GetComponentsInChildren<DamageText>(true);
		foreach (var t in texts)
			t.enabled = false;
	}

	private void Update()
	{
		cooldownCurr -= Time.deltaTime;
		if (queueCurr != queueNext && cooldownCurr <= 0)
		{
			var t = texts[nextIndex];
			t.ShowMsg(queue[queueCurr].Msg, queue[queueCurr].AnimTrigger);

			queueCurr = (queueCurr + 1) % MAX_QUEUE;
			nextIndex = (nextIndex + 1) % texts.Length;

			cooldownCurr = cooldown;
		}
	}

	public void ShowDamage(int damage, bool isCrit)
	{
		if (texts == null)
			return;

		string trigger = isCrit ? "Crit" : "Normal";
		string msg = damage.ToString();
		if (damage < 0)
		{
			msg = (-damage).ToString();
			trigger = "Heal";
		}
		else if (damage == 0)
		{
			msg = "MISS";
		}

		queue[queueNext].Msg = msg;
		queue[queueNext].AnimTrigger = trigger;
		queueNext = (queueNext+1) % MAX_QUEUE;
	}

	public void ShowMsg(string msg)
	{
		if (texts == null)
			return;

		queue[queueNext].Msg = msg;

		if (msg == "Heal")
			queue[queueNext].AnimTrigger = "Heal";
		else
			queue[queueNext].AnimTrigger = "Skill";

		queueNext = (queueNext+1) % MAX_QUEUE;
	}
}

}
