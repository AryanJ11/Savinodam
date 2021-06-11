using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class man_movement : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;
    bool flag;

    float dirX, dirY, headRotation = 0f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim=GetComponent<Animator>();
    }

    void Update() {

        Vector3 moveBy = transform.right * 1f + transform.forward * 1f;

        float actualSpeed = 5;

        rb.MovePosition(transform.position + moveBy.normalized * actualSpeed * Time.deltaTime);

        makeAnimation();
    }

    void makeAnimation()
    {
        //Debug.Log(flag);
        anim.SetBool("isWalking", true);
    }

}
