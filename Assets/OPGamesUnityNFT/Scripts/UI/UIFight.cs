using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{


[RequireComponent(typeof(Animator))]
public class UIFight : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Text textSpeed;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private FightManager manager;

	private float speed = 1.0f;

	private float[] speedValues = new float[] { 1, 0.5f, 0.25f };
	private int speedIndex = 0;

	private void Start()
	{
	}

	public void OnBtnBack()
	{
		SceneManager.LoadScene(1);
	}

	public void OnBtnSpeed()
	{
		speedIndex = (speedIndex + 1) % speedValues.Length;
		speed = speedValues[speedIndex];
		if (textSpeed != null)
		{
			textSpeed.text = speed.ToString() + "x";
		}

		Time.timeScale = speed;
	}

	public void OnBtnReplay()
	{
		manager.PlayFight();
	}

	public void OnBtnReplayFromFile()
	{
		manager.LoadReplayFromFile();
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

}
