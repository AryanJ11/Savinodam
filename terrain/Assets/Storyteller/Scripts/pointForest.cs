using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointForest : MonoBehaviour
{
    public Animator animator;

    public void point()
    {
        animator.SetBool("pontForest", true);
    }
    public void stop_point()
    {
        animator.SetBool("pontForest", false);
    }
}
