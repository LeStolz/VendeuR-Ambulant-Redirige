using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Door : MonoBehaviour
{
    private Vector3 localPosition;
    private Quaternion localRotation;

    void Start()
    {
        // doorHandler = transform.GetChild(0).gameObject;
        
        SaveTransform();
    }

    public void SaveTransform()
    {
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        // if (doorHandler != null)
        // {
        //     positionDoorHandler = doorHandler.transform.localPosition;
        //     rotationDoorHandler = doorHandler.transform.localRotation;
        // }
    }

    public void ResetTransform()
    {        
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
        // if (doorHandler != null)
        // {
        //     doorHandler.transform.localPosition = positionDoorHandler;
        //     doorHandler.transform.localRotation = rotationDoorHandler;
        // }
    }
}
