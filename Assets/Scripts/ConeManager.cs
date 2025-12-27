using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConeManager : MonoBehaviour
{
    public GameObject currentCone;

    [SerializeField] private GameObject conePrefab;
    [SerializeField] private GameObject coneReference;

    private XRGrabInteractable grabInteractable;
    private GameObject lastCone = null;

    void Start()
    {
        SetupCone(currentCone);
    }

    private void SetupCone(GameObject cone)
    {
        grabInteractable = cone.GetComponent<XRGrabInteractable>();

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
        Debug.Log("pos : " + coneReference.transform.position);
        currentCone = Instantiate(conePrefab, coneReference.transform.position, Quaternion.identity);
        currentCone.transform.SetParent(transform);
        currentCone.GetComponent<Rigidbody>().isKinematic = true;

        grabInteractable.selectEntered.RemoveListener(OnConeGrabbed);
        SetupCone(currentCone);
    }
}
