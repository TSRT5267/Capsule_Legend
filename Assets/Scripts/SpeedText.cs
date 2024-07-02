using UnityEngine;
using UnityEngine.UI;

public class SpeedText : MonoBehaviour
{
    GameObject player;
    Text text;

    void Start()
    {
        player = GameObject.Find("Player");
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "Speed : " + player.GetComponent<Rigidbody>().linearVelocity.magnitude.ToString("F1");
    }
}
