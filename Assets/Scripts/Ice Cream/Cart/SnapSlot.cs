using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapSlot : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private XRGrabInteractable grabInteractable;
    private float moveDuration = 0.1f;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnSelectExited);

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private IEnumerator MoveToInitial(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
    private void OnSelectExited(SelectExitEventArgs args)
    {
        if(Vector3.Distance(transform.position, initialPosition) < 0.2f)
        {
            StartCoroutine(MoveToInitial(initialPosition, initialRotation));
        }
    }
}
