using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class RoomLight : MonoBehaviour
{
    public Light2D roomLight; // Assign in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Turn off all other room lights
            RoomLight[] allRooms = FindObjectsOfType<RoomLight>();
            foreach (RoomLight room in allRooms)
                room.roomLight.enabled = false;

            // Turn on this room's light
            roomLight.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Optional: Turn off light when leaving
            // But if you want the next room's light on instantly,
            // you may not need this line.
            roomLight.enabled = false;
        }
    }
}
