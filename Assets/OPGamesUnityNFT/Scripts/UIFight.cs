using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Animator))]
public class UIFight : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Animator animator;

	private void Start()
	{
	}

	public void OnBtnBack()
	{
		SceneManager.LoadScene(1);
	}

	public void SetTextAnimationTrigger(string trigger, string label)
	{
		if (animator == null) return;
		if (text == null) return;

		text.text = label;
		animator.SetTrigger(trigger);
	}

	public void OnCameraShake()
	{
		Camera.main.DOShakePosition(0.5f, 0.2f, 30, 45, true);
	}
}
