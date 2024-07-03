using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform orientation;     // 회전
    [SerializeField] private Transform playerObj;       // 플레이어 몸체
    private Rigidbody rb;
    private MovePlayer pm;

    [Header("Input")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;   // 슬라이드키    
    private float horizontalInput;
    private float verticalInput;

    [Header("Slide")]
    [SerializeField] private float maxSlideTime;        // 최대 슬라이드 시간
    [SerializeField] private float slideForce;          // 슬라이드 힘
    [SerializeField] private float slideYScale;         // 슬라이드 상태에서 키
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

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z); //슬라이드시 Y스케일 축소
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void Sliding()
    {
        Vector3 moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;    //슬라이드 방향계산

        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f) // 경사면이 아닌곳에서 슬라이드
        {
            rb.AddForce(moveDir.normalized * slideForce, ForceMode.Force);  // 슬라이드 힘 부여

            //슬라이드 타이머
            slideTimer -= Time.deltaTime;
        }
        else // 경사면에서 슬라이드
        {
            rb.AddForce(pm.GetSlopeMoveDir(moveDir) * slideForce, ForceMode.Force);  // 슬라이드 힘 부여
        }
 
        // 지속시간 끝난후 슬라이드 중지
        if (slideTimer <= 0) StopSlide();
    }

    private void StopSlide()
    {
        pm.isSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z); //슬라이드 종료시 Y스케일 복구
    }








}
