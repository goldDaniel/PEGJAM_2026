using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Goblin", menuName = "Scriptable Objects/UI Animation Clip")]
public sealed class UIAnimationClip : ScriptableObject
{
	[SerializeField, Min(0.01f)]
	private float _duration = 0.2f;

	[SerializeField, Min(0f)]
	private float _delay;

	[SerializeField]
	private UIAnimationLoopMode _loopMode = UIAnimationLoopMode.None;

	[SerializeField]
	private UIEaseType _defaultEase = UIEaseType.SineOut;

	[SerializeField]
	private AnimationCurve _customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private PositionTrack[] _positionTracks = Array.Empty<PositionTrack>();

	[SerializeField]
	private ScaleTrack[] _scaleTracks = Array.Empty<ScaleTrack>();

	[SerializeField]
	private AlphaTrack[] _alphaTracks = Array.Empty<AlphaTrack>();

	public float Duration => _duration;
	public float Delay => _delay;
	public UIAnimationLoopMode LoopMode => _loopMode;
	public UIEaseType DefaultEase => _defaultEase;
	public AnimationCurve CustomCurve => _customCurve;
	public PositionTrack[] PositionTracks => _positionTracks;
	public ScaleTrack[] ScaleTracks => _scaleTracks;
	public AlphaTrack[] AlphaTracks => _alphaTracks;
	
	public static UIAnimationClip CreateRuntimeAnchoredMoveClip(Vector2 fromPos, Vector2 Pos, float duration, UIEaseType ease)
	{
		UIAnimationClip clip = CreateInstance<UIAnimationClip>();
		clip.hideFlags = HideFlags.DontSave;
		clip._duration = Mathf.Max(0.01f, duration);
		clip._delay = 0f;
		clip._loopMode = UIAnimationLoopMode.None;
		clip._defaultEase = ease;
		clip._positionTracks = new[]
		{
			new PositionTrack
			{
				Enabled = true,
				Space = UIPositionSpace.World,
				Ease = ease,
				From = fromPos,
				To = Pos,
			},
		};
		clip._scaleTracks = Array.Empty<ScaleTrack>();
		clip._alphaTracks = Array.Empty<AlphaTrack>();
		return clip;
	}

	public static UIAnimationClip CreateRuntimeScaleClip(Vector2 fromScale, Vector2 toScale, float duration, UIEaseType ease)
	{
		UIAnimationClip clip = CreateInstance<UIAnimationClip>();
		clip.hideFlags = HideFlags.DontSave;
		clip._duration = Mathf.Max(0.01f, duration);
		clip._delay = 0f;
		clip._loopMode = UIAnimationLoopMode.None;
		clip._defaultEase = ease;
		clip._scaleTracks = new[]
		{
			new ScaleTrack
			{
				Enabled = true,
				Ease = ease,
				From = fromScale,
				To = toScale,
			},
		};
		return clip;
	}
}

[Serializable]
public struct PositionTrack
{
	public bool Enabled;
	public UIEaseType Ease;
	public UIPositionSpace Space;
	public Vector2 From;
	public Vector2 To;
}

[Serializable]
public struct ScaleTrack
{
	public bool Enabled;
	public UIEaseType Ease;
	public Vector2 From;
	public Vector2 To;
}

[Serializable]
public struct AlphaTrack
{
	public bool Enabled;
	public UIEaseType Ease;
	[Range(0f, 1f)] public float From;
	[Range(0f, 1f)] public float To;
}
