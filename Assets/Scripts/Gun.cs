using UnityEngine;

public class Gun : MonoBehaviour
{

    [Header("ReFerences")]
    [SerializeField] Transform Cam;
    [SerializeField] Transform gunTip;          //총알발사 위치
    [SerializeField] GameObject bulletPrefab;   

    [Header("Setting")]
    [SerializeField] private int MaxBullet;
    [SerializeField] private float fireCooldown;
    [SerializeField] private float reloadCooldown;
    private int totalBullet;

    [Header("Fire")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private float fireForce;
    private bool readyToFire;
    private bool isReload;

    //---------------------------------------------------

    private void Start()
    {
        readyToFire = true;
        totalBullet = MaxBullet;
    }


    private void Update()
    {
        if (Input.GetKey(fireKey) && readyToFire && totalBullet > 0)
        {
            Fire();
        }  
        
        if(totalBullet <=0 && !isReload)
        {
            isReload = true;
            Invoke(nameof(Reload), reloadCooldown);
        }

    }

    private void Fire()
    {
        readyToFire = false;

        //총알 설정
        GameObject projectile = Instantiate(bulletPrefab, gunTip.position, gunTip.rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        
        // 발사 방향 계산
        Vector3 forceDir = Cam.transform.forward;
        RaycastHit hit;
        if(Physics.Raycast(Cam.position,Cam.forward,out hit,500f))
        {
            forceDir = (hit.point - gunTip.position).normalized;
        }

        // 발사 백터
        Vector3 forceToAdd = forceDir * fireForce;
        
        // 백터 적용
        projectileRb.AddForce(forceToAdd,ForceMode.Impulse);

        totalBullet--; // 총 총알 갯수--

        Invoke(nameof(ResetFire), fireCooldown);
    }

    private void ResetFire()
    {
        readyToFire = true; 
    }

    private void Reload()
    {
        isReload = false;
        totalBullet = MaxBullet;
    }
}
