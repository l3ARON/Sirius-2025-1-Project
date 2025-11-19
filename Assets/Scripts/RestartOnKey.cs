using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnKey : MonoBehaviour
{
    int count = 0;
    float timer = 0f;
    public float resetDelay = 3f; // 3초 후 초기화

    void Update()
    {
        // R 키 누르면 count 증가 + 타이머 시작
        if (Input.GetKeyDown(KeyCode.P))
        {
            count++;

            // 첫 입력일 때만 타이머 초기화
            if (count == 1)
            {
                timer = resetDelay;
            }
        }

        // 타이머 작동
        if (count > 0)
        {
            timer -= Time.deltaTime;

            // 타이머 끝 → count 초기화
            if (timer <= 0f)
            {
                count = 0;
            }
        }

        // R을 3번 누르면 씬 재시작
        if (count >= 3)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
