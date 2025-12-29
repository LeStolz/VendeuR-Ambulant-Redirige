using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.XR.Haptics;

public class HapticManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_8 = new(0.8f);

    public enum Hand
    {
        Left,
        Right,
        Both
    }

    XRController leftController;
    XRController rightController;
    SendHapticImpulseCommand shortImpulse = SendHapticImpulseCommand.Create(0, 1f, 0.4f);
    SendHapticImpulseCommand shorterImpulse = SendHapticImpulseCommand.Create(0, 0.4f, 0.1f);
    SendHapticImpulseCommand longImpulse = SendHapticImpulseCommand.Create(0, 0.6f, 1f);

    public static HapticManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        leftController = InputSystem.GetDevice<XRController>(CommonUsages.LeftHand);
        rightController = InputSystem.GetDevice<XRController>(CommonUsages.RightHand);
    }

    public void TriggerInteractionHaptic(Hand hand)
    {
        TriggerHaptic(hand, shorterImpulse);
    }

    public void TriggerSucessHaptic(Hand hand)
    {
        IEnumerator TriggerSucessHapticCoroutine()
        {
            TriggerHaptic(hand, shortImpulse);
            yield return _waitForSeconds0_8;
            TriggerHaptic(hand, shortImpulse);
        }

        StartCoroutine(TriggerSucessHapticCoroutine());
    }

    public void TriggerFailureHaptic(Hand hand)
    {
        TriggerHaptic(hand, longImpulse);
    }

    void TriggerHaptic(Hand hand, SendHapticImpulseCommand command)
    {
        leftController ??= InputSystem.GetDevice<XRController>(CommonUsages.LeftHand);
        rightController ??= InputSystem.GetDevice<XRController>(CommonUsages.RightHand);

        switch (hand)
        {
            case Hand.Left:
                leftController?.ExecuteCommand(ref command);
                break;
            case Hand.Right:
                rightController?.ExecuteCommand(ref command);
                break;
            case Hand.Both:
                leftController?.ExecuteCommand(ref command);
                rightController?.ExecuteCommand(ref command);
                break;
        }
    }
}
