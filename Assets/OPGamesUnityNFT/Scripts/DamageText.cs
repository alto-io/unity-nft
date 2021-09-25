using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

namespace OPGames.NFT
{

//[RequireComponent(TextMeshProUGUI)]
public class DamageText : MonoBehaviour
{
	[SerializeField] private float duration = 0.5f;

	private TextMeshProUGUI text;
	private Vector3 startLocalPos;

	private Sequence seqScale;
	private Sequence seqMove;
	private Sequence seqAlpha;

	private void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
		text.enabled = false;

		startLocalPos = transform.localPosition;
	}

	public void ShowDamage(int damage, bool isCrit)
	{
		if (text == null)
			return;

		StopCoroutine("DamageCR");

		if (seqScale != null) seqScale.Complete();
		if (seqMove != null) seqMove.Complete();
		if (seqAlpha != null) seqAlpha.Complete();

		damage = damage * 1000;

		text.text = damage == 0 ? "MISS" : damage.ToString();
		text.transform.localScale = Vector3.one;
		text.alpha = 0.0f;
		text.color = isCrit ? Color.red : Color.black;
		text.enabled = true;
		transform.localPosition = startLocalPos;

		StartCoroutine("DamageCR", isCrit);

	}

	public IEnumerator DamageCR(bool isCrit)
	{
		Vector3 localPos = transform.localPosition;

		float multCrit = isCrit ? 2.0f : 1.0f;
		seqScale = DOTween.Sequence();
		seqScale.Append(text.transform.DOScale(1.0f * multCrit, 0.2f));
		if (isCrit) seqScale.AppendInterval(0.2f);
		seqScale.Append(text.transform.DOScale(0.5f * multCrit, 0.5f));

		seqMove = DOTween.Sequence();
		seqMove.AppendInterval(0.2f);
		if (isCrit) seqMove.AppendInterval(0.2f);
		seqMove.Append(text.transform.DOLocalMoveY(30.0f, 0.5f));

		seqAlpha = DOTween.Sequence();
		seqAlpha.Append(DOTween.To(() => text.alpha, (x) => text.alpha = x, 1.0f, 0.2f));
		seqAlpha.AppendInterval(0.3f);
		if (isCrit) seqAlpha.AppendInterval(0.2f);
		seqAlpha.Append( DOTween.To(() => text.alpha, (x) => text.alpha = x, 0.0f, 0.2f));

		yield return seqScale.WaitForCompletion();

		text.enabled = false;
	}
}

}
