using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Configurações de Música")]
    [Tooltip("O clipe de áudio principal para a música de fundo.")]
    public AudioClip musicClip;

    [Tooltip("O mixer de áudio para controlar o volume da música. Deixe vazio para usar o padrão.")]
    public AudioMixerGroup musicMixerGroup;

    [Range(0f, 1f)]
    [Tooltip("O volume inicial da música.")]
    public float initialVolume = 1f;

    [Tooltip("Define se a música deve começar a tocar automaticamente ao iniciar o jogo.")]
    public bool playMusicOnAwake = true;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false; // Controlado por playMusicOnAwake
        audioSource.volume = initialVolume;

        if (musicMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = musicMixerGroup;
        }
    }

    void Start()
    {
        if (playMusicOnAwake)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (musicClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = musicClip;
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void ChangeMusic(AudioClip newClip)
    {
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        audioSource.volume = volume;
    }
}


