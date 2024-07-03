using UnityEngine;
using UnityEngine.UI;

public class StateText : MonoBehaviour
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
        text.text = "State : " + player.GetComponent<MovePlayer>().States;
    }
}
