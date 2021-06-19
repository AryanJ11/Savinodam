using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doSpeak : MonoBehaviour
{
    public Animator animator;

    public void SpeakNow()
    {
        animator.SetBool("isSpeaking", true);
    }
    public void StopSpeak()
    {
        animator.SetBool("isSpeaking", false);
    }
}
