using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointRiver : MonoBehaviour
{
    public Animator animator;

    public void point()
    {
        animator.SetBool("pointRiver", true);
    }
}
