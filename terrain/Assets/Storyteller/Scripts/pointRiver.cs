using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointRiver : MonoBehaviour
{
    public Animator animator;

    public void point()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("pointRiver", true);
    }
}
