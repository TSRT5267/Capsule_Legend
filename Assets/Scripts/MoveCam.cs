using Unity.VisualScripting;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    GameObject localPlayer = PlayerManager.LocalPlayerInstance;

    void Update()
    {
        // 카메라가 플레이를 따라옴
        //transform.position = cameraPosition.position;
        transform.position = localPlayer.transform.Find("CaneraPos").position;     
    }
}
