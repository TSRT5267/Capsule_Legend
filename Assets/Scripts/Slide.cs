using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform orientation;     // ȸ��
    [SerializeField] private Transform playerObj;       // �÷��̾� ��ü
    private Rigidbody rb;
    private MovePlayer pm;

    [Header("Input")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;   // �����̵�Ű    
    private float horizontalInput;
    private float verticalInput;

    [Header("Slide")]
    [SerializeField] private float maxSlideTime;        // �ִ� �����̵� �ð�
    [SerializeField] private float slideForce;          // �����̵� ��
    [SerializeField] private float slideYScale;         // �����̵� ���¿��� Ű
    private float startYScale;
    private float slideTimer;
    

    

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        pm = GetComponent<MovePlayer>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))                 
            StartSlide();

        if(Input.GetKeyUp(slideKey)  && pm.isSlide )
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.isSlide) Sliding();
    }

    //-------------------------------------------------------------------------------------------------------

    private void StartSlide()
    {

        


        pm.isSlide = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z); //�����̵�� Y������ ���
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void Sliding()
    {
        Vector3 moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;    //�����̵� ������

        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f) // ������ �ƴѰ����� �����̵�
        {
            rb.AddForce(moveDir.normalized * slideForce, ForceMode.Force);  // �����̵� �� �ο�

            //�����̵� Ÿ�̸�
            slideTimer -= Time.deltaTime;
        }
        else // ���鿡�� �����̵�
        {
            rb.AddForce(pm.GetSlopeMoveDir(moveDir) * slideForce, ForceMode.Force);  // �����̵� �� �ο�
        }
 
        // ���ӽð� ������ �����̵� ����
        if (slideTimer <= 0) StopSlide();
    }

    private void StopSlide()
    {
        pm.isSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z); //�����̵� ����� Y������ ����
    }








}
