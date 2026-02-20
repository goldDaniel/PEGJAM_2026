using UnityEngine;
using UnityEngine.UIElements.Experimental;

public enum UIEaseType
{
	Linear,
	SineIn,
	SineOut,
	SineInOut,
	CubicIn,
	CubicOut,
	CubicInOut,
	ExpoOut,
	BackOut,
	BounceOut,
	AnimationCurve,
}

public enum UIAnimationLoopMode
{
	None,
	Loop,
	PingPong,
}

public enum UITransition
{
	Enter,
	Exit,
}

public enum UITransitionInterruptMode
{
	StopAndPlay,
	ReverseCurrent,
}

public enum UIPositionSpace
{
	Anchored,
	World,
}

public static class UIEase
{
	public static float Evaluate(UIEaseType easeType, float t, AnimationCurve customCurve = null)
	{
		t = Mathf.Clamp01(t);

		switch (easeType)
		{
			case UIEaseType.SineIn:
				return Easing.InSine(t);
			case UIEaseType.SineOut:
				return Easing.OutSine(t);
			case UIEaseType.SineInOut:
				return Easing.InOutSine(t);
			case UIEaseType.CubicIn:
				return Easing.InCubic(t);
			case UIEaseType.CubicOut:
				return Easing.OutCubic(t);
			case UIEaseType.CubicInOut:
				return Easing.InOutCubic(t);
			case UIEaseType.ExpoOut:
				return Easing.OutPower(t, 6);
			case UIEaseType.BackOut:
				return Easing.OutBack(t);
			case UIEaseType.BounceOut:
				return Easing.OutBounce(t);
			case UIEaseType.AnimationCurve:
				return customCurve != null ? customCurve.Evaluate(t) : t;
			case UIEaseType.Linear:
			default:
				return t;
		}
	}
}
