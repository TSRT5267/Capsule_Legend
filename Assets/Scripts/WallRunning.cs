using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsGround;    // �ٴ� ���̾� 
    [SerializeField] private LayerMask whatIsWall;      // �� ���̾�
    [SerializeField] private float wallRunForce;        // ��Ÿ�� ��
    [SerializeField] private float wallRunTime;         // ��Ÿ�� �ð�
    private float wallRunTimer;                         // Ÿ�̸�

    [Header("Input")]
    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;

    [Header("Detection")] //������ 
    [SerializeField] private float wallCheckDis;        // �� �˻� �Ÿ�
    [SerializeField] private float minJumpHeight;       // �ּ� ���� ����
    private RaycastHit leftWallHit;                     // �޺� �����ɽ�Ʈ 
    private RaycastHit rightWallHit;                    // ������ �����ɽ�Ʈ 
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

        // ��Ÿ��
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
        //��Ÿ�� �ϴ� ������ �߷� ����X
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);


        // �浿�ϰ� �ִ� ���� Normal ����
        Vector3 wallNormal = isRightWall ? rightWallHit.normal : leftWallHit.normal;

        // ������ ���� ���� ������ ����
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // ��Ÿ�Ⱑ �ڷ� ���� ���� ���� 
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // �� ����
        rb.AddForce(wallForward * wallRunForce,ForceMode.Force);

        // ��Ÿ�� ������ �߰�
        if (!(isLeftWall && horizontalInput > 0) && !(isRightWall && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRun=false;
    }
}
