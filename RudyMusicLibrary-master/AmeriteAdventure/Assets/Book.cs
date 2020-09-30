using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Book : MonoBehaviour
{
    public bool isKnowledge;
    public int attack;
    public Sprite bookSprite;

    private SpriteRenderer mySR;

    public void UpdateBook(Sprite sprite, int attackNum)
    {
        mySR = GetComponentInChildren<SpriteRenderer>();
        bookSprite = sprite;
        mySR.sprite = bookSprite;
        attack = attackNum;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player(Clone)")
        {
            other.GetComponentInParent<PlayerMovement>().AddAttack(attack);
            Destroy(gameObject);
        }
    }
}
