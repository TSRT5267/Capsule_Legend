using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private float SensX;           // X민감도
    [SerializeField] private float SensY;           // Y민감도
    [SerializeField] private Transform orientation; // 방향 
    [SerializeField] private float MaxPitch;        // 최대 숙일수 있는 각
    
    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   // 마우스 잠금
        Cursor.visible = false;                     // 마우스 비가시화
    }
   
    void Update()
    {       
        // 마우스 인풋
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * SensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * SensY;

        // 마우스와 Rotation이 축과 방향이 다름 
        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -MaxPitch, MaxPitch);

        // 오일러 -> 쿼터니언
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); // 카메라
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);       // 방향


    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
}
