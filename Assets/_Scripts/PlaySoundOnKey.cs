using UnityEngine;

public class PlaySoundOnKey : MonoBehaviour
{
    [Header("Configura��es de �udio")]
    public AudioClip soundClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (soundClip != null)
            {
                audioSource.PlayOneShot(soundClip);
            }
            else
            {
                Debug.LogWarning("Nenhum AudioClip foi atribu�do!");
            }
        }
    }
}
