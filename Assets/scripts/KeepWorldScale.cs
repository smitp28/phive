using UnityEngine;

public class KeepWorldScale : MonoBehaviour
{
    void LateUpdate()
    {
        // Always face right regardless of parent flip
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}