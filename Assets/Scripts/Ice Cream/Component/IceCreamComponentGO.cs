using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class IceCreamComponentGO : MonoBehaviour
{
    [SerializeField] ToppingSO topping;
    [SerializeField] IceCreamFlavorSO flavor;

    [SerializeField] AudioSource sound;
    [SerializeField] Material sprinklesMaterial;
    [SerializeField] XRGrabInteractable grabInteractable;
    public IIceCreamComponent component;

    void Start()
    {
        if (topping != null)
        {
            component = new IceCreamToppingComponent { topping = topping, sprinklesMaterial = sprinklesMaterial };
        }
        else
        {
            component = new IceCreamFlavorComponent { flavor = flavor };
        }
    }

    public void Consume()
    {
        component.Consume(gameObject);
        sound.Play();

        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            HapticManager.Instance.TriggerInteractionHaptic(
                interactor.handedness == InteractorHandedness.Left ? HapticManager.Hand.Left : HapticManager.Hand.Right
            );
        }
    }

    public void Interact()
    {
        sound.Play();

        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            HapticManager.Instance.TriggerInteractionHaptic(
                interactor.handedness == InteractorHandedness.Left ? HapticManager.Hand.Left : HapticManager.Hand.Right
            );
        }
    }
}
