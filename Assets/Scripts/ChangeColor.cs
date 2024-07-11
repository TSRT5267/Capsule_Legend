using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    [SerializeField]private GameObject player;
    [SerializeField]private Renderer playerRenderer;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float HpPercentage = (float)player.GetComponent<MovePlayer>().hp / 100f;
        Color newColor = Color.Lerp(Color.red, Color.blue, HpPercentage);

        playerRenderer.material.color = newColor;
    }
}
