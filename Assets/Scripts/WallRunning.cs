using Unity.VisualScripting;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsGround;    // �ٴ� ���̾� 
    [SerializeField] private LayerMask whatIsWall;      // �� ���̾�
    [SerializeField] private float wallRunForce;        // ��Ÿ�� ��
    [SerializeField] private float wallJumpUpForce;     // ������ ���� ��
    [SerializeField] private float wallJumpSideForce;   // ��Ÿ�� ���� ��
    [SerializeField] private float wallClimbSpeed;      // ��Ÿ���� �����̵� �ӵ�
    [SerializeField] private float wallRunTime;         // ��Ÿ�� �ð�
    private float wallRunTimer;                         // Ÿ�̸�
    

    [Header("Input")]
    [SerializeField] private KeyCode wallJumpKey = KeyCode.Space;       // ������ Ű 
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.Q;      // ��Ÿ���� ������ �̵� Ű 
    [SerializeField] private KeyCode downwardsRunKey = KeyCode.E;    // ��Ÿ���� �Ʒ����� �̵� Ű
    private bool isUpwardsRun;
    private bool isDownwardsRun;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")] //������ 
    [SerializeField] private float wallCheckDis;        // �� �˻� �Ÿ�
    [SerializeField] private float minJumpHeight;       // �ּ� ���� ����
    private RaycastHit leftWallHit;                     // �޺� �����ɽ�Ʈ 
    private RaycastHit rightWallHit;                    // ������ �����ɽ�Ʈ 
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
        // �¿� �� �����ɽ���
        isLeftWall = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDis, whatIsWall);
        isRightWall = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDis, whatIsWall);
    }

    private bool AboveGround()
    {
        // �ٴڰ� �ּ� �Ÿ� ��ŭ ������ ������ true
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // �����̵� �Է�
        isUpwardsRun = Input.GetKey(upwardsRunKey);
        isDownwardsRun = Input.GetKey(downwardsRunKey);

        // ��Ÿ��
        if((isLeftWall || isRightWall) && verticalInput > 0  && AboveGround() && !exitingWall  )
        {         
            if (!pm.isWallRun) StartWallRun();

            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0 && pm.isWallRun)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(wallJumpKey)) WallJump(); // ������
        }
        else if (exitingWall) // ��Ÿ�Ⱑ ��ҵǴ� ���(������)
        {
            if (pm.isWallRun) StopWallRun();
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;
        }
        else // ��Ÿ�� ��
        {
            if (pm.isWallRun) StopWallRun();
        }
    }

    private void StartWallRun()
    {
        
        pm.isWallRun = true;

        wallRunTimer = wallRunTime; // ��Ÿ�� ���ӽð� �ʱ�ȭ
    }

    private void WallRunningMovement()  
    {
        //��Ÿ�� �ϴ� ������ �߷� ����X
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);


        // �浿�ϰ� �ִ� ���� �������� ����
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // ������ ���� ���� ������ ����
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // ��Ÿ�Ⱑ �ڷ� ���� ���� ���� 
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // �� ����
        rb.AddForce(wallForward * wallRunForce,ForceMode.Force);

        // ��Ÿ�� �����̵�
        if (isUpwardsRun) rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (isDownwardsRun) rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        // ��Ÿ�� ������ �߰�
        if (!(isLeftWall && horizontalInput > 0) && !(isRightWall && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRun = false;
    }

    private void WallJump()
    {
        // ��Ż��
        exitingWall = true;
        exitWallTimer = exitWallTime;   //Ż��ð� �ʱ�ȭ

        // ���� ���� ���� ����
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // �������� ����� ����
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // ������
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
