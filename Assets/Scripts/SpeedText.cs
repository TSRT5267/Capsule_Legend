using UnityEngine;
using UnityEngine.UI;

public class SpeedText : MonoBehaviour
{
    GameObject player;
    Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Speed : " + player.GetComponent<MovePlayer>().MoveSpeed.ToString("F1");
    }
}
