using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;

    void Update()
    {
        // ī�޶� �÷��̸� �����
        transform.position = cameraPosition.position;  
    }
}
