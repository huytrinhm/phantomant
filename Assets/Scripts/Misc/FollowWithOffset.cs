using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FollowWithOffset : MonoBehaviour
{
    public Transform target;
    public float offsetX = 0;
    public float offsetY = 0;

    void Update()
    {
        this.transform.position = new Vector2(target.position.x + offsetX, target.position.y + offsetY);
    }
}
