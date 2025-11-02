using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ⚙️ 플레이어가 스테이지 이동, 리스폰, 데미지, 사망 등의 이벤트를 처리하는 스크립트
public class PlayerSceneTrigger : MonoBehaviour
{
    Rigidbody2D rigid;                         // 플레이어 물리 제어용 Rigidbody2D
    public SceneManager sceneManager;          // 스테이지 관리 스크립트 참조
    SpriteRenderer spriteRender;               // 플레이어 스프라이트(색상 변경용)
    public GameManager gameManager;            // 전체 게임 상태 관리용 스크립트 참조
    public smallEnemyDash dashScript;          // 충돌한 적의 이동 스크립트 참조 (플레이어 사망 시 적 동작 제어용)

    void Awake()
    {
        // 컴포넌트 캐싱 (성능 최적화)
        rigid = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 현재는 매 프레임 처리할 내용 없음
    }

    // 🧩 트리거(Trigger Collider)에 진입했을 때 실행
    void OnTriggerEnter2D(Collider2D collision)
    {
        // MoveStage 태그에 닿으면 다음 스테이지로 이동
        if (collision.gameObject.tag == "MoveStage")
        {
            // 충돌한 오브젝트 이름을 숫자로 변환하여 스테이지 번호로 사용
            int stagenum = int.Parse(collision.gameObject.name);
            sceneManager.NextStage(stagenum);  // 스테이지 전환
        } 
        
        // RespawnTag 태그에 닿으면 리스폰 위치 갱신
        if (collision.gameObject.tag == "RespawnTag")
        {
            sceneManager.SetRespawn(collision.transform.position);
        }
    }

    // 💥 물리 충돌(Collision) 발생 시 실행
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy 태그를 가진 오브젝트에 부딪히면 데미지 처리
        if (collision.gameObject.tag == "Enemy")
        {
            // 충돌한 적의 smallEnemyDash 스크립트를 가져옴
            dashScript = collision.gameObject.GetComponent<smallEnemyDash>();
            // 플레이어가 공격받았을 때 데미지 처리
            OnDamaged(collision.transform.position);
        }
    }

    // 🔹 플레이어 속도를 0으로 만드는 함수 (정지 상태 유지용)
    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    // ❤️ 데미지 입었을 때 호출되는 함수
    void OnDamaged(Vector2 targetPos)
    {
        // 체력 감소
        gameManager.HealthDown();

        // 무적 상태를 위해 레이어 변경 (충돌 무시)
        gameObject.layer = 11;

        // 시각적으로 투명하게 (피격 효과)
        spriteRender.color = new Color(1, 1, 1, 0.4f);

        // 💨 반대 방향으로 넉백(튕겨나감)
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1; // 맞은 방향 반대쪽으로 밀림
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        // 일정 시간 후 무적 해제
        Invoke("OffDamaged", 3);
    }

    // 💡 무적 상태 해제 및 색상 복원
    void OffDamaged()
    {
        gameObject.layer = 10;                         // 원래 레이어 복귀
        spriteRender.color = new Color(1, 1, 1, 1);    // 불투명 복귀
    }

    // ☠️ 사망 처리 함수
    public void OnDie()
    {
        // 색상 반투명 처리 (사망 표시)
        spriteRender.color = new Color(1, 1, 1, 0.4f);

        // 사망 애니메이션용 코드 (현재 주석 처리됨)
        // spriteRender.flipY = true;           // 뒤집기 효과
        // colli.enabled = false;              // 충돌 비활성화
        // rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse); // 점프 효과
    }

    // 🌀 소형 적이 플레이어를 죽였을 때 이동 재개 함수
    public void SmallEnemyMoveReset()
    {
        dashScript.StopDash(); // 적의 대시 멈춤 해제 → 다시 이동 가능하게
    }
}
