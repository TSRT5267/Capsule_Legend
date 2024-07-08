using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Player")]  // 플레이어
    [SerializeField] private State state;   // 상태
    [SerializeField] private Climbing climbingScript; 


    [Header("Input")] // 키설정
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;               // 점프키 할당
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;            // 달리기키 할당
    [SerializeField] private KeyCode CrouchKey = KeyCode.LeftControl;   // 움크리기키 할당
    private float horizontalInput;
    private float verticalInput;

    [Header("Movement")] // 이동
    [SerializeField] private float walkSpeed;           // 걷기속도
    [SerializeField] private float runSpeed;            // 뛰기속도
    [SerializeField] private float slideSpeed;          // 슬라이드속도                                                    
    [SerializeField] private float wallRunSpeed;        // 벽타기 속도
    [SerializeField] private float climbSpeed;        // 벽오르기 속도                                                
    [SerializeField] private float groundDrag;          // 바닥 저항
    [SerializeField] private float speedIncreaseMultiplier;// 속도 증가 배수
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

    [Header("Slope")] // 경사면 이동
    [SerializeField] private float maxslopeAngle;       // 최대 경사각도
    [SerializeField] private float slopeIncreaseMultiplier;// 경사로속도 증가 배수
    private RaycastHit slopeHit;                        // 경사로 레이캐스팅
    private bool exitingSlope;                          // 경사로 탈출

    [Header("Slide")]    
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    
    [Header("Ground Check")] // 바닥 확인
    [SerializeField] private float playerHeight;        // 바닥인지 판단할 높이
    [SerializeField] private LayerMask whatIsGround;    // 바닥 레이어
    
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
        if(isClimb) // 벽 오르기
        {
            state = State.CLIMB;
            desiredMoveSpeed = climbSpeed;
        }
        else if(isWallRun) // 벽타기
        {
            state = State.WALLRUN;
            desiredMoveSpeed = wallRunSpeed;
        }
        else if(isSlide) // 슬라이드
        {
            state = State.SLIDE;

            if (OnSlope() && rb.linearVelocity.y < 0.1f) 
                desiredMoveSpeed = slideSpeed;
            else 
                desiredMoveSpeed = runSpeed;
        }
        else if(Input.GetKey(CrouchKey)) // 움크리기
        {
            state = State.CROUCH;
            desiredMoveSpeed = crouchSpeed;
        }
        else if(isGround && Input.GetKey(runKey))    // 달리기
        {
            state = State.RUN;
            desiredMoveSpeed = runSpeed;          
        }
        else if(isGround)                       // 걷기
        {
            state = State.WALK;
            desiredMoveSpeed = walkSpeed;
        }
        else                                    // 공중
        {
            state = State.AIR;
        }

        // 변화해야할 속도가 4이상인 경우 부르럽게 속도 변경
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            
        }
        else // 일반적인 달리기 같은 경우는 즉각적인 속도 변경
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;


        if (state == State.CROUCH) isCrounch = true;
        else isCrounch = false;

    }

    private IEnumerator SmoothlyLerpMoveSpeed() // moveSpeed를 desiredMoveSpeed 부드럽게 변경
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
   
    private void MyInput() // 플레이어 입력
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");  // 수평이동값 
        verticalInput = Input.GetAxisRaw("Vertical");       // 수직이동값
        
        // 준비가 되어있고 바닥일시 점프가능
        if (Input.GetKey(jumpKey) && readyToJump && isGround) 
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCoolDown); // 점프 재사용대기시간       
        }

        // 움크리기 시작
        if(Input.GetKeyDown(CrouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); //슬라이드시 Y스케일 축소
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        // 움크리기 중단
        if(Input.GetKeyUp(CrouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z); //슬라이드시 Y스케일 복구


        /* // 탭 스프레이핑  보류
         if(Input.GetAxis("Mouse ScrollWheel") > 0f && !isGround)
         {
             Debug.Log("탭스");
             rb.AddForce(orientation.forward * moveSpeed, ForceMode.Impulse);
         }*/

    }

    private void Move() // 이동
    {
        if (climbingScript.ExitingWall) return;     //벽오르는 중 벽에서 탈출시 행동정지

        // 이동 방향 계산
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // 경사면
        if (OnSlope() && !exitingSlope)
        {                
            rb.AddForce(GetSlopeMoveDir(moveDir) * moveSpeed * 20f, ForceMode.Force); // 경사면에 맞는 방향으로 이동   

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


        if (rb.linearVelocity.magnitude <= 1f && moveSpeed >= 10f)
        {
            StopAllCoroutines();
            moveSpeed = 7.0f;
        }
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

    private void ResetJump()    // 점프 조기화
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()  // 오를수 있는 경사면 판정
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight * 0.5f + 0.3f))
        {
            
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); //경사면에 각도 구하기
            return angle != 0 && angle < maxslopeAngle; 
        }
        return false;
    }

    public Vector3 GetSlopeMoveDir(Vector3 dir) // 경사면에서 방향
    {
        // 경사면 기울기 만큼 바닥면을 변경
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
