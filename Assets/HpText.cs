using UnityEngine;
using UnityEngine.UI;

public class HpText : MonoBehaviour
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
        text.text = "HP : " + player.GetComponent<MovePlayer>().hp.ToString();
    }
}
