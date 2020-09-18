using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAnimationHandler : MonoBehaviour
{
    public SlimeMovement SM_instance;

    public void chargeStart()
    {
        SM_instance.chargeDone = false;
    }

    public void chargeDone()
    {
        SM_instance.chargeDone = true;
    }

    public void Damaged()
    {
        SM_instance.GotDamaged();
    }
}
