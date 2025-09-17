using UnityEngine;

public class PresenceBeacon : MonoBehaviour
{
    [HideInInspector] public Transform followTarget;
    public bool follow = true;

    void Update()
    {
        if (follow && followTarget != null)
            transform.position = followTarget.position;
    }
}
