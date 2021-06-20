using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doSuprabhat_student : MonoBehaviour
{
    public Animator animator;

    public void SitNow()
    {
        animator.SetBool("Suprabhat", true);
    }
    public void notSitNow()
    {
        animator.SetBool("Suprabhat", false);
    }
}
