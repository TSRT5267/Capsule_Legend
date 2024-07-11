using UnityEngine;
using UnityEngine.UI;

public class SpeedText : MonoBehaviour
{
    public GameObject player;
    Text text;

    void Start()
    {       
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "Speed : " + player.GetComponent<Rigidbody>().linearVelocity.magnitude.ToString("F1");
    }
}
