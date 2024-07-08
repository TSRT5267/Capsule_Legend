using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private MovePlayer pm;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed;      //�������� ���ǵ�
    [SerializeField] private float maxClimbTime;    //�������� �ִ�ð�
    private float climbTimer;
    private bool isClimb;

    [Header("ClimbJump")]
    [SerializeField] private float climbJumpUpForce;        //�������� ���� ���ʹ��� ��
    [SerializeField] private float climbJumpBackForce;      //�������� ���� ���ʹ��� ��
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;    //�������� ����Ű
    [SerializeField] private int climbJumps;                //�������� ���� Ƚ�� 
    private int climbJumpsLeft;

    [Header("Exiting")]
    [SerializeField] private bool exitingWall;          // �� Ż�� 
    [SerializeField] private float exitWallTime;          // �� Ż�� �ð�
    private float exitWallTimer;

    [Header("Detection")]
    [SerializeField] private float detectionLength;     // �����Ÿ�
    [SerializeField] private float sphereCastRadius;    // ���Ǿ�ĳ��Ʈ ������
    [SerializeField] private float maxWallLookAngle;    // �ִ�þ� ����
    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField] private float minWallNormalAngleChange;    // ���ο� ���� �ּ� ����

    private void Update()
    {
        WallCheck();
        StateMachine();

        if(isClimb && !exitingWall) ClimbingMovement();

    }

    //------------------------------------------------------------------------

    private void StateMachine()
    {
        // �������� ����
        if(wallFront && Input.GetKey(KeyCode.W)&& wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if(!isClimb && climbTimer > 0) StartClimbing();

            // Ÿ�̸�
            if(climbTimer > 0 ) climbTimer -= Time.deltaTime;
            if(climbTimer < 0 ) StopClimbing();    
        }
        else if(exitingWall) // �� Ż��
        {
            if (isClimb) StopClimbing();
            
            //Ÿ�̸�
            if(exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if(exitWallTimer < 0) exitingWall = false;
        }
        else // �������� ����
        {
            if (isClimb) StopClimbing();
        }


        // �� ����
        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }


    private void WallCheck()
    {
        //���鿡 ���� �ִµ� ���Ǿ�ĳ��Ʈ�� ���� äũ
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward,
            out frontWallHit, detectionLength, whatIsWall);  

        //���Ǿ�ĳ��Ʈ�� ����� ���� �ٶ󺸴� ���� ���
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        //                          ���ο� ��                           ���� �������� �ٸ� ����
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

        // ���� �ִ� �� ����
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        // �������� �� �ӵ� ����
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

        // ������ ���� ���
        Vector3 forceToApply = transform.up *climbJumpUpForce + frontWallHit.normal*climbJumpBackForce;

        // ���� �� �ο�
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(forceToApply,ForceMode.Impulse);

        climbJumpsLeft--; //���� Ƚ�� -
    }

    public bool ExitingWall
    {
        get { return exitingWall; }
    }

}
