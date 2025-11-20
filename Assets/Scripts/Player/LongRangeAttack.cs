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
            // 오른쪽
            visual.localRotation = Quaternion.Euler(0, 180f, -37f);
        }
        else
        {
            // 왼쪽
            visual.localRotation = Quaternion.Euler(0, 0, -37f);
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
        // 1️⃣ 먼저 소형 적(SmallEnemyController)인지 확인
        SmallEnemyController small = other.GetComponentInParent<SmallEnemyController>();
        if (small != null)
        {
            // 원거리 공격 → melee = false (노카운트)
            small.TakeDamage(damage, false);
            Destroy(gameObject);
            return;
        }

        // 2️⃣ 그 외 Enemy 태그를 가진 적들 처리 (기존 제너릭 로직 유지)
        if (other.CompareTag("Enemy"))
        {
            var components = other.GetComponentsInParent<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null)        // 🔴 Missing Script 같은 null 컴포넌트 스킵
                    continue;

                var method = component.GetType().GetMethod("TakeDamage");
                if (method != null)
                {
                    method.Invoke(component, new object[] { damage });
                    break;
                }
            }

            Destroy(gameObject);
            return;
        }


        // 3️⃣ 바닥(플랫폼)에 부딪히면 삭제
        if (other.gameObject.layer == LayerMask.NameToLayer("flatform"))
        {
            Destroy(gameObject);
        }
    }
}
