using Unity.VisualScripting;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsGround;    // 바닥 레이어 
    [SerializeField] private LayerMask whatIsWall;      // 벽 레이어
    [SerializeField] private float wallRunForce;        // 벽타기 힘
    [SerializeField] private float wallJumpUpForce;     // 벽점프 위쪽 힘
    [SerializeField] private float wallJumpSideForce;   // 벽타기 측면 힘
    [SerializeField] private float wallClimbSpeed;      // 벽타기중 상하이동 속도
    [SerializeField] private float wallRunTime;         // 벽타기 시간
    private float wallRunTimer;                         // 타이머
    

    [Header("Input")]
    [SerializeField] private KeyCode wallJumpKey = KeyCode.Space;       // 벽점프 키 
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.Q;      // 벽타기중 위방향 이동 키 
    [SerializeField] private KeyCode downwardsRunKey = KeyCode.E;    // 벽타기중 아래방향 이동 키
    private bool isUpwardsRun;
    private bool isDownwardsRun;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")] //벽감지 
    [SerializeField] private float wallCheckDis;        // 벽 검사 거리
    [SerializeField] private float minJumpHeight;       // 최소 점프 높이
    private RaycastHit leftWallHit;                     // 왼벽 레이케스트 
    private RaycastHit rightWallHit;                    // 오른벽 레이케스트 
    private bool isLeftWall;
    private bool isRightWall;

    [Header("Exiting")]
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;
    private bool exitingWall;

    [Header("ReFerences")]
    [SerializeField] private Transform orientation;     
    private MovePlayer pm;             
    private Rigidbody rb;

    private void Start()
    {
        pm = GetComponent<MovePlayer>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.isWallRun) WallRunningMovement();
    }

    //---------------------------------------------------------------------

    private void CheckForWall()
    {
        // 좌우 벽 레이케스팅
        isLeftWall = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDis, whatIsWall);
        isRightWall = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDis, whatIsWall);
    }

    private bool AboveGround()
    {
        // 바닥과 최소 거리 만큼 떨어저 있으면 true
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 상하이동 입력
        isUpwardsRun = Input.GetKey(upwardsRunKey);
        isDownwardsRun = Input.GetKey(downwardsRunKey);

        // 벽타기
        if((isLeftWall || isRightWall) && verticalInput > 0  && AboveGround() && !exitingWall  )
        {         
            if (!pm.isWallRun) StartWallRun();

            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0 && pm.isWallRun)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(wallJumpKey)) WallJump(); // 벽점프
        }
        else if (exitingWall) // 벽타기가 취소되는 경우(벽점프)
        {
            if (pm.isWallRun) StopWallRun();
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;
        }
        else // 벽타기 끝
        {
            if (pm.isWallRun) StopWallRun();
        }
    }

    private void StartWallRun()
    {
        
        pm.isWallRun = true;

        wallRunTimer = wallRunTime; // 벽타기 지속시간 초기화
    }

    private void WallRunningMovement()  
    {
        //벽타기 하는 동안은 중력 적용X
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);


        // 충동하고 있는 벽에 법선백터 저장
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // 외적을 통해 전방 방향을 구함
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // 벽타기가 뒤로 가는 현상 수정 
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // 힘 적용
        rb.AddForce(wallForward * wallRunForce,ForceMode.Force);

        // 벽타기 상하이동
        if (isUpwardsRun) rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (isDownwardsRun) rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        // 벽타기 접지력 추가
        if (!(isLeftWall && horizontalInput > 0) && !(isRightWall && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRun = false;
    }

    private void WallJump()
    {
        // 벽탈출
        exitingWall = true;
        exitWallTimer = exitWallTime;   //탈출시간 초기화

        // 벽에 법선 백터 저장
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // 벽점프시 적용될 백터
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // 벽점프
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
