using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;


public class Camera_activator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] cameraList;
    public Transform stud;
    public Transform trigg;
    int flag=0;
    PlayableDirector pd;

    void Start()
    {

      pd = GetComponent<PlayableDirector>();
      if (cameraList.Length > 0){
          cameraList[0].gameObject.SetActive (true);
      }

      for(int i=1;i<cameraList.Length;i++)
        cameraList[i].gameObject.SetActive(false);
    }
    float x=-1;
    int actv=0;

    // Update is called once per frame
    void Update()
    {

        if(stud.position.x <=820 && stud.position.x >=815 && flag==0)
        {
            Debug.Log("Camera activated");
            cameraList[0].gameObject.SetActive(false);
            cameraList[1].gameObject.SetActive(true);
            actv=1;
        }

        if(actv==1)
        {
          pd.Play();

          if(x==-1)
            x=Time.time;

          if(Time.time>=x+30)
          {
            actv=0;
            flag=1;
          };
        }

        else
        {
          cameraList[0].gameObject.SetActive(true);
          cameraList[1].gameObject.SetActive(false);
        }
    }

    // IEnumerator track()
    // {
    //
    //   Debug.Log(Time.time);
    //   Debug.Log("Wait complete");
    // }

}
