using UnityEngine;

public class LongRangeAttack : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 20;
    public float lifeTime = 2f;
    public Vector2 direction = Vector2.right;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var components = other.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var method = component.GetType().GetMethod("TakeDamage");
                if (method != null)
                {
                    method.Invoke(component, new object[] { damage });
                    break;
                }
            }
            Destroy(gameObject);
        }
    }
}