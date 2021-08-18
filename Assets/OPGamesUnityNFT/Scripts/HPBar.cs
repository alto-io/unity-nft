using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HPBar : MonoBehaviour
{
	[SerializeField] private Image fill;

	public void SetValue(float val)
	{
		DOTween.To(() => fill.fillAmount, x=> fill.fillAmount = x, val, 0.5f);
	}
}
