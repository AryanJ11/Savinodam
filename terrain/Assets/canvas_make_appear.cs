using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class canvas_make_appear : MonoBehaviour
{
    public GameObject vv;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            vv.SetActive(true);
        }
    }
}
