using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    public int myX, myY, theirX, theirY;
    public GameObject DoorToTeleportTo;
    public Vector3 shiftInFrontOfDoorVec;
    public bool isLocked;

    public void Init(int thisX, int thisY, int thatX, int thatY, GameObject door, Vector3 shift)
    {
        myX = thisX;
        myY = thisY;
        theirX = thatX;
        theirY = thatY;
        DoorToTeleportTo = door;
        shiftInFrontOfDoorVec = shift;
        isLocked = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(isLocked);
        if (!isLocked)
        {
            if (collision.collider.gameObject.tag == "Player")
            {
                this.GetComponentInParent<RoomGeneration>().EnableRooms(myX, myY, false);
                this.GetComponentInParent<RoomGeneration>().EnableRooms(theirX, theirY, true);
                collision.collider.gameObject.transform.position = DoorToTeleportTo.transform.position + shiftInFrontOfDoorVec;
            }
        }
    }
}
