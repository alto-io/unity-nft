using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(Animator))]
public class DamageText : MonoBehaviour
{
	private Animator animator;
	private TextMeshProUGUI text;
	private Vector3 startLocalPos;

	private void Start()
	{
		animator = GetComponent<Animator>();

		text = GetComponent<TextMeshProUGUI>();
		text.enabled = false;

		startLocalPos = transform.localPosition;
	}

	public void ShowDamage(int damage, bool isCrit)
	{
		if (text == null)
			return;

		string msg = damage == 0 ? "MISS" : damage.ToString();
		ShowMsg(msg, isCrit ? "Crit" : "Normal");
	}

	public void OnDone()
	{
	}

	public void ShowMsg(string msg)
	{
		text.text = msg;

		if (msg == "Heal") ShowMsg(msg, "Heal");
		else               ShowMsg(msg, "Skill");
	}

	public void ShowMsg(string msg, string animTrigger)
	{
		text.text = msg;
		text.enabled = true;
		animator.SetTrigger(animTrigger);
	}
}

}
