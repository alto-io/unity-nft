using UnityEngine;
using DG.Tweening;

public class UISpinner : MonoBehaviour
{
	[SerializeField] private Vector3 rotationSpeed = new Vector3(0,0,5);
	private void Update()
	{
		transform.Rotate(rotationSpeed * Time.deltaTime);
	}
}
