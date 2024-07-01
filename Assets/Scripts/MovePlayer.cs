using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;

    private float horicaontalInput;
    private float verticalInput;
    private Vector3 moveDir;

    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;       // ������ ���� ȸ������ �ʰ� 
    }

    private void MyInput()
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // �����̵��� 
        verticalInput = Input.GetAxisRaw("Vertical");       // �����̵���
    }

    private void Update()
    {
        
    }
}
