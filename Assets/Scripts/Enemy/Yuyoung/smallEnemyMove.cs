using System.Collections;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;

    public float xSpeed;         // x축 속도
    public float ySpeed;           // y축 시작 속도
    
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        StartCoroutine(ChangeYSpeedRoutine());
        StartCoroutine(ChangeXSpeedRoutine());
    }

    private float rayLength = 10f;   // 레이캐스트 길이

    void FixedUpdate()
    {
        rigid.velocity = new Vector2(xSpeed, ySpeed);

        // 레이를 활용해서 ground까지의 거리 체크
        Debug.DrawRay(rigid.position, Vector3.down * rayLength, Color.green);
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, rayLength);

        if (rayHit.collider != null)
        {
            Debug.Log("SmallEnemy: " + rayHit.collider.name);
            ySpeed = Mathf.Abs(ySpeed); // 무조건 위로
            Debug.Log("GoingUP");
        }
    }

    IEnumerator ChangeYSpeedRoutine()   // 0.5~3초마다 ySpeed 방향 반전
    {
        while (true)
        {
            ySpeed *= -1; // 방향 반전
            yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        }
    }
    IEnumerator ChangeXSpeedRoutine()   // 5초마다 xSpeed 방향 반전
    {
        while (true)
        {
            xSpeed *= -1; // 방향 반전
            yield return new WaitForSeconds(5f);
        }
    }
}
