using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private float SensX;           // X�ΰ���
    [SerializeField] private float SensY;           // Y�ΰ���
    [SerializeField] private Transform orientation; // ���� 
    [SerializeField] private float MaxPitch;        // �ִ� ���ϼ� �ִ� ��
    
    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   // ���콺 ���
        Cursor.visible = false;                     // ���콺 �񰡽�ȭ
    }
   
    void Update()
    {       
        // ���콺 ��ǲ
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * SensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * SensY;

        // ���콺�� Rotation�� ��� ������ �ٸ� 
        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -MaxPitch, MaxPitch);

        // ���Ϸ� -> ���ʹϾ�
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); // ī�޶�
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);       // ����


    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
}
