using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_controller : MonoBehaviour
{
    float sensor_length=1.0f;
    float speed=5f;
    float turnSpeed=5f;
    Animator anim;
    Rigidbody rb;

    float dirVal=1.0f;
    float turnVal=0.0f;


    Collider myCollider;

    // Start is called before the first frame update
    void Start()
    {
      myCollider=transform.GetComponent<Collider>();
      rb = GetComponent<Rigidbody>();
      anim=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
      RaycastHit hit;
      int flag=0;
      //forward sensor

      if(Physics.Raycast(transform.position+ new Vector3(0,1,0), transform.forward, out hit, (sensor_length+transform.localScale.z)))
      {
        if(hit.collider.tag!="Obstacle" || hit.collider==myCollider)
          return;

        transform.Rotate(0,90,0);
        flag=1;

      }

      //backward sensor
      // if(Physics.Raycast(transform.position+ new Vector3(0,1,0), -transform.forward, out hit, (sensor_length+transform.localScale.z)))
      // {
      //   if(hit.collider.tag!="Obstacle" || hit.collider==myCollider)
      //     return;
      //
      //   if(dirVal==-1.0f)
      //       dirVal=1.0f;
      //   flag=1;
      // }

      //right sensor
      if(Physics.Raycast(transform.position+ new Vector3(0,1,0), transform.right, out hit, (sensor_length+transform.localScale.x)))
      {
        if(hit.collider.tag!="Obstacle" || hit.collider==myCollider)
          return;
        turnVal-=1;
        flag=1;

      }

      //left sensor
      if(Physics.Raycast(transform.position+ new Vector3(0,1,0), -transform.right, out hit, (sensor_length+transform.localScale.x)))
      {
        if(hit.collider.tag!="Obstacle" || hit.collider==myCollider)
          return;
        turnVal+=1;
        flag=1;
      }

      if(flag==0)
        turnVal=0;

      transform.Rotate(0,turnVal*turnSpeed*Time.deltaTime,0);

      transform.position+=transform.forward*(speed*dirVal)*Time.deltaTime;
      anim.SetBool("isWalking", true);

    }
}
