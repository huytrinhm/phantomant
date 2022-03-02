using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Animator _crossFade;
    [SerializeField] private GameObject _main;
    [SerializeField] private GameObject _settings;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundEffectSlider;
    [SerializeField] private TextMeshProUGUI _musicText;
    [SerializeField] private TextMeshProUGUI _soundEffectText;

    public void StartGame()
    {
        StartCoroutine(CrossFade());
    }

    IEnumerator CrossFade()
    {
        _crossFade.SetTrigger("Trans");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowSettings()
    {
        _musicSlider.value = AudioManager.Instance.MusicVolume*100;
        _soundEffectSlider.value = AudioManager.Instance.SoundEffectVolume*100;
        _musicText.text = (AudioManager.Instance.MusicVolume * 100).ToString();
        _soundEffectText.text = (AudioManager.Instance.SoundEffectVolume * 100).ToString();
        _main.SetActive(false);
        _settings.SetActive(true);
    }

    public void HideSettings()
    {
        _settings.SetActive(false);
        _main.SetActive(true);
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
