using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    public AudioSource sound;   // Inspector에서 안 넣어도 됨

    private void Awake()
    {
        // sound에 아무것도 안 들어있으면,
        // 이 오브젝트에 붙어있는 AudioSource를 자동으로 찾아서 사용
        if (sound == null)
        {
            sound = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"PlaySoundOnTrigger sound obj = {sound?.gameObject.name}", this);

        if (!collision.CompareTag("Player")) return;
        if (sound == null) return;

        // 혹시라도 비활성화돼 있으면 켜주기
        if (!sound.enabled)
            sound.enabled = true;

        if (!sound.gameObject.activeInHierarchy)
            sound.gameObject.SetActive(true);

        sound.Play();
    }
}
