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
    [SerializeField] private float maxGrappleDis;       // �ִ� �׷��� �Ÿ�
    [SerializeField] private float grappleDelayTime;    // �׷��� ������
    [SerializeField] private float overshootYAxis;      // ������ �̵��� �����ϱ� ���� ���� y��
    private Vector3 grapplePoint;                       // �׷��� ��������

    [Header("Prediction")]
    [SerializeField] private RaycastHit predictionHit;          // ����������hit
    [SerializeField] private float predictionSphereCastRadius;  // ���Ǿ�ĳ��Ʈ ������
    [SerializeField] private Transform predictionPoint;         // ���� ��ġ

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

        // ���� ������ Ȯ��
        CheckForPoint();
        // Ÿ�̸�
        if(grappleCdTimer > 0 ) grappleCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (isGrapple) lr.SetPosition(0, gunTip.position); // �׷����� ���� ������ �ѳ�
    }

    //----------------------------------------------------------------------

    private void StartGrapple()     //����
    {
        if (grappleCdTimer > 0) return;

        isGrapple = true;
        pm.isFreeze = true;

        
        if(predictionHit.point != Vector3.zero)
        {
            RaycastHit hit = predictionHit;
            if(hit.transform.tag != "OutsideWall") //�ܺ��� �׷��� ����
            {
                grapplePoint = hit.point;
                Invoke(nameof(ExecuteGrapple), grappleDelayTime);   // �����ð��� �׷����۵�
            }
            else
            {
                grapplePoint = cam.position + cam.forward * maxGrappleDis;
                Invoke(nameof(StopGrapple), grappleDelayTime);      // �����ð��� ����
            }
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDis;
            Invoke(nameof(StopGrapple), grappleDelayTime);      // �����ð��� ����
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);    // �׷��� �������� �ٷ� ����
    }

    private void ExecuteGrapple()   //����
    {
        pm.isFreeze = false;

        // �÷��̾� ��ġ�� ���� ������ ����
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y; // �׷��� �� ������ y ��
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis; //�������� ��������� y���� �߰��� ����

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis; //�÷��̾� ���� �Ʒ� �������� �׷��ý� overshootYAxis�� ���

        pm.JumpToPosition(grapplePoint, highestPointOnArc); //�÷��̾ ����

        Invoke(nameof(StopGrapple), 1f); //1�� �� ����
    }

    public void StopGrapple()      //����
    {
        pm.isFreeze = false;
        isGrapple = false;
        lr.enabled = false;

        grappleCdTimer = grappleCd;     
    }

    private void CheckForPoint()
    {
        if (isGrapple) return; //�׷��� ���̸� �۵� X

        RaycastHit sphereCastHit;   //���Ǿ�ĳ��Ʈ
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxGrappleDis, whatIsGrappleable);

        RaycastHit rayCastHit;      //����ĳ��Ʈ
        Physics.Raycast(cam.position, cam.forward,
            out rayCastHit, maxGrappleDis, whatIsGrappleable);

        Vector3 realHitPoint;

        // �켱����
        if(rayCastHit.point != Vector3.zero) realHitPoint = rayCastHit.point;
        else if(sphereCastHit.point != Vector3.zero) realHitPoint = sphereCastHit.point;
        else realHitPoint = Vector3.zero;

        if(realHitPoint != Vector3.zero) //���� �������� ������ Ȱ��ȭ
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else//�ƴϸ� ��Ȱ��ȭ
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = rayCastHit.point == Vector3.zero ? sphereCastHit : rayCastHit;

    }

    









}












