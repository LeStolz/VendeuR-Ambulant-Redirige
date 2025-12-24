using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DoorController : MonoBehaviour
{
    public List<Door> doors;
    public XRGrabInteractable handHeld;

    void Update()
    {
        if (!handHeld.isSelected)
        {
            foreach (Door door in doors)
            {
                door.SaveTransform();
            }
        }
        else
        {
            foreach (Door door in doors)
            {
                door.ResetTransform();
            }
        }
    }
}
