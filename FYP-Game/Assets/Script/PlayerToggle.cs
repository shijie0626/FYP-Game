using UnityEngine;
using System.Collections;

public class PlayerToggleWithBeacon : MonoBehaviour
{
    [Tooltip("The root Player GameObject to toggle")]
    public GameObject playerObject;

    [Tooltip("The presence beacon GameObject (disabled by default)")]
    public GameObject presenceBeacon;

    private bool isActive = true;

    void Start()
    {
        if (playerObject != null) isActive = playerObject.activeSelf;
        if (presenceBeacon != null) presenceBeacon.SetActive(false);
    }

    public void HidePlayer()
    {
        if (playerObject == null) return;

        // Stop darkness effect to avoid it being stuck
        GameController.Instance?.StopDarknessEffect();

        if (presenceBeacon != null)
        {
            presenceBeacon.transform.position = playerObject.transform.position;

            var beaconComp = presenceBeacon.GetComponent<PresenceBeacon>();
            if (beaconComp != null)
            {
                beaconComp.followTarget = playerObject.transform;
                beaconComp.follow = true;
            }

            presenceBeacon.SetActive(true);
        }

        playerObject.SetActive(false);
        isActive = false;
    }

    public void ShowPlayer()
    {
        if (playerObject == null) return;

        playerObject.SetActive(true);

        if (presenceBeacon != null)
        {
            var beaconComp = presenceBeacon.GetComponent<PresenceBeacon>();
            if (beaconComp != null)
            {
                beaconComp.follow = false;
                beaconComp.followTarget = null;
            }

            presenceBeacon.SetActive(false);
        }

        isActive = true;
    }

    public bool IsPlayerActive()
    {
        return isActive;
    }
}
