using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointForest : MonoBehaviour
{
    public Animator animator;

    public void point()
    {
        animator.SetBool("pointForest", true);
    }
}
