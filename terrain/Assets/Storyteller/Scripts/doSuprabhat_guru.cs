using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doSuprabhat_guru : MonoBehaviour
{
    public Animator animator;

    public void namaste()
    {
        animator.SetBool("Suprabhat_guru", true);
    }
    public void stop_namaste()
    {
        animator.SetBool("Suprabhat_guru", false);
    }
}