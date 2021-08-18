using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class FightManager : MonoBehaviour
{
	[SerializeField] private GameObject[] player;
	[SerializeField] private HPBar[] playerHP;

	[SerializeField] private TextMeshProUGUI textFight;

	private float[] hp = new float[2];

    private IEnumerator Start()
    {
		var seqFight = DOTween.Sequence();
		seqFight.Append(textFight.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f));
		seqFight.Append(textFight.transform.DOScale(Vector3.zero, 0.5f));

		yield return seqFight.WaitForCompletion();
        
		hp[0] = 100;
		hp[1] = 100;

		Vector3[] waypoints0 = new Vector3[]
		{
			player[0].transform.position,
			player[0].transform.position + (player[0].transform.up * 2.0f),
			player[1].transform.position
		};

		Vector3[] waypoints1 = new Vector3[]
		{
			player[1].transform.position,
			player[1].transform.position + (player[1].transform.up * 2.0f),
			player[0].transform.position
		};

		while (hp[0] > 0 && hp[1] > 0)
		{
			var seqAttack0 = DOTween.Sequence();
			seqAttack0.Append(
					player[0].transform.DOPath(waypoints0, 1.0f, PathType.CatmullRom)
					.SetEase(Ease.InBack));
			seqAttack0.Append(player[0].transform.DOMove(player[0].transform.position, 1.0f));

			yield return new WaitForSeconds(1.0f);
			hp[1] -= (float)Random.Range(10,20);
			playerHP[1].SetValue(hp[1]/100.0f);

			if (hp[1] <= 0)
				break;

			yield return seqAttack0.WaitForCompletion();

			yield return new WaitForSeconds(0.5f);

			var seqAttack1 = DOTween.Sequence();
			seqAttack1.Append(
					player[1].transform.DOPath(waypoints1, 1.0f, PathType.CatmullRom)
					.SetEase(Ease.InBack));
			seqAttack1.Append(player[1].transform.DOMove(player[1].transform.position, 1.0f));

			yield return new WaitForSeconds(1.0f);
			hp[0] -= (float)Random.Range(10,20);
			playerHP[0].SetValue(hp[0]/100.0f);

			if (hp[0] <= 0)
				break;

			yield return seqAttack1.WaitForCompletion();

			yield return new WaitForSeconds(0.5f);
		}

		if (hp[0] <= 0)
		{
			textFight.text = "Player 2 Wins";
		}
		else
		{
			textFight.text = "Player 1 Wins";
		}

		textFight.transform.DOScale(Vector3.one, 1.0f);
    }
}
