using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MyTest : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void HelloWorld();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Test successful!");
            HelloWorld();
        }
    }
}
