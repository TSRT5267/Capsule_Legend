using NUnit.Framework;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;          // �ٴ� ����
    [SerializeField] private Transform orientation;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoolDown;
    [SerializeField] private float airMultiplier;
    private bool readyToJump;



    [Header("Ground Check")]
    [SerializeField] private float playerHeight;        // �ٴ����� �Ǵ��� ����
    [SerializeField] private LayerMask whatIsGround;
    private bool isGround;


    private float horicaontalInput;
    private float verticalInput;

    private Vector3 moveDir;

    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;       // ������ ���� ȸ������ �ʰ� 
    }

    private void Update()
    {
        // �ٴ� Ȯ��
        isGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();          // �÷��̾� �Է�
        SpeedControl();     // ���� �ְ�ӵ� ����

        // �ٴ� ����
        if (isGround)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        Move(); //�⺻ �̵�
    }


    private void MyInput()
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // �����̵��� 
        verticalInput = Input.GetAxisRaw("Vertical");       // �����̵���
    }

    private void Move()
    {
        // �̵� ���� ���
        moveDir = orientation.forward * verticalInput + orientation.right * horicaontalInput;

        rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x ,0,rb.linearVelocity.z); // �������� �ӵ�

        if(flatVel.magnitude > moveSpeed) //���������ӵ� > �ְ�ӵ�
        {
            // �ְ� �ӵ��� ����
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z); 
        }
    }

    private void Jump()
    {
        // ������ Y�ӵ� �ʱ�ȭ
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    //------------------getter , setter----------------------------

    public float MoveSpeed
    {
        get { return moveSpeed; }       
    }
    
}
