using UnityEngine;
using UnityEngine.UI;
public class next_button_invoker : MonoBehaviour
{
    // Start is called before the first frame update
     public KeyCode key;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            GetComponent<Button>().onClick.Invoke();
        }
    }
}