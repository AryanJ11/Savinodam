using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointCow : MonoBehaviour
{
    public Animator animator;

    public void point()
    {
        animator.SetBool("pointCow", true);
    }
}