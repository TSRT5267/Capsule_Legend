using UnityEngine;
using UnityEngine.UI;

public class StateText : MonoBehaviour
{
    public GameObject player;
    Text text;

    void Start()
    {
        
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "State : " + player.GetComponent<MovePlayer>().States;
    }
}
