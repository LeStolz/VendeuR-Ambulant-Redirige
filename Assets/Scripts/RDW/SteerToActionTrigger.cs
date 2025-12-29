using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SteerToActionTrigger : MonoBehaviour
{
    [SerializeField] private float timeInterval = 2.1f;
    [SerializeField] private static float lastTriggerTime  = -Mathf.Infinity;
    private XRGrabInteractable xrGrabInteractable;

    void Start()
    {
        xrGrabInteractable = GetComponent<XRGrabInteractable>();
        xrGrabInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (Time.time - lastTriggerTime < timeInterval)
        {
            return;
        }
        lastTriggerTime = Time.time;
        RedirectionController.Instance.StarSteerToAction();
    }

    private void OnDestroy()
    {
        if (xrGrabInteractable != null)
        {
            xrGrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }
}
