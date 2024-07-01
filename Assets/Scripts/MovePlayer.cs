using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;

    private float horicaontalInput;
    private float verticalInput;
    private Vector3 moveDir;

    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;       // 물리로 인해 회전되지 않게 
    }

    private void MyInput()
    {
        horicaontalInput = Input.GetAxisRaw("Horizontal");  // 수평이동값 
        verticalInput = Input.GetAxisRaw("Vertical");       // 수직이동값
    }

    private void Update()
    {
        
    }
}
