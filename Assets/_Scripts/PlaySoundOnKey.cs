using UnityEngine;
using UnityEngine.Audio;

public class PlaySoundOnKey : MonoBehaviour
{
    [Header("Configurações de Áudio")]
    [Tooltip("O clipe de áudio a ser reproduzido.")]
    public AudioClip soundClip;

    [Tooltip("A tecla que, quando pressionada, irá reproduzir o som.")]
    public KeyCode activationKey = KeyCode.F;

    [Range(0f, 1f)]
    [Tooltip("O volume do som.")]
    public float volume = 1f;

    [Range(-3f, 3f)]
    [Tooltip("O pitch (velocidade de reprodução) do som.")]
    public float pitch = 1f;

    [Tooltip("O mixer de áudio para controlar o som. Deixe vazio para usar o padrão.")]
    public AudioMixerGroup soundMixerGroup;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        if (soundMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = soundMixerGroup;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            if (soundClip != null)
            {
                audioSource.PlayOneShot(soundClip, volume);
            }
            else
            {
                Debug.LogWarning("Nenhum AudioClip foi atribuído para reprodução na tecla " + activationKey.ToString() + "!");
            }
        }
    }

    // Método público para reproduzir o som de outros scripts, se necessário
    public void PlaySound()
    {
        if (soundClip != null)
        {
            audioSource.PlayOneShot(soundClip, volume);
        }
        else
        {
            Debug.LogWarning("Nenhum AudioClip foi atribuído para reprodução!");
        }
    }
}


