using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private float lifeTime = 3f;

    

    private void Update()
    {
        //Destroy(gameObject, lifeTime);

        Invoke(nameof(UnActive), lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Destroy(gameObject);
        UnActive();
    }

    private void UnActive()
    {
        gameObject.SetActive(false);
    }
}
