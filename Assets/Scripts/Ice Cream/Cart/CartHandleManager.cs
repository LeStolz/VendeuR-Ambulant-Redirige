using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Linq;

[RequireComponent(typeof(XRGrabInteractable))]
public class CartHandleManager : MonoBehaviour
{
    struct Child
    {
        public GameObject gameObject;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public ConfigurableJoint Joint;
    }

    [SerializeField] List<AudioSource> cartAudioSources;
    [SerializeField] List<ConeContainer> coneContainers;
    [SerializeField] List<GameObject> objects = new();

    private List<Child> objectLastTransforms = new();

    private Vector3 initialLocalPosition;
    private Vector3 lastLocalPosition;
    private Quaternion lastLocalRotation;

    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        initialLocalPosition = transform.localPosition;

        lastLocalPosition = transform.localPosition;
        lastLocalRotation = transform.localRotation;

        SaveCurrentTransforms();
    }

    private void SaveCurrentTransforms()
    {
        objectLastTransforms.Clear();

        foreach (var obj in objects)
        {
            Quaternion localRot = Quaternion.Inverse(transform.rotation) * obj.transform.rotation;
            Vector3 localPos = transform.InverseTransformPoint(obj.transform.position);

            if (obj.TryGetComponent<ConfigurableJoint>(out var joint))
            {
                localPos = joint.connectedAnchor;
            }

            objectLastTransforms.Add(new Child
            {
                gameObject = obj,
                localPosition = localPos,
                localRotation = localRot,
                Joint = joint
            });
        }
    }

    private void LockMotions()
    {
        foreach (var obj in objectLastTransforms)
        {
            obj.gameObject.GetComponent<Rigidbody>().useGravity = false;
            obj.gameObject.GetComponent<BoxCollider>().enabled = false;

            obj.gameObject.transform.SetPositionAndRotation(
                transform.TransformPoint(obj.localPosition),
                transform.rotation * obj.localRotation
            );

            if (obj.Joint != null)
            {
                obj.Joint.xMotion = ConfigurableJointMotion.Locked;
            }
        }
    }

    private void FreeMotions()
    {
        foreach (var obj in objectLastTransforms)
        {
            obj.gameObject.GetComponent<BoxCollider>().enabled = true;
            obj.gameObject.GetComponent<Rigidbody>().useGravity = true;

            if (obj.Joint != null)
            {
                obj.Joint.xMotion = ConfigurableJointMotion.Limited;
            }
        }
    }

    private void Update()
    {
        bool transformed = Vector3.Distance(transform.localPosition, lastLocalPosition) > GlobalThresholds.EPS
            || Quaternion.Angle(transform.localRotation, lastLocalRotation) > 10 * GlobalThresholds.EPS;

        if (transformed && grabInteractable.isSelected && !cartAudioSources.Any(audioSource => audioSource.isPlaying))
        {
            cartAudioSources.ForEach(audioSource => audioSource.Play());
        }
        else if ((!transformed || !grabInteractable.isSelected) && cartAudioSources.Any(audioSource => audioSource.isPlaying))
        {
            cartAudioSources.ForEach(audioSource => audioSource.Pause());
        }

        if (transformed || grabInteractable.isSelected)
        {
            foreach (var coneManager in coneContainers) coneManager.ToggleConeVisibility(false);
            LockMotions();
        }
        else
        {
            FreeMotions();
            foreach (var coneManager in coneContainers) coneManager.ToggleConeVisibility(true);
        }

        if (transform.localPosition.y < -10f || Vector3.Magnitude(transform.localPosition) > 1000f)
        {
            transform.localPosition = initialLocalPosition;
        }

        lastLocalPosition = transform.localPosition;
        lastLocalRotation = transform.localRotation;
    }
}