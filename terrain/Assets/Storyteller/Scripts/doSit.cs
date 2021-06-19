using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doSit : MonoBehaviour
{
    public Animator animator;

    public void SitNow()
    {
        animator.SetBool("sit", true);
    }
}
