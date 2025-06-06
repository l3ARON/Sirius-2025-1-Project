using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Boss : MonoBehaviour
{
    public enum Type { D };
    public Type enemyType;
    public int maxHealth = 100;
    public int curHealth;
    public Transform target;
    public BoxCollider2D meleeArea;
    //보스패턴 매직 캐스팅을 넣으면 재밌을 듯
    public float moveSpeed;

    Rigidbody2D rigid;
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    Animator anim;

    public GameObject breath;
    public Transform breathPort;
    //플레이어 찍을 위치 저장 변수
    //탐지 범위
    public float outlineDetec = 7f;
    public float middleDetec = 5f;
    public float inlineDetec = 3f;
    public bool isDead = false;

    bool isChase = false;
    bool isDetec = false;
    CameraShake cameraS;
    Coroutine thinkRoutine;

    GameObject instanceBreath;
    bool isInvincible = false;

    void Start()
    {
        cameraS = GetComponent<CameraShake>();
        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        //플레이어 추적
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        curHealth = maxHealth;
        Invoke("ChaseStart", 1);
    }

    //이동 시작 세팅 함수
    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalking", true);
    }
    //이동 끝 세팅 함수
    void ChaseEnd()
    {
        isChase = false;
        anim.SetBool("isWalking", false);
    }
    void FixedUpdate()
    {
        if (isDead) // 보스가 죽었을때 모든 활동 정지
        {
            StopAllCoroutines();
            return;
        }
        //보스로 부터 플레이어의 방향을 계산, 플레이어를 향해 움직임
        if (isChase)
        {
            float dirX = ((Vector2)target.position - rigid.position).normalized.x; //플레이어와 보스의 x벡터 구하기
            rigid.velocity = new Vector2(dirX * moveSpeed, rigid.velocity.y); //플레이어쪽으로 이동
            // 이동 방향에 따라 회전 방향 설정
            if (dirX < 0)
                transform.rotation = Quaternion.Euler(0, 0, 0); // 왼쪽 바라봄
            else
                transform.rotation = Quaternion.Euler(0, 180, 0);   // 오른쪽 바라봄
        }
        if (!isDetec)
        {
            float distance = Vector2.Distance(rigid.position, target.position);
            if (distance <= outlineDetec)
            {
                isDetec = true;
                CancelInvoke();
                thinkRoutine = StartCoroutine(Think());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Melee")
        {
            if (isInvincible) return; //공격시 데미지 무시;

            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector2 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        //바깥 감지 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos, outlineDetec);

        //중간 감지 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos, middleDetec);

        //안쪽 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, inlineDetec);
    }

    IEnumerator OnDamage(Vector2 reactVec)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            spriteRenderer.color = Color.white;
            /*if (curHealth <= 50)
            {
                //오른쪽으로 후퇴
                StopEverything();
                isDetec = false;
                ChaseEnd();
                anim.SetTrigger("doRush");
                yield return new WaitForSeconds(1f);//준비시간
                spriteRenderer.flipX = true;
                rigid.velocity = new Vector2(-10f, rigid.velocity.y);

                yield return new WaitForSeconds(2f);//이동
                rigid.velocity = new Vector2(0f, rigid.velocity.y);
                gameObject.SetActive(false);
            }*/
        }
        else
        {
            StopEverything();
            spriteRenderer.color = Color.gray;
            gameObject.layer = 13; //반응 안하게
            isDead = true;
            isChase = false;
            isDetec = false;
            anim.SetTrigger("doDdie");
            reactVec = reactVec.normalized;
            reactVec += Vector2.up;
            rigid.AddForce(reactVec * 2, ForceMode2D.Impulse);
        }
    }

    IEnumerator Think()
    {
        //행동 시간 조정
        yield return new WaitForSeconds(1f);

        float distance = Vector2.Distance(rigid.position, target.position);

        if (distance <= inlineDetec)
        {
            StartCoroutine(Taunt());
        }
        else if (distance <= middleDetec)
        {
            StartCoroutine(BreathShot());
        }
        else if (distance <= outlineDetec)
        {
            StartCoroutine(Rush());
        }
        else
        {
            isDetec = false;
        }
    }

    void StopEverything() {
        StopAllCoroutines();
        if (instanceBreath != null)
        {
            Destroy(instanceBreath);
        }
    }

    IEnumerator BreathShot() //브레스 함수
    {
        ChaseEnd(); //이동 멈춤
        anim.SetTrigger("doBreath");
        yield return new WaitForSeconds(1f); //입벌리고 기 모으는 시간

        //브레스 생성
        instanceBreath = Instantiate(breath, breathPort.position, breathPort.rotation);

        yield return new WaitForSeconds(4f); //4초 후 브레스 없애기
        Destroy(instanceBreath);
        ChaseStart(); //이동 시작
        StartCoroutine(Think());
    }
    IEnumerator Rush() //돌진 함수
    {
        ChaseEnd();
        anim.SetTrigger("doRush");
        yield return new WaitForSeconds(1f);//준비시간
        isInvincible = true; //무적 활성화

        float dirX = ((Vector2)target.position - rigid.position).normalized.x; //플레이어와 보스의 x벡터 구하기
        rigid.velocity = new Vector2(dirX * 10f, rigid.velocity.y);
        for (int i = 0; i < 10; i++)
        {
            //cameraS.Shake(0.05f);
            yield return new WaitForSeconds(0.1f);//이동   
        }
        rigid.velocity = new Vector2(0f, rigid.velocity.y);
        isInvincible = false;

        yield return new WaitForSeconds(1f);//이동
        ChaseStart();
        StartCoroutine(Think());
    }
    IEnumerator Taunt() //찍기 함수
    {
        Debug.Log("찍기");
        //걷는 모션 제거, 플레이어를 향해서 이동만 허용
        ChaseEnd();
        isInvincible = true;
        anim.SetTrigger("doTaunt");
        //충동범위 활성화
        yield return new WaitForSeconds(1f); //1초동안 하늘에 떠있음

        //원래 이동 스피드 기억
        isChase = true;
        float temp = moveSpeed;
        moveSpeed = 3f;

        yield return new WaitForSeconds(0.5f); //0.5초동안 플레이어쪽으로 이동

        cameraS.Shake(0.5f); //카메라 흔들림 효과 shake(지속시간)
        isChase = false;
        meleeArea.enabled = true;
        //충돌범위 해제
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
        isInvincible = false;

        yield return new WaitForSeconds(1f);
        moveSpeed = temp;
        ChaseStart();
        StartCoroutine(Think());
    }

}
