using Unity.VisualScripting;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    GameObject localPlayer = PlayerManager.LocalPlayerInstance;

    void Update()
    {
        // ī�޶� �÷��̸� �����
        //transform.position = cameraPosition.position;
        transform.position = localPlayer.transform.Find("CaneraPos").position;     
    }
}
