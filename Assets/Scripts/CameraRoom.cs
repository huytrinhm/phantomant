using UnityEngine;
using Cinemachine;

public class CameraRoom : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    private CinemachineBasicMultiChannelPerlin _noise;

    private void Awake()
    {
        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameMaster.Instance.CurrentVirtualCamera = _camera;
            GameMaster.Instance.CameraNoise = _noise;
            _camera.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _camera.gameObject.SetActive(false);
        }
    }
}
