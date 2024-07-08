using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Player")]  // �÷��̾�
    [SerializeField] private State state;   // ����
    [SerializeField] private Climbing climbingScript; 


    [Header("Input")] // Ű����
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;               // ����Ű �Ҵ�
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;            // �޸���Ű �Ҵ�
    [SerializeField] private KeyCode CrouchKey = KeyCode.LeftControl;   // ��ũ����Ű �Ҵ�
    private float horizontalInput;
    private float verticalInput;

    [Header("Movement")] // �̵�
    [SerializeField] private float walkSpeed;           // �ȱ�ӵ�
    [SerializeField] private float runSpeed;            // �ٱ�ӵ�
    [SerializeField] private float slideSpeed;          // �����̵�ӵ�                                                    
    [SerializeField] private float wallRunSpeed;        // ��Ÿ�� �ӵ�
    [SerializeField] private float climbSpeed;        // �������� �ӵ�                                                
    [SerializeField] private float groundDrag;          // �ٴ� ����
    [SerializeField] private float speedIncreaseMultiplier;// �ӵ� ���� ���
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

    [Header("Slope")] // ���� �̵�
    [SerializeField] private float maxslopeAngle;       // �ִ� ��簢��
    [SerializeField] private float slopeIncreaseMultiplier;// ���μӵ� ���� ���
    private RaycastHit slopeHit;                        // ���� ����ĳ����
    private bool exitingSlope;                          // ���� Ż��

    [Header("Slide")]    
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    
    [Header("Ground Check")] // �ٴ� Ȯ��
    [SerializeField] private float playerHeight;        // �ٴ����� �Ǵ��� ����
    [SerializeField] private LayerMask whatIsGround;    // �ٴ� ���̾�
    
    [Header("State")]
    [SerializeField] private bool isGround;
    [SerializeField] private bool isCrounch;
    public bool isSlide;
    public bool isWallRun;
    public bool isClimb;

    private Vector3 moveDir;
    private Rigidbody rb;
    
    public enum State
    {
        WALK,
        RUN,
        AIR,
        CROUCH,
        SLIDE,
        WALLRUN,
        CLIMB
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
        if(isClimb) // �� ������
        {
            state = State.CLIMB;
            desiredMoveSpeed = climbSpeed;
        }
        else if(isWallRun) // ��Ÿ��
        {
            state = State.WALLRUN;
            desiredMoveSpeed = wallRunSpeed;
        }
        else if(isSlide) // �����̵�
        {
            state = State.SLIDE;

            if (OnSlope() && rb.linearVelocity.y < 0.1f) 
                desiredMoveSpeed = slideSpeed;
            else 
                desiredMoveSpeed = runSpeed;
        }
        else if(Input.GetKey(CrouchKey)) // ��ũ����
        {
            state = State.CROUCH;
            desiredMoveSpeed = crouchSpeed;
        }
        else if(isGround && Input.GetKey(runKey))    // �޸���
        {
            state = State.RUN;
            desiredMoveSpeed = runSpeed;          
        }
        else if(isGround)                       // �ȱ�
        {
            state = State.WALK;
            desiredMoveSpeed = walkSpeed;
        }
        else                                    // ����
        {
            state = State.AIR;
        }

        // ��ȭ�ؾ��� �ӵ��� 4�̻��� ��� �θ����� �ӵ� ����
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            
        }
        else // �Ϲ����� �޸��� ���� ���� �ﰢ���� �ӵ� ����
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;


        if (state == State.CROUCH) isCrounch = true;
        else isCrounch = false;

    }

    private IEnumerator SmoothlyLerpMoveSpeed() // moveSpeed�� desiredMoveSpeed �ε巴�� ����
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            
            if(OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }          
            
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
    }
   
    private void MyInput() // �÷��̾� �Է�
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");  // �����̵��� 
        verticalInput = Input.GetAxisRaw("Vertical");       // �����̵���
        
        // �غ� �Ǿ��ְ� �ٴ��Ͻ� ��������
        if (Input.GetKey(jumpKey) && readyToJump && isGround) 
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCoolDown); // ���� ������ð�       
        }

        // ��ũ���� ����
        if(Input.GetKeyDown(CrouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); //�����̵�� Y������ ���
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        // ��ũ���� �ߴ�
        if(Input.GetKeyUp(CrouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z); //�����̵�� Y������ ����


        /* // �� ����������  ����
         if(Input.GetAxis("Mouse ScrollWheel") > 0f && !isGround)
         {
             Debug.Log("�ǽ�");
             rb.AddForce(orientation.forward * moveSpeed, ForceMode.Impulse);
         }*/

    }

    private void Move() // �̵�
    {
        if (climbingScript.ExitingWall) return;     //�������� �� ������ Ż��� �ൿ����

        // �̵� ���� ���
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ����
        if (OnSlope() && !exitingSlope)
        {                
            rb.AddForce(GetSlopeMoveDir(moveDir) * moveSpeed * 20f, ForceMode.Force); // ���鿡 �´� �������� �̵�   

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


        if (rb.linearVelocity.magnitude <= 1f && moveSpeed >= 10f)
        {
            StopAllCoroutines();
            moveSpeed = 7.0f;
        }
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

    private void ResetJump()    // ���� ����ȭ
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()  // ������ �ִ� ���� ����
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight * 0.5f + 0.3f))
        {
            
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); //���鿡 ���� ���ϱ�
            return angle != 0 && angle < maxslopeAngle; 
        }
        return false;
    }

    public Vector3 GetSlopeMoveDir(Vector3 dir) // ���鿡�� ����
    {
        // ���� ���� ��ŭ �ٴڸ��� ����
        return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
    }

    //------------------getter , setter----------------------------

    public State States
    {
        get { return state; }
    }

    public bool IsGround
    {
        get { return isGround; }
    }
    
}
