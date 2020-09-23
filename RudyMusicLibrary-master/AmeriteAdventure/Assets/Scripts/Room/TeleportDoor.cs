using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    public RoomGeneration myParentInstance, myOtherParent;
    public int startDoor = -1;
    public int myX, myY, theirX, theirY;
    public GameObject DoorToTeleportTo;
    public Vector3 shiftInFrontOfDoorVec;
    public bool isLocked;

    public void Init(RoomGeneration ins, int thisX, int thisY, int thatX, int thatY, GameObject door, Vector3 shift)
    {
        myParentInstance = ins;
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
                myParentInstance.EnableRooms(myX, myY, false);
                if (startDoor >= 0)
                {
                    myOtherParent.EnableRooms(theirX, theirY, true);
                }
                else
                {
                    myParentInstance.EnableRooms(theirX, theirY, true);
                }
                Debug.Log(DoorToTeleportTo.name);
                Debug.Log("Where Imma go: " + DoorToTeleportTo.transform.position + shiftInFrontOfDoorVec);
                collision.collider.gameObject.transform.position = DoorToTeleportTo.transform.position + shiftInFrontOfDoorVec;
                Debug.Log("I be here: " + collision.collider.gameObject.transform.position);
            }
        }
    }
}
