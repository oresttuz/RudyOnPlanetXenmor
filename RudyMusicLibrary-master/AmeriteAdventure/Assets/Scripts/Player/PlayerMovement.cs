﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //refrences to objects/variables in inspector
    public Rigidbody rb;
    public GameObject pfHB, pfSpear;
    public HealthBar hb_instance;
    public List<GameObject> weapons;
    
    //editable variables to analyze runtime
    public LayerMask enemyLayers, hurtsMe;
    public float attackRange, moveSpeed, dashSpeed, dashDuration;
    public List<int> weaponOneAttacks, weaponTwoAttacks;
    public AttackData[] playerKnowledge = new AttackData[4];

    public bool dashing = false, attacking = false;
    //variables specific to this instance
    private WeaponAnimationHandler wah1_Instance, wah2_Instance;
    
    private float dashCount = 0.0f;
    private int whichWeapon = -1; //-1 = first, 1 = second
    private Vector3 movement, dash, throwForce = new Vector3(20f, 0f, 20f);
    private int w1AttackIndex = 0, w2AttackIndex = 0;
    private Vector2 origin = new Vector2(0f, 0f);

    private void Start()
    {
        //----------  Section: Code Independent of the player having weapons  ----------//
        movement = new Vector3(0f, 0f, 0f);
        dash = new Vector3(0f, 0f, 0f);

        if (hb_instance == null)
        {
            hb_instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
            hb_instance.Init();
        }

        if (weapons == null) { weapons = new List<GameObject>(); } 
        if (weaponOneAttacks == null) { weaponOneAttacks = new List<int>(); }
        if (weaponTwoAttacks == null) { weaponTwoAttacks = new List<int>(); }
        if (weaponOneAttacks.Count < 0) { weaponOneAttacks.Add(0); }
        if (weaponTwoAttacks.Count < 0) { weaponTwoAttacks.Add(0); }

        for (int ad = 0; ad < playerKnowledge.Length; ++ad)
        {
            playerKnowledge[ad] = new AttackData(ad);
        }
        //end of section

        //----------  Section: Weapon Code  ----------//
        if (transform.childCount >= 3)
        {
            weapons.Add(transform.GetChild(2).gameObject);
            InitWeapon(0);
        }
        if (transform.childCount >= 4)
        {
            weapons.Add(transform.GetChild(3).gameObject);
            InitWeapon(1);
        }
        //end of section
    }

    public void UpdatePlayerOnSceneLoad(float sceneHp)
    {
        if (hb_instance == null)
        {
            hb_instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
            hb_instance.Init();
        }
        hb_instance.UpdateHpOnSceneLoad(sceneHp);
    }

    // Update is called once per frame
    private void Update()
    {
        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");
        if (weapons.Count > 0)
        {
            if (Input.GetMouseButtonDown(0)) //leftClick
            {
                if (!dashing && !attacking) { Attack1(); }
            }
        }
        if (Input.GetMouseButtonDown(1)) //rightClick
        {
            if (!dashing && !attacking) { Dash1(); }
        }
        if (dashing)
        {
            if (dashCount >= dashDuration)
            {
                dashCount = 0.0f;
                dashing = false;
                dash = new Vector3(0f, 0f, 0f);
                rb.velocity = dash;
            }
            else { dashCount += Time.deltaTime; }
        }
        else { rb.MovePosition(rb.position + (movement * moveSpeed * Time.deltaTime)); }
    }

    private void FixedUpdate()
    {
        if (hb_instance.currHealth <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        if (attacking) { return; }
        if (weapons.Count == 2)
        {
            Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            float angle = -1f * AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);
            if (whichWeapon < 0)
            {
                weapons[0].transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
                weapons[1].transform.rotation = Quaternion.Euler(new Vector3(0f, weapons[1].transform.rotation.eulerAngles.y + 0.5f, 0f));
            }
            else
            {
                weapons[0].transform.rotation = Quaternion.Euler(new Vector3(0f, weapons[0].transform.eulerAngles.y + 0.5f, 0f));
                weapons[1].transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
            }

            if (Input.GetKeyDown(KeyCode.LeftShift)) //toggle Weapon
            {
                whichWeapon *= -1;
                if (whichWeapon < 0)
                {
                    Color newColor = weapons[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color;
                    newColor.a = 1f;
                    weapons[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
                    Color newColor2 = weapons[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color;
                    newColor.a = 0.5f;
                    weapons[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
                }
                else
                {
                    Color newColor = weapons[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color;
                    newColor.a = 0.5f;
                    weapons[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
                    Color newColor2 = weapons[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color;
                    newColor.a = 1f;
                    weapons[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
                }
            }
        }
        else if (weapons.Count == 1)
        {
            Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            float angle = -1f * AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

            weapons[0].transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
        }
    }

    public void AddWeapon(GameObject weapon)
    {
        if (weapons.Count >= 2) { return; }
        weapons.Add(weapon);
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = new Vector3(0f, 0f, 0f);
        weapon.transform.localScale= new Vector3(1f, 1f, 1f);
        InitWeapon(weapons.Count - 1);
    }

    public void InitWeapon(int indexOfWeapon)
    {
        Color newColor = weapons[indexOfWeapon].transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        newColor.a = 1f;
        weapons[indexOfWeapon].transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
        if (indexOfWeapon == 0)
        {
            wah1_Instance = weapons[indexOfWeapon].GetComponentInChildren<WeaponAnimationHandler>();
            wah1_Instance.WeaponAnimator.SetInteger("Type", weaponOneAttacks[0]);
        }
        else if (indexOfWeapon == 1)
        {
            wah2_Instance = weapons[indexOfWeapon].GetComponentInChildren<WeaponAnimationHandler>();
            wah2_Instance.WeaponAnimator.SetInteger("Type", weaponTwoAttacks[0]);
        }
    }

    public void ThrowWeapon()
    {
        if (weapons.Count == 0) { return; }
        int index = 0;
        if (weapons.Count == 2)
        {
            if (whichWeapon > 0) { index = 1; }
        }
        weapons[index].SetActive(false);
        GameObject tempRefToWeapon = Instantiate(pfSpear, weapons[index].transform.GetChild(0).position, weapons[index].transform.rotation, null);
        tempRefToWeapon.transform.rotation = Quaternion.Euler(tempRefToWeapon.transform.rotation.eulerAngles.x, tempRefToWeapon.transform.eulerAngles.y - 90f, tempRefToWeapon.transform.eulerAngles.z);

        float y = tempRefToWeapon.transform.rotation.eulerAngles.y;
        Debug.Log(y);
        y *= Mathf.Deg2Rad;
        Debug.Log(y);

        tempRefToWeapon.GetComponent<Rigidbody>().velocity = new Vector3(throwForce.x * Mathf.Sin(y), 0f, throwForce.z * Mathf.Cos(y));
    }

    public void ReturnWeapon()
    {
        if (weapons.Count == 0) { return; }
        if (weapons.Count == 2)
        {
            if (weapons[1].activeInHierarchy == false) { weapons[1].SetActive(true); }
            else { weapons[0].SetActive(true); }
        }
        else { weapons[0].SetActive(true); }
    }

    public void Attack1()
    {
        attacking = true;
        if (whichWeapon < 0)
        {
            if (weapons[0].activeSelf) { w1AttackIndex = NextAttackAnimation(wah1_Instance, weaponOneAttacks, w1AttackIndex); }
        }
        else
        {
            if (weapons[1].activeSelf) { w2AttackIndex = NextAttackAnimation(wah2_Instance, weaponTwoAttacks, w2AttackIndex); }
        }
    }

    public int NextAttackAnimation(WeaponAnimationHandler InstanceOfWah, List<int> wAttacks, int indexCounter)
    {
        ++indexCounter;
        if (indexCounter >= wAttacks.Count)
        {
            indexCounter = 0;
        }
        //can tint it here //can tint it here //can tint it here //can tint it here //can tint it here //can tint it here //can tint it here
        InstanceOfWah.WeaponAnimator.SetBool("Attacking", true);
        InstanceOfWah.nextType = wAttacks[indexCounter];
        return indexCounter;
    }

    public void Dash1()
    {
        if (!dashing)
        {
            dashing = true;
            dash = movement;
            rb.velocity = dash * dashSpeed; //* Time.deltaTime
        }
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerEnter(Collider objectCollider)
    {
        if (objectCollider.gameObject.layer == hurtsMe)
        {
            if (objectCollider.gameObject.tag == "Enemy")
            {
                hb_instance.Damage(objectCollider.gameObject.GetComponentInParent<EnemyData>().damage);
                if (hb_instance.currHealth < 0f)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    public bool AreThereEnemiesAroundMe()
    {
        Collider[] enemyColliders = Physics.OverlapBox(transform.position, new Vector3(10f, 0.2f, 10f), Quaternion.identity, enemyLayers);
        if (enemyColliders.Length > 0) { return true; }
        return false;
    }

    public void AddAttack(int attackNum)
    {
        if (whichWeapon < 0)
        {
            weaponOneAttacks.Add(attackNum);
        }
        else
        {
            weaponTwoAttacks.Add(attackNum);
        }
    }
}

public class AttackData
{
    public int attackNum;
    public AttackType knowledge;

    public AttackData(int numOfAttack)
    {
        attackNum = numOfAttack;
        knowledge = AttackType.None;
    }
}

public enum AttackType
{
    None = 0,
    Fire = 1,
    Ice = 2,
    Lighting = 3,
    Nature = 4
};