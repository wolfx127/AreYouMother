using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 15f;
    [SerializeField] int damage = 1;
    Rigidbody rb; 
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Launch(Vector3 dir)
    {
        rb.linearVelocity = dir * speed;
        Invoke(nameof(Hide), 2f);
    }
    void Hide()
    {
        rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
       // other.GetComponent<>()?.TakeDamage(damage);
        Hide();
    }
}