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

		text.text = damage == 0 ? "MISS" : damage.ToString();
		text.enabled = true;
		animator.SetTrigger(isCrit ? "Crit" : "Normal");
	}

	public void OnDone()
	{
	}
}

}
