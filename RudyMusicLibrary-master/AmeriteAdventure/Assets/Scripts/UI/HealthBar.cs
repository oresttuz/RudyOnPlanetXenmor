using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Sprite[] healthSprites;
    public List<HeartObject> health;

    public Transform HealthBarTransform, followTransform;

    public int totalHealth;
    public float currHealth;

    public Vector3 startPos;
    public float padding;

    public HeartObject pfHeart;

    /*
    private void Start()
    {
        startPos = new Vector3(35f, -30f, 0f);

        health = new List<HeartObject>();
        int startingHealth = totalHealth;
        for (int i = 0; i < startingHealth; i++)
        {
            AddHealth();
        }

        //reset just for start function
        totalHealth = startingHealth;
        currHealth = (totalHealth * 1f);
    }
    */

    public void Init()
    {
        health = new List<HeartObject>();
        int startingHealth = totalHealth;
        startPos = new Vector3(35f, -30f, 0f);
        HealthBarTransform = this.transform;
        for (int i = 0; i < startingHealth; i++)
        {
            AddHealth();
        }
        //reset just for start function
        totalHealth = startingHealth;
        currHealth = (totalHealth * 1f);
    }

    public void Init(Vector3 whereImAt, Transform TransformToFollow)
    {
        health = new List<HeartObject>();
        int startingHealth = totalHealth;
        for (int i = 0; i < startingHealth; i++)
        {
            AddHealth();
        }
        //reset just for start function
        totalHealth = startingHealth;
        currHealth = (totalHealth * 1f);
        startPos = whereImAt;
        HealthBarTransform = this.transform;
        followTransform = TransformToFollow;
    }

    public void UpdateHpOnSceneLoad(float sceneHp)
    {
        if (sceneHp < currHealth)
        {
            Damage(currHealth - sceneHp);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) { Heal(1.25f); }
        if (Input.GetKeyDown(KeyCode.I)) { Damage(1.5f); }
        if (Input.GetKeyDown(KeyCode.O)) { AddHealth(); }
        if (Input.GetKeyDown(KeyCode.P)) { RemoveHealth(1); }
        if (!followTransform.Equals(null)) { this.transform.position = followTransform.position + new Vector3(-0.75f, 1f, 0f); }
    }

    public void AddHealth()
    {
        health.Add(MakeHeart());
        totalHealth++;
        currHealth++;
        if (health.Count > 1)
        {
            for (int i = 0; i < health.Count - 1; i++)
            {
                health[i].transform.localPosition = new Vector3(health[i].transform.localPosition.x + padding, startPos.y, startPos.z);
            }
        }
    }

    public void RemoveHealth(int numHealth)
    {
        if (numHealth < 1 || totalHealth < 0) { return; }
        if (numHealth > totalHealth) { numHealth = totalHealth; }
        int tempCount = health.Count;
        for (int i = 0; i < numHealth; i++)
        {
            HeartObject temp = health[0];
            health.RemoveAt(0);
            Destroy(temp.gameObject);
        }
        totalHealth -= numHealth;
        if (totalHealth < currHealth) { currHealth = totalHealth; }
    }

    public void Heal(float amount)
    {
        Debug.Log("Before Heal: " + currHealth);
        if (amount < 0.25f) //amount is too small
        {
            return;
        }
        if (currHealth >= (totalHealth * 1.0f)) //fully healed
        {
            return;
        }
        currHealth = 0f;
        for (int i = health.Count - 1; i >= 0; i--)
        {
            Debug.Log("Before: " + health[i].health);
            amount = health[i].Heal(amount);
            Debug.Log("After: " + health[i].health);
            health[i].renderer.sprite = healthSprites[(int)(health[i].health * 4)];
            currHealth += health[i].health;
        }
    }
    public void Damage(float amount)
    {
        if (amount < 0.25f) //amount too small
        {
            return;
        }
        if (currHealth == 0f) //no health to remove;
        {
            return;
        }
        float diff = 0f;
        foreach (HeartObject heart in health)
        {
            if (amount > 0f)
            {
                diff = heart.health;
                amount = heart.Damage(amount);
                diff -= heart.health;
                heart.renderer.sprite = healthSprites[4 - (int)(heart.health * 4)];
                currHealth -= diff;
            }   
        }
        Debug.Log("currHealth: " + currHealth);
    }

    public HeartObject MakeHeart()
    {
        HeartObject cloneHeartObject = Instantiate(pfHeart, new Vector3(0,0,0), Quaternion.identity, HealthBarTransform);
        cloneHeartObject.transform.localPosition = startPos;
        cloneHeartObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        return cloneHeartObject;
    }
}
