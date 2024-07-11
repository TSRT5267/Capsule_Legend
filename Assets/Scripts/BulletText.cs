using UnityEngine;
using UnityEngine.UI;

public class BulletText : MonoBehaviour
{
    public GameObject gun;
    Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gun.GetComponent<Gun>().ISReload)
            text.text = "Bullet : " + gun.GetComponent<Gun>().ToTalBullet.ToString();
        else
            text.text = "  RELOAD  ";
    }
}
