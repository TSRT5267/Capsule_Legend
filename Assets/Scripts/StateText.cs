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
        if(player.GetComponent<MovePlayer>().States == MovePlayer.State.WALK)
            text.text = "State : Walk";
        else if(player.GetComponent<MovePlayer>().States == MovePlayer.State.AIR)
            text.text = "State : Air";
        else if (player.GetComponent<MovePlayer>().States == MovePlayer.State.RUN)
            text.text = "State : Run";
    }
}
