using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;

    public AudioSource firstbossmusic;
    public AudioSource firststagemusic;
    void Start()
    {
        musicSource.clip = firststagemusic.clip;
        musicSource.Play();
    }

    public void FirstBossSFX()
    {
    musicSource.Stop();                    // 이전 노래 끔
    musicSource.clip = firstbossmusic.clip; // 노래 변경
    musicSource.Play();                   // 보스 음악 시작
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
