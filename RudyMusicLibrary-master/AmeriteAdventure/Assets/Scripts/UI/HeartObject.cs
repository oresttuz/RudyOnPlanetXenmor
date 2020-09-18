using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartObject : MonoBehaviour
{
    public SpriteRenderer renderer;
    public Transform transform;

    public int SpriteIndex;
    public float health;

    public HeartObject(SpriteRenderer sr, Transform t, int si, float h)
    {
        renderer = sr;
        transform = t;
        SpriteIndex = si;
        health = h;
    }

    public float Heal(float amount)
    {
        if (health < 1f && health >= 0f)
        {
            if (health + amount < 1f) // Health > Amount
            {
                health += amount;
                return 0f;
            }
            float diff = health;
            health = 1f;
            amount -= (1f - diff); // Amount > Health
            return amount;
        }
        return amount; // Health = 1f
    }

    public float Damage(float amount)
    {
        if (health <= 1f && health > 0f)
        {
            if (health - amount > 0f) // Health > Amount
            {
                health -= amount;
                return 0f;
            }
            amount -= health; // Amount > Health
            health = 0f;
            return amount;
        }
        return amount; // Health = 0f
    }
}
