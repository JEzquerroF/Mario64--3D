using System.Collections;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public enum SurfaceType
    {
        Metal,
        Mud,
        Default
    }

    [SerializeField] private AudioSource m_SoundManager;
    [SerializeField] private AudioSource m_SoundManager3D;
    [SerializeField] private AudioClip[] m_AmbienceSoundClips;
    [SerializeField] private AudioClip[] m_MudFootstepClips;
    [SerializeField] private AudioClip[] m_MetalFootstepClips;
    [SerializeField] private AudioClip[] m_DefaultFootstepClips;

    private int index;

    public static SoundsManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayFootstepSound(Transform _transform, float volume, SurfaceType surfaceType)
    {
        AudioClip[] footstepClips = GetFootstepClipsBySurface(surfaceType);

        if (footstepClips != null && footstepClips.Length > 0)
        {
            index = Random.Range(0, footstepClips.Length);

            AudioSource _audioSource = Instantiate(m_SoundManager, _transform.position, Quaternion.identity);
            _audioSource.clip = footstepClips[index];
            _audioSource.volume = volume;
            _audioSource.loop = false;
            _audioSource.Play();
            float soundLength = _audioSource.clip.length;
            Destroy(_audioSource.gameObject, soundLength);
        }
    }

    private AudioClip[] GetFootstepClipsBySurface(SurfaceType surfaceType)
    {
        switch (surfaceType)
        {
            case SurfaceType.Metal:
                return m_MetalFootstepClips;
            case SurfaceType.Mud:
                return m_MudFootstepClips;
            default:
                return m_DefaultFootstepClips;
        }
    }

    public void PlaySoundClip(AudioClip _audioClip, Transform _transform, float volume)
    {
        AudioSource _audioSource = Instantiate(m_SoundManager, _transform.position, Quaternion.identity);

        _audioSource.clip = _audioClip;
        _audioSource.volume = volume;
        _audioSource.loop = false;
        _audioSource.Play();
        float soundLenght = _audioSource.clip.length;
        Destroy(_audioSource.gameObject, soundLenght);
    }

    public void PlayLongSound(AudioClip _audioClip, Transform _transform, float volume, float soundLenght)
    {
        AudioSource _audioSource = Instantiate(m_SoundManager, _transform.position, Quaternion.identity);

        _audioSource.clip = _audioClip;
        _audioSource.volume = volume;
        _audioSource.loop = true;
        _audioSource.Play();
        Destroy(_audioSource.gameObject, soundLenght);
    }

    public void PlayLoopSound3D(AudioClip _audioClip, Transform _transform, float volume)
    {
        AudioSource _audioSource = Instantiate(m_SoundManager3D, _transform.position, Quaternion.identity);

        _audioSource.clip = _audioClip;
        _audioSource.volume = volume;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void PlayLongSound3D(AudioClip _audioClip, Transform _transform, float volume, float soundLenght)
    {
        AudioSource _audioSource = Instantiate(m_SoundManager3D, _transform.position, Quaternion.identity);

        _audioSource.clip = _audioClip;
        _audioSource.volume = volume;
        _audioSource.loop = false;
        _audioSource.Play();
        Destroy(_audioSource.gameObject, soundLenght);
    }

    private IEnumerator RandomPlayer()
    {
        float waitTime;

        while (true)
        {
            waitTime = Random.Range(0.0f, 500.0f);
            yield return new WaitForSeconds(waitTime);
            PlayRandomSound(transform, 0.05f);
        }
    }

    private void PlayRandomSound(Transform _transform, float volume)
    {
        index = Random.Range(0, m_AmbienceSoundClips.Length);

        AudioSource _audioSource = Instantiate(m_SoundManager, _transform.position, Quaternion.identity);

        _audioSource.clip = m_AmbienceSoundClips[index];
        _audioSource.volume = volume;
        _audioSource.loop = false;
        float soundLenght = _audioSource.clip.length;
        _audioSource.Play();
        Destroy(_audioSource.gameObject, soundLenght);
    }

    public void StopAllSounds()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.CompareTag("SoundsFX"))
            {
                audioSource.Stop();
            }
        }
    }
}
