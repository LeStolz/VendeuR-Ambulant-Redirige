using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConeContainer : MonoBehaviour
{
    [SerializeField] ConeFlavorSO coneFlavor;
    [SerializeField] IceCream conePrefab;

    [SerializeField] Transform coneSpawnLocation;

    private XRGrabInteractable grabInteractable;
    private IceCream lastCone = null;
    private IceCream currentCone;

    void Start()
    {
        SpawnCone();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.name.Contains("cone"))
                {
                    mat.color = coneFlavor.color;
                }
            }
        }
    }

    private void SetupCone(IceCream cone)
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
        currentCone = Instantiate(conePrefab, coneSpawnLocation.position, Quaternion.identity).GetComponent<IceCream>();
        currentCone.transform.SetParent(transform);
        currentCone.GetComponent<Rigidbody>().isKinematic = true;
        currentCone.Initialize(coneFlavor);

        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnConeGrabbed);

        SetupCone(currentCone);
    }

    public void ToggleConeVisibility(bool on)
    {
        if (currentCone != null)
            currentCone.gameObject.SetActive(on);
    }
}
