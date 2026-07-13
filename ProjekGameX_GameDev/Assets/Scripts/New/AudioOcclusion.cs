using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioOcclusion : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioLowPassFilter _lowPassFilter;
    private Transform _playerListener;

    [Header("Configuración de Oclusión")]
    public LayerMask occlusionLayers;

    [Tooltip("Frecuencia cuando hay una pared. 1000 = Muy ahogado, 5000 = Ligeramente ahogado")]
    [Range(500f, 22000f)] public float muffledFrequency = 1200f;

    [Tooltip("Cuánto baja el volumen al estar detrás de una pared (0.5 = a la mitad)")]
    [Range(0f, 1f)] public float muffledVolumeMultiplier = 0.4f;

    public float transitionSpeed = 8f;

    private float _openFrequency = 22000f;
    private float _baseVolume;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _lowPassFilter = GetComponent<AudioLowPassFilter>();
        _baseVolume = _audioSource.volume;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerListener = player.transform;
        }
    }

    private void Update()
    {
        if (_playerListener == null) return;

        float targetFrequency = _openFrequency;
        float targetVolume = _baseVolume;

        Vector3 sourcePos = transform.position;
        Vector3 listenerPos = _playerListener.position + Vector3.up * 1.5f;

        if (Physics.Linecast(sourcePos, listenerPos, occlusionLayers, QueryTriggerInteraction.Ignore))
        {
            targetFrequency = muffledFrequency;
            targetVolume = _baseVolume * muffledVolumeMultiplier;
        }

        _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassFilter.cutoffFrequency, targetFrequency, Time.deltaTime * transitionSpeed);
        _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
    }
}