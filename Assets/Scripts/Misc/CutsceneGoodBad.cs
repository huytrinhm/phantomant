using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;

public class CutsceneGoodBad : MonoBehaviour
{
    public GameObject cutsceneCanvasGood;
    public VideoPlayer videoPlayerGood;
    public GameObject pressKeyTextGood;
    public GameObject cutsceneCanvasBad;
    public VideoPlayer videoPlayerBad;
    public GameObject pressKeyTextBad;
    private GameObject cutsceneCanvas;
    private VideoPlayer videoPlayer;
    private GameObject pressKeyText;
    private bool ended = false;
    private bool isActive = false;
    private bool oneTimeActive = false;

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
        GameMaster.Instance.CutsceneEnd();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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
            if (GameMaster.Instance.KillCount > 0 || GameMaster.Instance.ScrollCount > 0)
            {
                cutsceneCanvas = cutsceneCanvasBad;
                videoPlayer = videoPlayerBad;
                pressKeyText = pressKeyTextBad;
            } else
            {
                cutsceneCanvas = cutsceneCanvasGood;
                videoPlayer = videoPlayerGood;
                pressKeyText = pressKeyTextGood;
            }

            videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.musicPlayer);
            videoPlayer.loopPointReached += EndVideo;
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
