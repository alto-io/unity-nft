using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLookAtCamera : MonoBehaviour
{
	private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
		cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
		if (cam == null)
			return;

		Vector3 look = cam.transform.position;
		look.y = transform.position.y;
		transform.LookAt(look);
        
    }
}
