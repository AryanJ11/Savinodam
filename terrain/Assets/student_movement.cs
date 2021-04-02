using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class student_movement : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;

    [SerializeField] float speed;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundChecker;
    [SerializeField] float checkRadius;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform cam;
    [SerializeField] float sensitivity;
    [SerializeField] float headRotationLimit = 90f;

    float dirX, dirY, headRotation = 0f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim=GetComponent<Animator>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float x1 = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float y1 = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * -1f;

        transform.Rotate(0f, x1, 0f);

        headRotation += y1;
        headRotation = Mathf.Clamp(headRotation, -headRotationLimit, headRotationLimit);
        cam.localEulerAngles = new Vector3(headRotation, 0f, 0f);

        Vector3 moveBy = transform.right * x + transform.forward * z;

        float actualSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift)) {
            actualSpeed *= sprintMultiplier;
        }

        rb.MovePosition(transform.position + moveBy.normalized * actualSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround()) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        makeAnimation();

        dirX=x*actualSpeed;
        dirY=z*actualSpeed;
    }

    void makeAnimation()
    {
        if(dirX==0 && dirY==0)
        {
          anim.SetBool("isWalking", false);
          anim.SetBool("isRunning", false);
        }

        if((Mathf.Abs(dirX)==speed || Mathf.Abs(dirX)==speed
            || Mathf.Abs(dirY)==speed || Mathf.Abs(dirY)==speed)
            && rb.velocity.y==0)
          anim.SetBool("isWalking", true);

        else if((Mathf.Abs(dirX)==speed*sprintMultiplier || Mathf.Abs(dirX)==speed*sprintMultiplier
                || Mathf.Abs(dirY)==speed*sprintMultiplier || Mathf.Abs(dirY)==speed*sprintMultiplier)
                && rb.velocity.y==0)
          anim.SetBool("isRunning", true);
        else
          anim.SetBool("isRunning", false);

    }

    bool IsOnGround() {
        Collider[] colliders = Physics.OverlapSphere(groundChecker.position, checkRadius, groundLayer);

        if (colliders.Length > 0) {
            return true;
        }else {
            return false;
        }
    }

}
