using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("ReFerences")]
    [SerializeField] private MovePlayer pm;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask whatIsGrappleable;
    [SerializeField] private LineRenderer lr;

    [Header("Grappling")]
    [SerializeField] private float maxGrappleDis;       // 최대 그래플 거리
    [SerializeField] private float grappleDelayTime;    // 그래플 딜레이
    [SerializeField] private float overshootYAxis;      // 포물선 이동에 구현하기 위해 더할 y값
    private Vector3 grapplePoint;                       // 그래플 도착지점

    [Header("Prediction")]
    [SerializeField] private RaycastHit predictionHit;          // 예상조준점hit
    [SerializeField] private float predictionSphereCastRadius;  // 스피어캐스트 반지름
    [SerializeField] private Transform predictionPoint;         // 예상 위치

    [Header("Cooldown")]
    [SerializeField] private float grappleCd;
    private float grappleCdTimer;

    [Header("Input")]
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse1;
    private bool isGrapple;
    



    //---------------------------------------------------------------------

    private void Start()
    {
        pm = GetComponent<MovePlayer>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(grappleKey)) StartGrapple();

        // 예상 조준점 확인
        CheckForPoint();
        // 타이머
        if(grappleCdTimer > 0 ) grappleCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (isGrapple) lr.SetPosition(0, gunTip.position); // 그래플의 줄의 시작은 총끝
    }

    //----------------------------------------------------------------------

    private void StartGrapple()     //시작
    {
        if (grappleCdTimer > 0) return;

        isGrapple = true;
        pm.isFreeze = true;

        
        if(predictionHit.point != Vector3.zero)
        {
            RaycastHit hit = predictionHit;
            if(hit.transform.tag != "OutsideWall") //외벽은 그래플 금지
            {
                grapplePoint = hit.point;
                Invoke(nameof(ExecuteGrapple), grappleDelayTime);   // 일정시간후 그래플작동
            }
            else
            {
                grapplePoint = cam.position + cam.forward * maxGrappleDis;
                Invoke(nameof(StopGrapple), grappleDelayTime);      // 일정시간후 정지
            }
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDis;
            Invoke(nameof(StopGrapple), grappleDelayTime);      // 일정시간후 정지
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);    // 그래플 도착지점 줄로 연결
    }

    private void ExecuteGrapple()   //실행
    {
        pm.isFreeze = false;

        // 플레이어 위치를 가장 낮은점 설정
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y; // 그래플 두 지점에 y 차
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis; //포물선을 만들기위에 y값을 추가로 더함

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis; //플레이어 보다 아래 방향으로 그래플시 overshootYAxis만 사용

        pm.JumpToPosition(grapplePoint, highestPointOnArc); //플레이어에 적용

        Invoke(nameof(StopGrapple), 1f); //1초 후 정지
    }

    public void StopGrapple()      //정지
    {
        pm.isFreeze = false;
        isGrapple = false;
        lr.enabled = false;

        grappleCdTimer = grappleCd;     
    }

    private void CheckForPoint()
    {
        if (isGrapple) return; //그래플 중이면 작동 X

        RaycastHit sphereCastHit;   //스피어캐스트
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxGrappleDis, whatIsGrappleable);

        RaycastHit rayCastHit;      //레이캐스트
        Physics.Raycast(cam.position, cam.forward,
            out rayCastHit, maxGrappleDis, whatIsGrappleable);

        Vector3 realHitPoint;

        // 우선순위
        if(rayCastHit.point != Vector3.zero) realHitPoint = rayCastHit.point;
        else if(sphereCastHit.point != Vector3.zero) realHitPoint = sphereCastHit.point;
        else realHitPoint = Vector3.zero;

        if(realHitPoint != Vector3.zero) //예상 조준점이 있으면 활성화
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else//아니면 비활성화
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = rayCastHit.point == Vector3.zero ? sphereCastHit : rayCastHit;

    }

    









}












