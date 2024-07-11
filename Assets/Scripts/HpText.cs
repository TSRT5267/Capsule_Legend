using UnityEngine;
using UnityEngine.UI;

public class HpText : MonoBehaviour
{
    public GameObject player;
    Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "HP : " + player.GetComponent<MovePlayer>().hp.ToString();
    }
}
