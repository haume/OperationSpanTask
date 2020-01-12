using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Pressed : MonoBehaviour
{
    public GameObject text;

    void OnTouchDown()
    {
        text.GetComponent<Text>().text = "test";
    }
}
