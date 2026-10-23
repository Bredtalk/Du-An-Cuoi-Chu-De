using System;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;


    void Die()
    {
        Debug.Log("Die");
    }

    public bool isDead => currentHealth <= 0;

    public bool TakeDamage(int damage)
    {
        if (isDead)
        {
            return true;
        }

        currentHealth -= damage;
        if (isDead)
        {
            Die();
        }
        return isDead;
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        
    }
    
   
 
}