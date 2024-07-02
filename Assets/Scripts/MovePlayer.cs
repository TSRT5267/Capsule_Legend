using NUnit.Framework;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Player")]  // �÷��̾�
    [SerializeField] private State state;   // ����

    [Header("KeyBinds")] // Ű����
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;               // ����Ű �Ҵ�
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;            // �޸���Ű �Ҵ�
    [SerializeField] private KeyCode CrouchKeyHold = KeyCode.LeftControl;   // ��ũ����Ű Ȧ�� �Ҵ�
    [SerializeField] private KeyCode CrouchKeyToggle = KeyCode.C;           // ��ũ����Ű ��� �Ҵ�

    [Header("Move")] // �̵�
    [SerializeField] private float walkSpeed;           // �ȱ�ӵ�
    [SerializeField] private float runSpeed;            // �ٱ�ӵ�
    [SerializeField] private float groundDrag;          // �ٴ� ����
    [SerializeField] private Transform orientation;     // ȸ������ �޾ƿ� ���
    private float moveSpeed;           // �̵��ӵ�

    [Header("Jump")] // ����
    [SerializeField] private float jumpForce;           // ������
    [SerializeField] private float jumpCoolDown;        // ���� ������ð�
    [SerializeField] private float airMultiplier;       // ���������
    private bool readyToJump = true;

    [Header("Crouching")] // ��ũ����
    [SerializeField] private float crouchSpeed;         // ��ũ�� ���¿��� �ӵ� 
    [SerializeField] private float crouchYScale;        // ��ũ�� ���¿��� Ű
    private float startYScale;
    private bool isCrouch = false;

    [Header("Slope Handle")] // ���� ����
    [SerializeField] private float maxslopeAngle;       // �ִ� ��簢��
    private RaycastHit slopeHit;                        // ���� ����ĳ����
    private bool exitingSlope;                          // ���� Ż��

    [Header("Ground Check")] // �ٴ� Ȯ��
    [SerializeField] private float playerHeight;        // �ٴ����� �Ǵ��� ����
    [SerializeField] private LayerMask whatIsGround;    // �ٴ� ���̾�
    private bool isGround;


    private float horicaontalInput;
    private float verticalInput;

    private Vector3 moveDir;

    private Rigidbody rb;
    

    public enum State
    {
        WALK,
        RUN,
        AIR,
        CROUCH,
    }



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;       // ������ ���� ȸ������ �ʰ� 

        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        Move(); //�⺻ �̵�
    }

    private void Update()
    {
        // �ٴ� Ȯ��
        isGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();          // �÷��̾� �Է�
        SpeedControl();     // XZ �ְ�ӵ� ����
        StateHandler();     // ���� ����

        // �ٴ� ����
        if (isGround)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    

//--------------------------------------------------------------------------------------------
    private void StateHandler() // ���� ����
    {
        if(isCrouch)                            // ��ũ����
        {
            state = State.CROUCH;
            moveSpeed = crouchSpeed;
        }
        else if(isGround && Input.GetKey(runKey))    // �޸���
        {
            state = State.RUN;
            moveSpeed = runSpeed;          
        }
        else if(isGround)                       // �ȱ�
        {
            state = State.WALK;
            moveSpeed = walkSpeed;
        }
        else                                    // ����
        {
            state = State.AIR;
        }
       
    }
   
    private void MyInput() // �÷��̾� �Է�
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // �����̵��� 
        verticalInput = Input.GetAxisRaw("Vertical");       // �����̵���
        
        // �غ� �Ǿ��ְ� �ٴ��Ͻ� ��������
        if (Input.GetKey(jumpKey) && readyToJump && isGround) 
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCoolDown); // ���� ������ð�       
        }

        

        Crouch();

        /* // �� ����������  ����
         if(Input.GetAxis("Mouse ScrollWheel") > 0f && !isGround)
         {
             Debug.Log("�ǽ�");
             rb.AddForce(orientation.forward * moveSpeed, ForceMode.Impulse);
         }*/

    }

    private void Move() // �̵�
    {
        // �̵� ���� ���
        moveDir = orientation.forward * verticalInput + orientation.right * horicaontalInput;

        // ����
        if (OnSlope() && !exitingSlope)
        {                
            rb.AddForce(GetSlopeMoveDir() * moveSpeed * 20f, ForceMode.Force); // ���鿡 �´� �������� �̵�   

            if (rb.linearVelocity.y > 0) // ���� ƨ�������
                rb.AddForce(Vector3.down*80f, ForceMode.Force); // �Ʒ��� �����ο� 
        }
            
        
        // �ٴ�
        else if(isGround)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force); // Force���� �������� ������

        // ����
        else if (!isGround)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force); // ���� ����� �߰�

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() // �ְ�ӵ� ����
    {
        if(OnSlope() && !exitingSlope) // �����ΰ��
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // �������� �ӵ�

            if (flatVel.magnitude > moveSpeed) //XZ�����ӵ� > �ְ�ӵ�
            {
                // �ְ� �ӵ��� ����
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        
    }

    private void Jump() // ����
    {
        exitingSlope = true;

        // ������ Y�ӵ� �ʱ�ȭ
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);   //Inpulse���� �ѹ��� ������
        
    }

    private void Crouch() // ��ũ����
    {
        // ��ũ���� ���
        if (Input.GetKeyDown(CrouchKeyToggle))
        {
            isCrouch = !isCrouch;
            if (isCrouch)
            {
                // Yũ�� ���
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);  // �پ�� ũ�⸸ŭ �Ʒ��� �̵�
            }
            else
            {
                // Yũ�� ����
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }

        // ��ũ���� Ȧ��
        if (Input.GetKeyDown(CrouchKeyHold))
        {
            isCrouch = true;
            // Ȧ�尡 �������� ���� ��� ���¸� �����ϰ� ũ�� ���
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);  // �پ�� ũ�⸸ŭ �Ʒ��� �̵�
        }
        else if (Input.GetKeyUp(CrouchKeyHold))  // ��� ���°� �ƴϸ� ũ�� ����
        {
            isCrouch = false;
            // Yũ�� ����
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void ResetJump()    // ���� ����ȭ
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()  // ������ �ִ� ���� ����
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight * 0.5f + 0.3f))
        {
            
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); //���鿡 ���� ���ϱ�
            return angle != 0 && angle < maxslopeAngle; 
        }
        return false;
    }

    private Vector3 GetSlopeMoveDir() // ���鿡�� ����
    {
        // ���� ���� ��ŭ �ٴڸ��� ����
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    //------------------getter , setter----------------------------

    public State States
    {
        get { return state; }
    }
    
}
