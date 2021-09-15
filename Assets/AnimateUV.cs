using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateUV : MonoBehaviour
{
    public Vector2 scrollSpeed;

    private Renderer renderer;
    private Vector2 offset;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        offset.x += Time.time * scrollSpeed.x;
        offset.y += Time.time * scrollSpeed.y;
        renderer.material.mainTextureOffset = offset;
    }
}
