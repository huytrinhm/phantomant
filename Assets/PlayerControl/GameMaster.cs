using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance { get { return _instance; } }

    [Header("General")]
    [SerializeField] private CinemachineBrain _cinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera _camera;

    [Header("Change Map")]
    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private GameObject _map1;
    [SerializeField] private GameObject _map2;
    [SerializeField] private float _animStartTime = 0.2f;
    [SerializeField] private float _animHangTime = 0.3f;
    [SerializeField] private float _animEndTime = 0.1f;
    [SerializeField] private float _zoomFactor = 0.1f;
    [SerializeField] private float _shakeAmount = 0.5f;
    [SerializeField] private float _shakeFrequency = 0.1f;
    [SerializeField] private ParticleSystem _changeMapParticle;

    [Header("Menus")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundEffectSlider;
    [SerializeField] private TextMeshProUGUI _musicText;
    [SerializeField] private TextMeshProUGUI _soundEffectText;

    public bool IsPaused = false;

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
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

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
        if(Input.GetButtonDown("ChangeMap") && !_isPlayingInvalidAnim && !IsPaused)
        {
            SwitchMap();
        }

        if(Input.GetButtonDown("Pause"))
        {
            if (!_pauseMenu.activeInHierarchy && !_settingsMenu.activeInHierarchy)
                PauseGame();
            else if (!_settingsMenu.activeInHierarchy)
                ResumeGame();
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
            while (IsPaused)
                yield return null;
            _camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, startSize*(1-_zoomFactor), elapsed / _animStartTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        while (IsPaused)
            yield return null;
        _cameraNoise.m_FrequencyGain = _shakeFrequency;
        _cameraNoise.m_AmplitudeGain = _shakeAmount;
        while (IsPaused)
            yield return null;
        yield return new WaitForSecondsRealtime(_animHangTime);
        _cameraNoise.m_AmplitudeGain = 0f;

        _map1Renderer.enabled = !_map1Renderer.enabled;
        _map2Renderer.enabled = !_map2Renderer.enabled;

        elapsed = 0f;
        while (elapsed < _animEndTime)
        {
            while (IsPaused)
                yield return null;
            _camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize * (1 - _zoomFactor), startSize, elapsed / _animEndTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _camera.m_Lens.OrthographicSize = startSize;
        if(!IsPaused)
            Time.timeScale = 1f;
        _isPlayingInvalidAnim = false;
    }

    private void PauseGame()
    {
        IsPaused = true;
        _cinemachineBrain.m_IgnoreTimeScale = false;
        
        Time.timeScale = 0f;
        _pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        _cinemachineBrain.m_IgnoreTimeScale = true;
        Time.timeScale = 1f;
        _pauseMenu.SetActive(false);
    }

    public void ShowSettings()
    {
        _musicSlider.value = AudioManager.Instance.MusicVolume * 100;
        _soundEffectSlider.value = AudioManager.Instance.SoundEffectVolume * 100;
        _musicText.text = (AudioManager.Instance.MusicVolume * 100).ToString();
        _soundEffectText.text = (AudioManager.Instance.SoundEffectVolume * 100).ToString();
        _pauseMenu.SetActive(false);
        _settingsMenu.SetActive(true);
    }

    public void HideSettings()
    {
        _settingsMenu.SetActive(false);
        _pauseMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnMusicChanged()
    {
        AudioManager.Instance.MusicVolume = _musicSlider.value;
        _musicText.text = (AudioManager.Instance.MusicVolume * 100).ToString();
    }

    public void OnSoundEffectChanged()
    {
        AudioManager.Instance.SoundEffectVolume = _soundEffectSlider.value;
        _soundEffectText.text = (AudioManager.Instance.SoundEffectVolume * 100).ToString();
    }
}
