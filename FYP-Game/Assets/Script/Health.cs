using UnityEngine;

public class Health : MonoBehaviour
{
    public float startingHealth;
    public float currentHealth;
    public GameObject GameOverPanel;
    public void Awake()
    {
        currentHealth = startingHealth;
    }


    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {

        }


    }

    public void Update()
    {
        if (currentHealth <= 0)
        {
         
        }
    }

}