using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;
using UnityEngine.UI;


public class End_Event : MonoBehaviour
{

  Canvas EscCan;


  void start()
  {
    //Find the object you're looking for
    GameObject tempObject = GameObject.Find("Savinodam");
    if(tempObject != null){
        //If we found the object , get the Canvas component from it.
        EscCan = tempObject.GetComponent<Canvas>();
        if(EscCan == null){
            Debug.Log("Could not locate Canvas component on " + tempObject.name);
        }
    }
  }

  void update()
  {

  }

  public void enablecanvas()
  {

    EscCan.gameObject.SetActive(true);
  }

}
