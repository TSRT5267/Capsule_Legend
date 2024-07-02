using NUnit.Framework;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Player")]  // 플레이어
    [SerializeField] private State state;   // 상태

    [Header("KeyBinds")] // 키설정
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;               // 점프키 할당
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;            // 달리기키 할당
    [SerializeField] private KeyCode CrouchKeyHold = KeyCode.LeftControl;   // 움크리기키 홀드 할당
    [SerializeField] private KeyCode CrouchKeyToggle = KeyCode.C;           // 움크리기키 토글 할당

    [Header("Move")] // 이동
    [SerializeField] private float walkSpeed;           // 걷기속도
    [SerializeField] private float runSpeed;            // 뛰기속도
    [SerializeField] private float groundDrag;          // 바닥 저항
    [SerializeField] private Transform orientation;     // 회전값을 받아올 대상
    private float moveSpeed;           // 이동속도

    [Header("Jump")] // 점프
    [SerializeField] private float jumpForce;           // 점프력
    [SerializeField] private float jumpCoolDown;        // 점프 재사용대기시간
    [SerializeField] private float airMultiplier;       // 공중제어력
    private bool readyToJump = true;

    [Header("Crouching")] // 움크리기
    [SerializeField] private float crouchSpeed;         // 움크린 상태에서 속도 
    [SerializeField] private float crouchYScale;        // 움크린 상태에서 키
    private float startYScale;
    private bool isCrouch = false;

    [Header("Slope Handle")] // 경사면 조절
    [SerializeField] private float maxslopeAngle;       // 최대 경사각도
    private RaycastHit slopeHit;                        // 경사로 레이캐스팅
    private bool exitingSlope;                          // 경사로 탈출

    [Header("Ground Check")] // 바닥 확인
    [SerializeField] private float playerHeight;        // 바닥인지 판단할 높이
    [SerializeField] private LayerMask whatIsGround;    // 바닥 레이어
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
        rb.freezeRotation = true;       // 물리로 인해 회전되지 않게 

        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        Move(); //기본 이동
    }

    private void Update()
    {
        // 바닥 확인
        isGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();          // 플레이어 입력
        SpeedControl();     // XZ 최고속도 조절
        StateHandler();     // 상태 변경

        // 바닥 저항
        if (isGround)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    

//--------------------------------------------------------------------------------------------
    private void StateHandler() // 상태 변경
    {
        if(isCrouch)                            // 움크리기
        {
            state = State.CROUCH;
            moveSpeed = crouchSpeed;
        }
        else if(isGround && Input.GetKey(runKey))    // 달리기
        {
            state = State.RUN;
            moveSpeed = runSpeed;          
        }
        else if(isGround)                       // 걷기
        {
            state = State.WALK;
            moveSpeed = walkSpeed;
        }
        else                                    // 공중
        {
            state = State.AIR;
        }
       
    }
   
    private void MyInput() // 플레이어 입력
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // 수평이동값 
        verticalInput = Input.GetAxisRaw("Vertical");       // 수직이동값
        
        // 준비가 되어있고 바닥일시 점프가능
        if (Input.GetKey(jumpKey) && readyToJump && isGround) 
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCoolDown); // 점프 재사용대기시간       
        }

        

        Crouch();

        /* // 탭 스프레이핑  보류
         if(Input.GetAxis("Mouse ScrollWheel") > 0f && !isGround)
         {
             Debug.Log("탭스");
             rb.AddForce(orientation.forward * moveSpeed, ForceMode.Impulse);
         }*/

    }

    private void Move() // 이동
    {
        // 이동 방향 계산
        moveDir = orientation.forward * verticalInput + orientation.right * horicaontalInput;

        // 경사면
        if (OnSlope() && !exitingSlope)
        {                
            rb.AddForce(GetSlopeMoveDir() * moveSpeed * 20f, ForceMode.Force); // 경사면에 맞는 방향으로 이동   

            if (rb.linearVelocity.y > 0) // 위로 튕기는현상
                rb.AddForce(Vector3.down*80f, ForceMode.Force); // 아래로 힘을부여 
        }
            
        
        // 바닥
        else if(isGround)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force); // Force모드로 연속적인 힘적용

        // 공중
        else if (!isGround)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force); // 공중 제어력 추가

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() // 최고속도 고정
    {
        if(OnSlope() && !exitingSlope) // 경사로인경우
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // 평지에서 속도

            if (flatVel.magnitude > moveSpeed) //XZ에서속도 > 최고속도
            {
                // 최고 속도로 고정
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        
    }

    private void Jump() // 점프
    {
        exitingSlope = true;

        // 점프시 Y속도 초기화
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);   //Inpulse모드로 한번에 힘적용
        
    }

    private void Crouch() // 움크리기
    {
        // 움크리기 토글
        if (Input.GetKeyDown(CrouchKeyToggle))
        {
            isCrouch = !isCrouch;
            if (isCrouch)
            {
                // Y크기 축소
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);  // 줄어든 크기만큼 아래로 이동
            }
            else
            {
                // Y크기 복구
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }

        // 움크리기 홀드
        if (Input.GetKeyDown(CrouchKeyHold))
        {
            isCrouch = true;
            // 홀드가 눌려있을 때는 토글 상태를 무시하고 크기 축소
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);  // 줄어든 크기만큼 아래로 이동
        }
        else if (Input.GetKeyUp(CrouchKeyHold))  // 토글 상태가 아니면 크기 복구
        {
            isCrouch = false;
            // Y크기 복구
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void ResetJump()    // 점프 조기화
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()  // 오를수 있는 경사면 판정
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight * 0.5f + 0.3f))
        {
            
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); //경사면에 각도 구하기
            return angle != 0 && angle < maxslopeAngle; 
        }
        return false;
    }

    private Vector3 GetSlopeMoveDir() // 경사면에서 방향
    {
        // 경사면 기울기 만큼 바닥면을 변경
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    //------------------getter , setter----------------------------

    public State States
    {
        get { return state; }
    }
    
}
