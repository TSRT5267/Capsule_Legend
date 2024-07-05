using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsGround;    // 바닥 레이어 
    [SerializeField] private LayerMask whatIsWall;      // 벽 레이어
    [SerializeField] private float wallRunForce;        // 벽타기 힘
    [SerializeField] private float wallRunTime;         // 벽타기 시간
    private float wallRunTimer;                         // 타이머

    [Header("Input")]
    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;

    [Header("Detection")] //벽감지 
    [SerializeField] private float wallCheckDis;        // 벽 검사 거리
    [SerializeField] private float minJumpHeight;       // 최소 점프 높이
    private RaycastHit leftWallHit;                     // 왼벽 레이케스트 
    private RaycastHit rightWallHit;                    // 오른벽 레이케스트 
    private bool isLeftWall;
    private bool isRightWall;

    [Header("Player")]
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

        // 벽타기
        if((isLeftWall || isRightWall) && verticalInput > 0  && AboveGround())
        {
            if (!pm.isWallRun) StartWallRun();
        }
        else
        {
            if (pm.isWallRun) StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.isWallRun = true;
    }

    private void WallRunningMovement()  
    {
        //벽타기 하는 동안은 중력 적용X
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);


        // 충동하고 있는 벽에 Normal 저장
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // 외적을 통해 전방 방향을 구함
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // 벽타기가 뒤로 가는 현상 수정 
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // 힘 적용
        rb.AddForce(wallForward * wallRunForce,ForceMode.Force);

        // 벽타기 접지력 추가
        if (!(isLeftWall && horizontalInput > 0) && !(isRightWall && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRun=false;
    }
}
