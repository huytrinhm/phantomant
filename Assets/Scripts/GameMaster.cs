using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private GameObject _map1;
    [SerializeField] private GameObject _map2;
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private float _animStartTime = 0.2f;
    [SerializeField] private float _animHangTime = 0.3f;
    [SerializeField] private float _animEndTime = 0.1f;
    [SerializeField] private float _zoomFactor = 0.1f;
    [SerializeField] private float _shakeAmount = 0.5f;
    [SerializeField] private float _shakeFrequency = 0.1f;
    [SerializeField] private ParticleSystem _changeMapParticle;

    private Renderer _map1Renderer;
    private Collider2D _map1Collider;
    private Renderer _map2Renderer;
    private Collider2D _map2Collider;
    private CinemachineBasicMultiChannelPerlin _cameraNoise;
    private int _defaultLayer;
    private int _platformLayer;

    private bool _isPlayingInvalidAnim = false;

    private void Awake()
    {
        _map1.TryGetComponent<Renderer>(out _map1Renderer);
        _map1.TryGetComponent<Collider2D>(out _map1Collider);
        _map2.TryGetComponent<Renderer>(out _map2Renderer);
        _map2.TryGetComponent<Collider2D>(out _map2Collider);
        _cameraNoise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        _defaultLayer = LayerMask.NameToLayer("Default");
        _platformLayer = LayerMask.NameToLayer("Platform");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("ChangeMap") && !_isPlayingInvalidAnim)
        {
            SwitchMap();
        }
    }

    private void SwitchMap()
    {
        if(_map1Collider.isTrigger)
        {
            if (_map1Collider.IsTouching(_playerCollider))
            {
                InvalidSwitchMap();
                return;
            }
        } else
        {
            if (_map2Collider.IsTouching(_playerCollider))
            {
                InvalidSwitchMap();
                return;
            }
        }

        _map1Collider.isTrigger = !_map1Collider.isTrigger;
        _map2Collider.isTrigger = !_map2Collider.isTrigger;
        _map1Renderer.enabled = !_map1Renderer.enabled;
        _map2Renderer.enabled = !_map2Renderer.enabled;
        _changeMapParticle.Play();
        if (_map1Renderer.enabled)
        {
            _map1.layer = _platformLayer;
            _map2.layer = _defaultLayer;
        } else
        {
            _map1.layer = _defaultLayer;
            _map2.layer = _platformLayer;
        }
    }

    private void InvalidSwitchMap()
    {
        StartCoroutine(Cor_InvalidSwitchMap());
    }

    IEnumerator Cor_InvalidSwitchMap()
    {
        _isPlayingInvalidAnim = true;
        Time.timeScale = 0f;
        _map1Renderer.enabled = !_map1Renderer.enabled;
        _map2Renderer.enabled = !_map2Renderer.enabled;

        _changeMapParticle.Play();

        float elapsed = 0f;
        float startSize = _camera.m_Lens.OrthographicSize;
        while (elapsed < _animStartTime)
        {
            _camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, startSize*(1-_zoomFactor), elapsed / _animStartTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _cameraNoise.m_FrequencyGain = _shakeFrequency;
        _cameraNoise.m_AmplitudeGain = _shakeAmount;
        yield return new WaitForSecondsRealtime(_animHangTime);
        _cameraNoise.m_AmplitudeGain = 0f;

        _map1Renderer.enabled = !_map1Renderer.enabled;
        _map2Renderer.enabled = !_map2Renderer.enabled;

        elapsed = 0f;
        while (elapsed < _animEndTime)
        {
            _camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize * (1 - _zoomFactor), startSize, elapsed / _animEndTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _camera.m_Lens.OrthographicSize = startSize;
        Time.timeScale = 1f;
        _isPlayingInvalidAnim = false;
    }

}
