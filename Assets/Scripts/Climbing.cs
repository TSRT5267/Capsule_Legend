using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private MovePlayer pm;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed;      //벽오르기 스피드
    [SerializeField] private float maxClimbTime;    //벽오르기 최대시간
    private float climbTimer;
    private bool isClimb;

    [Header("ClimbJump")]
    [SerializeField] private float climbJumpUpForce;        //벽오르기 점프 위쪽방향 힘
    [SerializeField] private float climbJumpBackForce;      //벽오르기 점프 뒤쪽방향 힘
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;    //벽오르기 점프키
    [SerializeField] private int climbJumps;                //벽오르기 점프 횟수 
    private int climbJumpsLeft;

    [Header("Exiting")]
    [SerializeField] private bool exitingWall;          // 벽 탈출 
    [SerializeField] private float exitWallTime;          // 벽 탈출 시간
    private float exitWallTimer;

    [Header("Detection")]
    [SerializeField] private float detectionLength;     // 감지거리
    [SerializeField] private float sphereCastRadius;    // 스피어캐스트 반지름
    [SerializeField] private float maxWallLookAngle;    // 최대시야 각도
    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField] private float minWallNormalAngleChange;    // 새로운 벽의 최소 각도

    private void Update()
    {
        WallCheck();
        StateMachine();

        if(isClimb && !exitingWall) ClimbingMovement();

    }

    //------------------------------------------------------------------------

    private void StateMachine()
    {
        // 벽오르기 시작
        if(wallFront && Input.GetKey(KeyCode.W)&& wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if(!isClimb && climbTimer > 0) StartClimbing();

            // 타이머
            if(climbTimer > 0 ) climbTimer -= Time.deltaTime;
            if(climbTimer < 0 ) StopClimbing();    
        }
        else if(exitingWall) // 벽 탈출
        {
            if (isClimb) StopClimbing();
            
            //타이머
            if(exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if(exitWallTimer < 0) exitingWall = false;
        }
        else // 벽오르기 중지
        {
            if (isClimb) StopClimbing();
        }


        // 벽 점프
        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }


    private void WallCheck()
    {
        //정면에 벽이 있는데 스피어캐스트를 통해 채크
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward,
            out frontWallHit, detectionLength, whatIsWall);  

        //스피어캐스트가 실행시 벽을 바라보는 각도 계산
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        //                          새로운 벽                           같은 벽이지만 다른 각도
        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if (pm.IsGround || (wallFront && newWall))
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        isClimb = true;
        pm.isClimb = true;

        // 전에 있던 벽 저장
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        // 벽오르기 시 속도 제어
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
    }

    private void StopClimbing()
    {
        isClimb = false;
        pm.isClimb = false;
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        // 점프할 백터 계산
        Vector3 forceToApply = transform.up *climbJumpUpForce + frontWallHit.normal*climbJumpBackForce;

        // 점프 힘 부여
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(forceToApply,ForceMode.Impulse);

        climbJumpsLeft--; //점프 횟수 -
    }

    public bool ExitingWall
    {
        get { return exitingWall; }
    }

}
