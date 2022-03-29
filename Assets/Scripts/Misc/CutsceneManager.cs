using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    public float blinkInterval = 1f;
    public VideoPlayer videoPlayer;
    public TextMeshProUGUI pressKeyText;
    public Animator crossFade;
    private bool ended = false;

    void Start()
    {
        videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.musicPlayer);
        pressKeyText.alpha = 0;
        videoPlayer.Play();
        videoPlayer.loopPointReached += EndVideo;
    }

    private void Update()
    {
        if(ended && Input.anyKey)
        {
            StartCoroutine(CrossFade());
        }
    }

    IEnumerator CrossFade()
    {
        crossFade.SetTrigger("Trans");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void EndVideo(VideoPlayer vp)
    {
        pressKeyText.alpha = 1;
        ended = true;
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            float elapsed = 0f;
            while (elapsed < blinkInterval/2)
            {
                pressKeyText.alpha = Mathf.Lerp(1, 0, elapsed / (blinkInterval / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < blinkInterval / 2)
            {
                pressKeyText.alpha = Mathf.Lerp(0, 1, elapsed / (blinkInterval / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
