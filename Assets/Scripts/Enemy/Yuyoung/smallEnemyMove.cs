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

    public float minRayLength;   // ground기준 최소 하강  높이
    public float maxRayLength;   // ground기준 최대 상승  높이

    void FixedUpdate()
    {
        rigid.velocity = new Vector2(xSpeed, ySpeed);
        // 최대 상승
        Debug.DrawRay(rigid.position, Vector3.down * maxRayLength, Color.red);
        RaycastHit2D rayHitHigh = Physics2D.Raycast(rigid.position, Vector3.down, maxRayLength);

        if (rayHitHigh.collider == null)
        {
            Debug.Log("Too High");
            ySpeed = -1 * Mathf.Abs(ySpeed); // 무조건 위로
            Debug.Log("GoingDown");
        }

        // 최대 하강 
        Debug.DrawRay(rigid.position, Vector3.down * minRayLength, Color.green);
        RaycastHit2D rayHitLow = Physics2D.Raycast(rigid.position, Vector3.down, minRayLength);

        if (rayHitLow.collider != null)
        {
            Debug.Log("Too Low");
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
