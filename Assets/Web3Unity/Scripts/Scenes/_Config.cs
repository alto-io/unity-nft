using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Config : MonoBehaviour
{
    // account will be updated once user logs in
    public static string Account = "0x0000000000000000000000000000000000000000";
	public const string AccountEmpty = "0x0000000000000000000000000000000000000000";

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
