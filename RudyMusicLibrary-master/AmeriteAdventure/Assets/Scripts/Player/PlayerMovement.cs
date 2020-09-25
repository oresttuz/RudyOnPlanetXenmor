using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //refrences to objects/variables in inspector
    public Rigidbody rb;
    public GameObject FirstWeapon, SecondWeapon, pfHB;
    public Transform FirstWeaponCenter, SecondWeaponCenter;
    public HealthBar hb_instance;
    
    //editable variables to analyze runtime
    public LayerMask enemyLayers, hurtsMe;
    public float attackRange, moveSpeed = 5f, dashSpeed = 10f, dashDuration = 5.0f;
    public List<int> weaponOneAttacks, weaponTwoAttacks;
    

    //variables specific to this instance
    private WeaponAnimationHandler wah1_Instance, wah2_Instance;
    private bool dashing = false;
    private float dashCount = 0.0f;
    private int whichWeapon = -1; //-1 = first, 1 = second
    private Vector3 movement, dash;
    private int w1AttackIndex = 0, w2AttackIndex = 0;

    private void Start()
    {
        if (weaponOneAttacks == null) { weaponOneAttacks = new List<int>(); }
        if (weaponTwoAttacks == null) { weaponTwoAttacks = new List<int>(); }
        if (weaponOneAttacks.Count < 0) { weaponOneAttacks.Add(1); }
        if (weaponTwoAttacks.Count < 0) { weaponTwoAttacks.Add(1); }

        movement = new Vector3(0f, 0f, 0f);
        dash = new Vector3(0f, 0f, 0f);

        wah1_Instance = this.transform.GetChild(0).GetComponentInChildren<WeaponAnimationHandler>();
        wah2_Instance = this.transform.GetChild(1).GetComponentInChildren<WeaponAnimationHandler>();

        hb_instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
        hb_instance.Init();

        Color newColor = FirstWeapon.GetComponent<SpriteRenderer>().color;
        newColor.a = 1f;
        FirstWeapon.GetComponent<SpriteRenderer>().color = newColor;
        wah1_Instance.WeaponAnimator.SetInteger("Type", weaponOneAttacks[0]); 
        Color newColor2 = SecondWeapon.GetComponent<SpriteRenderer>().color;
        newColor.a = 0.5f;
        SecondWeapon.GetComponent<SpriteRenderer>().color = newColor;
        wah2_Instance.WeaponAnimator.SetInteger("Type", weaponTwoAttacks[0]);
    }

    public void UpdatePlayerOnSceneLoad(float sceneHp)
    {
        hb_instance.UpdateHpOnSceneLoad(sceneHp);
    }

    // Update is called once per frame
    void Update()
    {
        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");
        if (Input.GetMouseButtonDown(0)) //leftClick
        {
            Attack1();
        }
        if (Input.GetMouseButtonDown(1)) //rightClick
        {
            Dash1();
        }
    }

    private void FixedUpdate()
    {
        if (dashing)
        {
            rb.MovePosition(rb.position + dash * dashSpeed * Time.fixedDeltaTime);
            if (dashCount >= dashDuration)
            {
                dashCount = 0.0f;
                dashing = false;
                dash = new Vector3(0f, 0f, 0f);
            }
            else
            {
                dashCount++;
            }
        }
        else
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        //Get the Screen positions of the object
        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = -1f * AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //Ta Daaa
        if (whichWeapon < 0)
        {
            FirstWeaponCenter.transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
            SecondWeaponCenter.transform.rotation = Quaternion.Euler(new Vector3(0f, SecondWeaponCenter.transform.rotation.eulerAngles.y + 0.5f, 0f));
        }
        else
        {
            FirstWeaponCenter.transform.rotation = Quaternion.Euler(new Vector3(0f, FirstWeaponCenter.transform.eulerAngles.y + 0.5f, 0f));
            SecondWeaponCenter.transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift)) //toggle Weapon
        {
            whichWeapon *= -1;
            if (whichWeapon < 0)
            {
                Color newColor = FirstWeapon.GetComponent<SpriteRenderer>().color;
                newColor.a = 1f;
                FirstWeapon.GetComponent<SpriteRenderer>().color = newColor;
                Color newColor2 = SecondWeapon.GetComponent<SpriteRenderer>().color;
                newColor.a = 0.5f;
                SecondWeapon.GetComponent<SpriteRenderer>().color = newColor;
            }
            else
            {
                Color newColor = FirstWeapon.GetComponent<SpriteRenderer>().color;
                newColor.a = 0.5f;
                FirstWeapon.GetComponent<SpriteRenderer>().color = newColor;
                Color newColor2 = SecondWeapon.GetComponent<SpriteRenderer>().color;
                newColor.a = 1f;
                SecondWeapon.GetComponent<SpriteRenderer>().color = newColor;
            }
        }
    }

    public void Attack1()
    {
        if (whichWeapon < 0)
        {
            w1AttackIndex = NextAttackAnimation(wah1_Instance, weaponOneAttacks, w1AttackIndex);
        }
        else
        {
            w2AttackIndex = NextAttackAnimation(wah2_Instance, weaponTwoAttacks, w2AttackIndex);
        }
    }

    public int NextAttackAnimation(WeaponAnimationHandler InstanceOfWah, List<int> wAttacks, int indexCounter)
    {
        ++indexCounter;
        if (indexCounter >= wAttacks.Count)
        {
            indexCounter = 0;
        }
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
        }
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerEnter(Collider objectCollider)
    {
        if (objectCollider.gameObject.layer == hurtsMe)
        {
            if (objectCollider.gameObject.tag == "Enemy")
            {
                hb_instance.Damage(objectCollider.gameObject.GetComponentInParent<EnemyData>().dmg);
                if (hb_instance.currHealth < 0f)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

}

public class Technique
{
    /*
     * Techniques currently only vary by attackRange
     * Shape, hitbox type, attack type, time to activate, time to finish animation, counterattacks, etc...
     * are not implemented
     * This class will be used to store all relevant data for attacks
     * This may later include "Knowledge" (the magic system)
     * Techniques {for now} do not need to worry if the attack has a prereq such as dashing or parrying
     * That logic will be left to the main PlayerMovement class for now
     * 
     * actions? delegates?
     */

    public float damage, attackRange;

    public void Func(GameObject weapon, LayerMask layersAffected, Animator weaponAnimator)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(weapon.transform.position, attackRange, layersAffected);
        weaponAnimator.SetBool("Attacking", true);

        foreach (Collider enemy in hitEnemies)
        {
            
            //add dmg
            
        }
    }
}