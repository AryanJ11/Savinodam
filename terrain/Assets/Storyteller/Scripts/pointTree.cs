using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointTree: MonoBehaviour
{
    public Animator animator;
    public void point()
    {
        animator.SetBool("pointTree", true);
    }

}
