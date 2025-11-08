using UnityEngine;

public class LongRangeAttack : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 2f;
    public Vector2 direction = Vector2.right;

    private Transform visual; // 시각적 오브젝트 (자식 or 본인)

    void Start()
    {
        // 자식 오브젝트 중에서 SpriteRenderer 있는 거 찾기 (없으면 자기 자신)
        visual = GetComponentInChildren<SpriteRenderer>()?.transform ?? transform;

        // 🔥 방향에 따라 좌우 반전
        if (direction.x > 0)
        {
            visual.localRotation = Quaternion.Euler(0, 180f, -37f);  // 왼쪽 방향
        }
        else
        {
            visual.localRotation = Quaternion.Euler(0, 0, -37f);     // 오른쪽 방향
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 이동 방향은 그대로 유지
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var components = other.GetComponentsInParent<MonoBehaviour>();
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

        if (other.gameObject.layer == LayerMask.NameToLayer("flatform"))
        {
            Destroy(gameObject);
        }
    }
}
