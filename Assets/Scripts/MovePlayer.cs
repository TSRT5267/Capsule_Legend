using NUnit.Framework;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;          // 바닥 저항
    [SerializeField] private Transform orientation;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoolDown;
    [SerializeField] private float airMultiplier;
    private bool readyToJump;



    [Header("Ground Check")]
    [SerializeField] private float playerHeight;        // 바닥인지 판단할 높이
    [SerializeField] private LayerMask whatIsGround;
    private bool isGround;


    private float horicaontalInput;
    private float verticalInput;

    private Vector3 moveDir;

    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;       // 물리로 인해 회전되지 않게 
    }

    private void Update()
    {
        // 바닥 확인
        isGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();          // 플레이어 입력
        SpeedControl();     // 평지 최고속도 조절

        // 바닥 저항
        if (isGround)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        Move(); //기본 이동
    }


    private void MyInput()
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // 수평이동값 
        verticalInput = Input.GetAxisRaw("Vertical");       // 수직이동값
    }

    private void Move()
    {
        // 이동 방향 계산
        moveDir = orientation.forward * verticalInput + orientation.right * horicaontalInput;

        rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x ,0,rb.linearVelocity.z); // 평지에서 속도

        if(flatVel.magnitude > moveSpeed) //평지에서속도 > 최고속도
        {
            // 최고 속도로 고정
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z); 
        }
    }

    private void Jump()
    {
        // 점프시 Y속도 초기화
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    //------------------getter , setter----------------------------

    public float MoveSpeed
    {
        get { return moveSpeed; }       
    }
    
}
