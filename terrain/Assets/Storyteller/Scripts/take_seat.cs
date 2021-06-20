using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class take_seat : MonoBehaviour
{
    public Animator animator;

    public void sit()
    {
        animator.SetBool("take_seat", true);
    }
    
    public void stop_sit()
    {
        animator.SetBool("take_seat", false);
    }
}