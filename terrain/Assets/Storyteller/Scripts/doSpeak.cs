using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doSpeak : MonoBehaviour
{
    public Animator animator;
    

    public void SpeakNow()
    {

        //animator.enabled = true; 
        //  animator.ResetTrigger("isSpeaking");
        animator = GetComponent<Animator>();
        animator.SetBool("isSpeaking", true);
       // animator.speed = 1;
    }
    public void StopSpeak()
    {
        // animator.enabled = false;
        //    animator.ResetTrigger("isSpeaking");
        //  animator = gameObject.GetComponent<Animator>();
        animator = GetComponent<Animator>();
        animator.SetBool("isSpeaking", false);
       // animator.speed = 0;
    }
}
