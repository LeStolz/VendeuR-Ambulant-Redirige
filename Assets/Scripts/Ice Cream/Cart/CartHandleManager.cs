using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Linq;

[RequireComponent(typeof(XRGrabInteractable))]
public class CartHandleManager : MonoBehaviour
{
    struct Child
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public ConfigurableJoint Joint;
    }

    [SerializeField] List<AudioSource> cartAudioSources;
    [SerializeField] List<ConeContainer> coneContainers;
    [SerializeField] List<GameObject> objects = new();

    private List<Child> objectLastTransforms = new();

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        SaveCurrentTransforms();
    }

    private void SaveCurrentTransforms()
    {
        objectLastTransforms.Clear();

        foreach (var obj in objects)
        {
            Quaternion localRot = Quaternion.Inverse(transform.rotation) * obj.transform.rotation;

            if (obj.TryGetComponent<ConfigurableJoint>(out var joint))
            {
                Vector3 localPos = joint.connectedAnchor;

                objectLastTransforms.Add(new Child
                {
                    localPosition = localPos,
                    localRotation = localRot,
                    Joint = joint
                });
            }
            else
            {
                Vector3 localPos = transform.InverseTransformPoint(obj.transform.position);

                objectLastTransforms.Add(new Child
                {
                    localPosition = localPos,
                    localRotation = localRot,
                    Joint = null
                });
            }
        }
    }

    private void UpdateTransforms()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            obj.GetComponent<Rigidbody>().useGravity = false;
            obj.GetComponent<BoxCollider>().enabled = false;

            if (objectLastTransforms[i].Joint == null)
            {
                obj.transform.SetPositionAndRotation(
                    transform.TransformPoint(objectLastTransforms[i].localPosition),
                    transform.rotation * objectLastTransforms[i].localRotation
                );
            }
        }
    }

    private void FreeCoordinates()
    {
        foreach (var obj in objectLastTransforms)
        {
            if (obj.Joint == null) continue;
            var jt = obj.Joint;

            jt.xMotion = ConfigurableJointMotion.Locked;
            jt.yMotion = ConfigurableJointMotion.Locked;
            jt.zMotion = ConfigurableJointMotion.Locked;
        }
    }

    private void LockCoordinates()
    {
        foreach (var obj in objectLastTransforms)
        {
            if (obj.Joint == null) continue;
            var jt = obj.Joint;

            jt.xMotion = ConfigurableJointMotion.Limited;
            jt.yMotion = ConfigurableJointMotion.Locked;
            jt.zMotion = ConfigurableJointMotion.Locked;
        }
    }

    private void UpdateColliderStates()
    {
        foreach (var obj in objects)
        {
            obj.GetComponent<BoxCollider>().enabled = true;
            obj.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    private void Update()
    {
        if (grabInteractable.isSelected && !cartAudioSources.Any(audioSource => audioSource.isPlaying))
        {
            cartAudioSources.ForEach(audioSource => audioSource.Play());
        }
        else if (!grabInteractable.isSelected && cartAudioSources.Any(audioSource => audioSource.isPlaying))
        {
            cartAudioSources.ForEach(audioSource => audioSource.Pause());
        }

        if (
            Vector3.Distance(transform.position, lastPosition) > GlobalThresholds.EPS
            || Quaternion.Angle(transform.rotation, lastRotation) > 10 * GlobalThresholds.EPS
            || grabInteractable.isSelected
        )
        {
            foreach (var coneManager in coneContainers) coneManager.ToggleConeVisibility(false);

            FreeCoordinates();
            UpdateTransforms();
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        else
        {
            LockCoordinates();
            UpdateColliderStates();

            foreach (var coneManager in coneContainers) coneManager.ToggleConeVisibility(true);
        }
    }
}
