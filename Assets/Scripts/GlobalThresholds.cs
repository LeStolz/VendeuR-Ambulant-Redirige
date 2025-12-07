using System;
using UnityEngine;

class GlobalThresholds : ScriptableObject
{
	public static float INTERACTION_ACTIVE_THRESHOLD = 0.95f;
	public static TimeSpan INTERACTION_ACTIVE_DEBOUNCE = TimeSpan.FromSeconds(1);
}