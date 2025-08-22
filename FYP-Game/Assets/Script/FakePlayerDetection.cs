using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;     // Assign your player here
    public float speed = 3f;     // Movement speed
    public float stoppingDistance = 1f; // Distance before stopping (optional)

    void Update()
    {
        if (player != null)
        {
            // Calculate distance to player
            float distance = Vector2.Distance(transform.position, player.position);

            // Only move if outside the stopping distance
            if (distance > stoppingDistance)
            {
                // Move towards player
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );
            }
        }
    }
}
