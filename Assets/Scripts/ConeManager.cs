using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConeManager : MonoBehaviour
{
    public GameObject currentCone;

    [SerializeField] private GameObject conePrefab;

    private XRGrabInteractable grabInteractable;
    private Vector3 initialPosition;
    private GameObject lastCone = null;

    void Start()
    {
        SetupCone(currentCone);
    }

    private void SetupCone(GameObject cone)
    {
        grabInteractable = cone.GetComponent<XRGrabInteractable>();
        initialPosition = cone.transform.position;

        grabInteractable.selectEntered.AddListener(OnConeGrabbed);
        grabInteractable.selectExited.AddListener(OnConeReleased);
    }

    private void OnConeGrabbed(SelectEnterEventArgs args)
    {
        StoreCurrentConeAsLast();
        SpawnCone();
    }

    private void OnConeReleased(SelectExitEventArgs args)
    {
        lastCone.GetComponent<Rigidbody>().isKinematic = false;
        lastCone.GetComponent<XRGrabInteractable>().selectExited.RemoveListener(OnConeReleased);
        lastCone.transform.SetParent(null);
    }

    private void StoreCurrentConeAsLast()
    {
        lastCone = currentCone;
    }

    private void SpawnCone()
    {
        currentCone = Instantiate(conePrefab, initialPosition, Quaternion.identity);
        currentCone.transform.SetParent(transform);
        currentCone.GetComponent<Rigidbody>().isKinematic = true;

        grabInteractable.selectEntered.RemoveListener(OnConeGrabbed);
        SetupCone(currentCone);
    }
}
