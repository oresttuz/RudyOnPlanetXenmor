using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTowardsExit : MonoBehaviour
{
    public Transform exitDoorTransform, playerTransform;

    private void FixedUpdate()
    {
        if (exitDoorTransform == null) { return; }
        Vector2 positionOnScreen = new Vector2(playerTransform.position.x, playerTransform.position.z);
        Vector2 exitdoor = new Vector2(exitDoorTransform.position.x, exitDoorTransform.position.z);
        float angle = -1f * AngleBetweenTwoPoints(positionOnScreen, exitdoor);
        transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
