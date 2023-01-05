using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance { get { return _instance; } }
    public float _cameraSize = 8.73f;

    [Header("General")]
    [SerializeField] private CinemachineBrain _cinemachineBrain;
    public Animator CrossFade;

    private CinemachineBasicMultiChannelPerlin _cameraNoise;
    [HideInInspector()] public CinemachineBasicMultiChannelPerlin CameraNoise {
        get {
            return _cameraNoise;
        }
        set {
            _cameraNoise = value;
            _cameraNoise.m_AmplitudeGain = 0;
        }
    }

    private CinemachineVirtualCamera _camera;
    public CinemachineVirtualCamera CurrentVirtualCamera {
        get { return _camera; }
        set { 
            _camera = value;
            _camera.m_Lens.OrthographicSize = _cameraSize;
        }
    }

    public CameraRoom QueueCam;

    [Header("Change Map")]
    public Collider2D _playerCollider;
    [SerializeField] private GameObject _map1;
    [SerializeField] private GameObject _map2;
    [SerializeField] private GameObject _spikesMap1;
    [SerializeField] private GameObject _spikesMap2;
    [SerializeField] private GameObject _backgroundMap1;
    [SerializeField] private GameObject _backgroundMap2;
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
    [SerializeField] private GameObject _gameOverMenu;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundEffectSlider;
    [SerializeField] private TextMeshProUGUI _musicText;
    [SerializeField] private TextMeshProUGUI _soundEffectText;

    [Header("Effects")]
    [Range(0, 1)] public float HitFadeAmount = 0.2f;
    public ParticleSystem DeathParticle;

    public bool IsPaused = false;

    private Renderer _map1Renderer;
    private Collider2D _map1Collider;
    private CompositeShadowCaster2D _map1Shadow;
    private Renderer _map2Renderer;
    private Collider2D _map2Collider;
    private CompositeShadowCaster2D _map2Shadow;
    private int _defaultLayer;
    private int _platformLayer;

    public bool IsOnMap1 = true;
    public bool CutscenePlaying = false;
    public bool AllowSwitchMap = true;

    public int KillCount = 0;
    public int ScrollCount = 0;

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
        _map1.TryGetComponent<CompositeShadowCaster2D>(out _map1Shadow);
        _map2.TryGetComponent<Renderer>(out _map2Renderer);
        _map2.TryGetComponent<Collider2D>(out _map2Collider);
        _map2.TryGetComponent<CompositeShadowCaster2D>(out _map2Shadow);

        _defaultLayer = LayerMask.NameToLayer("Default");
        _platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic("main_music");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("ChangeMap") && !_isPlayingInvalidAnim && !IsPaused && AllowSwitchMap)
        {
            SwitchMap();
        }

        if(Input.GetButtonDown("Pause") && !CutscenePlaying)
        {
            if (!_pauseMenu.activeInHierarchy && !_settingsMenu.activeInHierarchy)
                PauseGame();
            else if (!_settingsMenu.activeInHierarchy)
                ResumeGame();
        }
    }

    private void SwitchMap()
    {
        AudioManager.Instance.PlaySoundEffect("hero_switch_map");
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
        _map1Shadow.enabled = !_map1Shadow.enabled;
        _map2Shadow.enabled = !_map2Shadow.enabled;
        _spikesMap1.SetActive(!_spikesMap1.activeSelf);
        _spikesMap2.SetActive(!_spikesMap2.activeSelf);
        _backgroundMap1.SetActive(!_backgroundMap1.activeSelf);
        _backgroundMap2.SetActive(!_backgroundMap2.activeSelf);

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

        IsOnMap1 = _map1Renderer.enabled;
        if (IsOnMap1)
            AudioManager.Instance.StopSoundEffect("hero_run_grass");
        else
            AudioManager.Instance.StopSoundEffect("hero_run_stone");
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
        _map1Shadow.enabled = !_map1Shadow.enabled;
        _map2Shadow.enabled = !_map2Shadow.enabled;
        _spikesMap1.SetActive(!_spikesMap1.activeSelf);
        _spikesMap2.SetActive(!_spikesMap2.activeSelf);
        _backgroundMap1.SetActive(!_backgroundMap1.activeSelf);
        _backgroundMap2.SetActive(!_backgroundMap2.activeSelf);

        _changeMapParticle.Play();

        float elapsed = 0f;
        float startSize = CurrentVirtualCamera.m_Lens.OrthographicSize;
        while (elapsed < _animStartTime)
        {
            while (IsPaused)
                yield return null;
            CurrentVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, startSize*(1-_zoomFactor), elapsed / _animStartTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        while (IsPaused)
            yield return null;

        yield return Cor_ShakeCamera(_shakeAmount, _shakeFrequency, _animHangTime);

        _map1Renderer.enabled = !_map1Renderer.enabled;
        _map2Renderer.enabled = !_map2Renderer.enabled;
        _map1Shadow.enabled = !_map1Shadow.enabled;
        _map2Shadow.enabled = !_map2Shadow.enabled;
        _spikesMap1.SetActive(!_spikesMap1.activeSelf);
        _spikesMap2.SetActive(!_spikesMap2.activeSelf);
        _backgroundMap1.SetActive(!_backgroundMap1.activeSelf);
        _backgroundMap2.SetActive(!_backgroundMap2.activeSelf);

        elapsed = 0f;
        while (elapsed < _animEndTime)
        {
            while (IsPaused)
                yield return null;
            CurrentVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize * (1 - _zoomFactor), startSize, elapsed / _animEndTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        CurrentVirtualCamera.m_Lens.OrthographicSize = startSize;
        if(!IsPaused)
            Time.timeScale = 1f;
        _isPlayingInvalidAnim = false;
    }

    private void PauseGame()
    {
        IsPaused = true;
        AudioManager.Instance.PauseAllSoundEffect();
        AudioManager.Instance.PlayMusic("menu_music");
        _cinemachineBrain.m_IgnoreTimeScale = false;
        
        Time.timeScale = 0f;
        _pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        AudioManager.Instance.UnPauseAllSoundEffect();
        AudioManager.Instance.PlayMusic("main_music");
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

    public void GameOver()
    {
        IsPaused = true;
        AudioManager.Instance.StopAllSoundEffect();
        _cinemachineBrain.m_IgnoreTimeScale = false;
        Time.timeScale = 0f;
        _gameOverMenu?.SetActive(true);
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

    public void ReloadLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShakeCamera(float amplitude, float frequency, float duration)
    {
        StartCoroutine(Cor_ShakeCamera(amplitude, frequency, duration));
    }

    public IEnumerator Cor_ShakeCamera(float amplitude, float frequency, float duration)
    {
        CameraNoise.m_FrequencyGain = frequency;
        CameraNoise.m_AmplitudeGain = amplitude;
        while (IsPaused)
            yield return null;
        yield return new WaitForSecondsRealtime(duration);
        CameraNoise.m_AmplitudeGain = 0f;
    }

    public void Hover()
    {
        AudioManager.Instance.PlaySoundEffect("button_hover");
    }

    public void Click()
    {
        AudioManager.Instance.PlaySoundEffect("button_click");
    }

    public void CutsceneStart()
    {
        CutscenePlaying = true;
        IsPaused = true;
        AudioManager.Instance.StopAllSoundEffect();
        Time.timeScale = 0f;
    }

    public void CutsceneEnd()
    {
        CutscenePlaying = false;
        IsPaused = false;
        Time.timeScale = 1f;
    }
}
