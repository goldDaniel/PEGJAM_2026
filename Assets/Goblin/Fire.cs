using UnityEngine;

public class Fire : MonoBehaviour
{
    public int maxHealth;

    private int currHealth;

    public int Health() => currHealth;
    public int MaxHealth() => maxHealth;

    private void Awake()
    {
        currHealth = maxHealth;
    }

    public void Reset()
    {
        currHealth = maxHealth;
    }

    public void Damage()
    {
        currHealth--;
    }

    public bool isDead()
    {
        return currHealth <= 0;
    }
}
