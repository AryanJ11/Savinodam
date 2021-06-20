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
    public void stop_point()
    {
        animator.SetBool("pointCow", false);
    }
}