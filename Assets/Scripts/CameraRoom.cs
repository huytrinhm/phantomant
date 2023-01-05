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

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (GameMaster.Instance.CurrentVirtualCamera == _camera)
    //        return;

    //    if (!collision.CompareTag("Player"))
    //        return;

    //    if (GameMaster.Instance.CurrentVirtualCamera == null || !GameMaster.Instance.CurrentVirtualCamera.gameObject.activeInHierarchy) {
    //        GameMaster.Instance.CurrentVirtualCamera = _camera;
    //        GameMaster.Instance.CameraNoise = _noise;
    //        _camera.gameObject.SetActive(true);
    //    }
    //}

    public void ChangeToCam(bool queued = false)
    {
        GameMaster.Instance.CurrentVirtualCamera = _camera;
        GameMaster.Instance.CameraNoise = _noise;
        _camera.gameObject.SetActive(true);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        if (queued)
            GameMaster.Instance.QueueCam = null;

        if (this.name == "Room19")
        {
            PlayerController playerController = GameMaster.Instance._playerCollider.GetComponentInParent<PlayerController>();
            MeleeAttackManager attackManager = GameMaster.Instance._playerCollider.GetComponentInParent<MeleeAttackManager>();
            playerController.EndRoom();
            attackManager.IsEndRoom = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameMaster.Instance.CurrentVirtualCamera == _camera)
            return;

        if (!collision.CompareTag("Player"))
            return;

        if (GameMaster.Instance.CurrentVirtualCamera == null || !GameMaster.Instance.CurrentVirtualCamera.gameObject.activeInHierarchy)
        {
            ChangeToCam();
        } else
        {
            GameMaster.Instance.QueueCam = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (GameMaster.Instance.QueueCam != null)
        {
            if (GameMaster.Instance.QueueCam == this)
            {
                GameMaster.Instance.QueueCam = null;
            } else
            {
                _camera.gameObject.SetActive(false);
                GameMaster.Instance.QueueCam.ChangeToCam();
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}
