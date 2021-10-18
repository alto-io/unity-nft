using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HPBar : MonoBehaviour
{
	[SerializeField] private Image fill;
	[SerializeField] private Gradient colorRamp;
	[SerializeField] private float amount;

	public void SetSolidColor(Color c)
	{
		GradientAlphaKey[] alphas = new GradientAlphaKey[2];
		GradientColorKey[] colors = new GradientColorKey[2];

		alphas[0].alpha = 1;
		alphas[0].time = 0;

		alphas[1].alpha = 1;
		alphas[1].time = 1;

		colors[0].color = c;
		colors[0].time = 0;

		colors[1].color = c;
		colors[1].time = 1;

		colorRamp.SetKeys(colors, alphas);

		fill.color = c;
	}

	public void SetValue(float val, bool tween = true)
	{
		if (tween)
		{
			DOTween.To(
					() => fill.fillAmount, 
					x => 
					{ 
						fill.fillAmount = x; 
						fill.color = colorRamp.Evaluate(x); 
						amount = x;
					}, 
					val, 
					0.5f);
		}
		else
		{
			amount = val;
			fill.fillAmount = val;
			fill.color = colorRamp.Evaluate(val); 
		}
	}

	public void Subtract(float val)
	{
		float target = fill.fillAmount - val;
		if (target < 0)
			target = 0;

		SetValue(target);
	}
}
