using UnityEngine;

public class MashAttack : MonoBehaviour
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

    public void Damage(int amount = 1)
    {
        currHealth -= amount;
    }

    public bool Finished()
    {
        return currHealth <= 0;
    }
}
