using System;
using UnityEngine;

class GlobalThresholds : ScriptableObject
{
	public static float INTERACTION_ACTIVE_THRESHOLD = 0.9f;
	public static TimeSpan INTERACTION_ACTIVE_DEBOUNCE = TimeSpan.FromSeconds(1);
	public static float EPS = 1e-3f;
	public static float ANG_EPS = 5f;
}