using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class NextSceneTrigger : MonoBehaviour
{
    public GameObject cutsceneCanvas;
    public VideoPlayer videoPlayer;
    public GameObject pressKeyText;
    private bool ended = false;
    private bool isActive = false;
    private bool oneTimeActive = false;

    void Start()
    {
        videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.musicPlayer);
        videoPlayer.loopPointReached += EndVideo;
    }

    private void Update()
    {
        if (isActive && ended && Input.anyKeyDown)
        {
            StartCoroutine(EndCutscene());
        }
    }

    IEnumerator EndCutscene()
    {
        GameMaster.Instance.CrossFade.SetTrigger("Trans");
        yield return new WaitForSecondsRealtime(0.5f);
        cutsceneCanvas.SetActive(false);
        isActive = false;
        GameMaster.Instance.CrossFade.Play("SceneFadeIn");
        yield return new WaitForSecondsRealtime(0.5f);
        GameMaster.Instance.CutsceneEnd();
        AudioManager.Instance.PlayMusic("main_music");
        Destroy(gameObject);
    }

    void EndVideo(VideoPlayer vp)
    {
        pressKeyText.SetActive(true);
        ended = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !oneTimeActive)
        {
            GameMaster.Instance.ScrollCount++;
            StartCoroutine(StartCutscene());
        }
    }

    IEnumerator StartCutscene()
    {
        AudioManager.Instance.StopMusic();
        GameMaster.Instance.CutsceneStart();
        GameMaster.Instance.CrossFade.SetTrigger("Trans");
        cutsceneCanvas.SetActive(true);
        pressKeyText.SetActive(false);
        isActive = true;
        oneTimeActive = true;
        videoPlayer.Play();
        yield return new WaitForSecondsRealtime(0.5f);
        GameMaster.Instance.CrossFade.Play("SceneFadeIn");
    }
}
