using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatterns : MonoBehaviour
{
    // 어떤 패턴이든 실행 중이면 true
    public bool IsActing { get; private set; } = false;

    // ============ [패턴 1] 주기적으로 몬스터 소환 ============
    [Header("패턴 1 - 몬스터 소환")]
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxMinionCount = 5;

    private readonly List<GameObject> spawnedMinions = new List<GameObject>();

    // ============ [패턴 2] 점프 후 제자리 찍기 ============
    [Header("패턴 2 - 점프 찍기")]
    public float jumpForce = 700f;
    public float slamForce = 1400f;
    public float airWait = 0.5f;
    public float afterSlamHitTime = 0.5f;
    public float afterSlamWait = 1f;
    public AudioSource jumpClip;
    public AudioSource slamClip;

    // ============ [패턴 3] 브레스 ============
    [Header("패턴 3 - 브레스")]
    public GameObject breathPrefab;
    public Transform breathPort;
    public float breathWindup = 1f;
    public float breathDuration = 4f;
    public AudioSource breathClip;

    GameObject currentBreath;

    // ---------------------------------------------------
    // 패턴 1: 몬스터 소환 루프
    // ---------------------------------------------------
    public IEnumerator SpawnLoop(BossController boss)
    {
        while (!boss.IsDead)
        {
            // 죽은 미니언 리스트에서 제거
            spawnedMinions.RemoveAll(m => m == null);

            if (minionPrefab != null &&
                spawnPoints != null &&
                spawnPoints.Length > 0 &&
                spawnedMinions.Count < maxMinionCount)
            {
                Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject m = Instantiate(minionPrefab, p.position, Quaternion.identity);
                spawnedMinions.Add(m);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ---------------------------------------------------
    // 패턴 2: 점프 후 제자리 찍기
    // ---------------------------------------------------
    public IEnumerator JumpSlam(BossController boss)
    {
        if (IsActing) yield break;
        IsActing = true;

        var rigid = boss.Rigid;
        var anim  = boss.Anim;

        // 수평 속도 제거
        rigid.velocity = Vector2.zero;

        // 점프 준비
        anim.SetTrigger("doTaunt");
        if (jumpClip != null) jumpClip.Play();

        // 위로 점프
        rigid.AddForce(Vector2.up * jumpForce);
        yield return new WaitForSeconds(airWait);

        // 잠깐 공중 정지
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0f;
        yield return new WaitForSeconds(0.2f);

        // 급강하
        rigid.gravityScale = 1f;
        rigid.AddForce(Vector2.down * slamForce);

        // 착지 연출
        if (slamClip != null) slamClip.Play();
        if (boss.cameraShake != null)
            boss.cameraShake.Shake(0.5f, 0.05f);

        // 이 타이밍에 meleeArea를 켜서 플레이어에게 데미지 (BossController에서 처리)
        if (boss.meleeArea != null)
        {
            boss.meleeArea.enabled = true;
            yield return new WaitForSeconds(afterSlamHitTime);
            boss.meleeArea.enabled = false;
        }

        // 후딜
        yield return new WaitForSeconds(afterSlamWait);

        IsActing = false;
    }

    // ---------------------------------------------------
    // 패턴 3: 브레스
    // ---------------------------------------------------
    public IEnumerator Breath(BossController boss)
    {
        if (IsActing) yield break;
        IsActing = true;

        var rigid = boss.Rigid;
        var anim  = boss.Anim;

        // 제자리에서 브레스만, 수평 속도 제거
        rigid.velocity = Vector2.zero;

        // 플레이어 방향으로 바라보게 회전
        if (boss.target != null)
        {
            float dirX = boss.target.position.x - boss.transform.position.x;
            boss.transform.rotation = Quaternion.Euler(0, dirX < 0 ? 0 : 180, 0);
        }

        // 바람 모으기 애니메이션
        anim.SetTrigger("doBreath");
        if (breathClip != null) breathClip.Play();
        yield return new WaitForSeconds(breathWindup);

        // 브레스 생성
        if (breathPrefab != null && breathPort != null)
        {
            currentBreath = Instantiate(breathPrefab, breathPort.position, breathPort.rotation);
        }

        // 카메라 흔들림
        if (boss.cameraShake != null)
            boss.cameraShake.Shake(breathDuration, 0.01f);

        // 유지 시간
        yield return new WaitForSeconds(breathDuration);

        // 브레스 종료
        if (currentBreath != null)
            Destroy(currentBreath);

        anim.SetTrigger("endBreath");

        IsActing = false;
    }
}
