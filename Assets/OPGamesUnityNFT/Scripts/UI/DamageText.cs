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

	private bool initialized = false;

	private void Start()
	{
	}

	public void OnDone()
	{
	}

	public void ShowMsg(string msg, string animTrigger)
	{
		if (!initialized) Init();

		if (text != null)
		{
			text.text = msg;
			text.enabled = true;
		}
		else
		{
			Debug.Log("DamageText:ShowMsg - text is null");
		}

		if (animator != null)
		{
			animator.SetTrigger(animTrigger);
		}
	}

	private void Init()
	{
		animator = GetComponent<Animator>();

		text = GetComponent<TextMeshProUGUI>();
		if (text != null)
		{
			text.enabled = false;
		}
		else
		{
			Debug.Log("DamageText:Start - cannot find TextMeshProUGUI");
		}

		startLocalPos = transform.localPosition;

		initialized = true;
	}
}

}
