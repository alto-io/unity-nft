using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateUV : MonoBehaviour
{
    public Vector2 scrollSpeed;

    private Vector2 offset;

	private Renderer r;

	private void Start()
	{
		r = GetComponent<Renderer>();
	}

    private void Update()
    {
        offset.x += Time.deltaTime * scrollSpeed.x;
        offset.y += Time.deltaTime * scrollSpeed.y;
        r.material.mainTextureOffset = offset;
    }
}
