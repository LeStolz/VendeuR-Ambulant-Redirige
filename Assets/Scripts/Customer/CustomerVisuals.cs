using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerVisuals : MonoBehaviour
{
	Transform target;

	[SerializeField] List<Eye> eyes;
	List<Vector3> initialEyeLocalPositions = new();
	float eyeMaxOffset;

	[SerializeField] float turnToPlayerMaxThresholdDegrees = 45f;
	[SerializeField] float turnToPlayerMinThresholdDegrees = 5f;
	[SerializeField] float turningSpeed = 2f;
	[SerializeField] float rotateToPlayerDistance = 15f;
	bool turningToPlayer = false;

	[SerializeField] GameObject orderDisplayUI;
	[SerializeField] GameObject alertUI;

	void Start()
	{
		target = Camera.main.transform;

		foreach (var eye in eyes)
		{
			initialEyeLocalPositions.Add(eye.transform.localPosition);
			eyeMaxOffset = eye.transform.parent.localScale.x * (1 - eye.transform.localScale.x / 1.7f);
		}
	}



	IEnumerator SetNormalEmotionAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		SetEmotion(EyeEmotion.Normal);
	}

	public void SetEmotion(EyeEmotion emotion)
	{
		foreach (var eye in eyes)
		{
			eye.SetEmotion(emotion);
		}

		StopAllCoroutines();
		if (emotion != EyeEmotion.Normal)
		{
			StartCoroutine(SetNormalEmotionAfterDelay(2f));
		}
	}

	public void HandlePlayerEnterRange()
	{
		orderDisplayUI.SetActive(true);
		alertUI.SetActive(false);
	}

	public void HandlePlayerExitRange()
	{
		orderDisplayUI.SetActive(false);
		alertUI.SetActive(true);
	}

	void Update()
	{
		var distanceToPlayer = Vector3.Distance(transform.position, target.position);

		alertUI.SetActive(distanceToPlayer < rotateToPlayerDistance && !orderDisplayUI.activeSelf);

		if (target == null || distanceToPlayer > rotateToPlayerDistance) return;

		for (int i = 0; i < eyes.Count; i++)
		{
			Vector3 directionToTarget = (target.position - eyes[i].transform.position).normalized;
			Vector3 lookDirection = eyes[i].transform.parent.InverseTransformDirection(directionToTarget);

			Vector3 offset = new(
				Mathf.Clamp(lookDirection.x, -eyeMaxOffset, eyeMaxOffset),
				0,
				Mathf.Clamp(lookDirection.z, -eyeMaxOffset, eyeMaxOffset)
			);

			eyes[i].transform.localPosition = initialEyeLocalPositions[i] + offset;
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