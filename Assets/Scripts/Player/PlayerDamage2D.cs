using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 피격/무적/넉백/HP UI 처리
/// </summary>
public class PlayerDamage2D : MonoBehaviour
{
    [Header("Refs")]
    private PlayerRefs refs;          // 공통 컴포넌트 모음
    // (필요하면 GameManager, SceneManager도 여기로 옮겨올 수 있음)

    [Header("HP 설정")]
    public int maxHp = 3;
    public int curHp;

    [Header("넉백 설정")]
    public float knockbackPower = 10f;      // 넉백 세기
    public float knockbackUpPower = 1f;     // 위쪽 힘 비율

    [Header("무적/깜박임 설정")]
    public float invincibleTime = 2f;       // 피격 후 무적 시간
    public float blinkInterval = 0.1f;      // 깜빡임 간격
    public int normalLayer = 10;           // 평소 레이어
    public int invincibleLayer = 11;       // 무적 레이어

    [Header("UI (HP 표시용 오브젝트들)")]
    public GameObject[] hpIcons;           // HP UI 아이콘들 (ex. 하트 3개)

    bool isInvincible = false;

    void Awake()
    {
        if (refs == null) refs = GetComponent<PlayerRefs>();
        curHp = maxHp;
        UpdateHpUI();
    }

    /// <summary>
    /// 적에게 맞았을 때 외부에서 호출하는 함수
    /// </summary>
    public void OnDamaged(Vector2 hitPos)
    {
        if (isInvincible) return;  // 무적 상태면 무시

        curHp--;
        UpdateHpUI();

        if (curHp <= 0)
        {
            Die();
            return;
        }

        // 넉백 + 무적 + 깜빡임 코루틴 시작
        StartCoroutine(DamageRoutine(hitPos));
    }

    IEnumerator DamageRoutine(Vector2 hitPos)
    {
        isInvincible = true;

        // 레이어 변경 (충돌 무시용)
        gameObject.layer = invincibleLayer;

        // 넉백 방향 계산 (적 기준 반대 방향으로 튕기기)
        int dir = transform.position.x - hitPos.x > 0 ? 1 : -1;

        refs.rigid.velocity = Vector2.zero;
        refs.rigid.AddForce(
            new Vector2(dir * knockbackPower, knockbackUpPower * knockbackPower),
            ForceMode2D.Impulse
        );

        // 깜빡임 + 무적 타이머
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibleTime)
        {
            visible = !visible;
            refs.spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 상태 복구
        refs.spriteRenderer.enabled = true;
        gameObject.layer = normalLayer;
        isInvincible = false;
    }

    void UpdateHpUI()
    {
        // 하트 3개라면 curHp 개수만큼만 켜고, 나머지는 끈다
        if (hpIcons == null || hpIcons.Length == 0) return;

        for (int i = 0; i < hpIcons.Length; i++)
        {
            if (hpIcons[i] == null) continue;
            hpIcons[i].SetActive(i < curHp);   // curHp보다 작은 인덱스만 활성화
        }
    }

    void Die()
    {
        // TODO: 사망 연출 + GameOver 처리
        // 예: 애니메이션 재생, 입력 막기, GameManager에 알리기 등
        Debug.Log("Player Dead");
    }
}
