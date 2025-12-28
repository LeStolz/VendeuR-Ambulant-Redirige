using System.Collections.Generic;
using UnityEngine;

public class CustomerVisuals : MonoBehaviour
{
	Transform target;

	[SerializeField] List<Transform> eyes;
	List<Vector3> initialEyeLocalPositions = new();
	float eyeMaxOffset;

	[SerializeField] float turnToPlayerMaxThresholdDegrees = 45f;
	[SerializeField] float turnToPlayerMinThresholdDegrees = 5f;
	[SerializeField] float turningSpeed = 2f;

	[SerializeField] float rotateToPlayerDistance = 15f;

	bool turningToPlayer = false;

	void Start()
	{
		target = Camera.main.transform;

		foreach (var eye in eyes)
		{
			initialEyeLocalPositions.Add(eye.localPosition);
			eyeMaxOffset = eye.parent.localScale.x * (1 - eye.localScale.x / 1.7f);
		}
	}

	void Update()
	{
		if (target == null || Vector3.Distance(transform.position, target.position) > rotateToPlayerDistance) return;

		for (int i = 0; i < eyes.Count; i++)
		{
			Vector3 directionToTarget = (target.position - eyes[i].position).normalized;
			Vector3 lookDirection = eyes[i].parent.InverseTransformDirection(directionToTarget);

			Vector3 offset = new(
				Mathf.Clamp(lookDirection.x, -eyeMaxOffset, eyeMaxOffset),
				0,
				Mathf.Clamp(lookDirection.z, -eyeMaxOffset, eyeMaxOffset)
			);

			eyes[i].localPosition = initialEyeLocalPositions[i] + offset;
		}

		Vector3 toTarget = (target.position - transform.position).normalized;
		toTarget.y = 0;
		if (Vector3.Angle(transform.forward, toTarget) > turnToPlayerMaxThresholdDegrees)
		{
			turningToPlayer = true;
		}

		if (turningToPlayer)
		{
			Vector3 lookDirection = toTarget;
			lookDirection.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);

			if (Vector3.Angle(transform.forward, toTarget) < turnToPlayerMinThresholdDegrees)
			{
				turningToPlayer = false;
			}
		}
	}
}