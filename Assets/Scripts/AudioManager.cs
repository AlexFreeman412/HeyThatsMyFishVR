using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioSource penguinSound;

    private static AudioManager _instance = null;
    public static AudioManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
            _instance = this;

        DontDestroyOnLoad(this.gameObject);
        gameObject.name = "$AudioManager";

    }

    void Start()
    {
        AudioSource[] asources = GetComponents<AudioSource>();
        backgroundMusic = asources[0];
        penguinSound = asources[1];
    }

    public void PlayPenguinSound()
    {
        if(!penguinSound.isPlaying)
            penguinSound.Play();
    }

    public void ChangeBackgroundMusic(AudioClip clip)
    {
        backgroundMusic.clip = clip;
        backgroundMusic.loop = true;
        if (!backgroundMusic.isPlaying)
            backgroundMusic.Play();
    }

}
