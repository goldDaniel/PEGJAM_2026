using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public sealed class UIAnimator : MonoBehaviour
{
	private sealed class ActiveAnimation
	{
		public int Id;
		public UIAnimationClip Clip;
		public UIAnimOptions Options;
		public float Time;
		public bool IsPaused;
	}

	[SerializeField]
	private UIAnimationTarget _target;

	private readonly List<ActiveAnimation> _activeAnimations = new List<ActiveAnimation>();
	private int _nextId = 1;

	private void Awake()
	{
		if (_target == null)
			_target = GetComponent<UIAnimationTarget>();
	}

	private void LateUpdate()
	{
		if (_target == null || _activeAnimations.Count == 0)
			return;

		float dt = Time.deltaTime;
		ApplyAnimations(dt);
	}

	public UIAnimHandle Play(UIAnimationClip clip, UIAnimOptions options = default)
	{
		if (_target == null || clip == null)
			return null;

		if (MathUtils.ApproximatelyZero(options.Speed))
			options.Speed = 1f;

		ActiveAnimation activeAnimation = new ActiveAnimation
		{
			Id = _nextId++,
			Clip = clip,
			Options = options,
			Time = options.Reverse ? clip.Duration + clip.Delay : 0f,
		};

		_activeAnimations.Add(activeAnimation);
		return new UIAnimHandle(this, activeAnimation.Id);
	}

	public bool IsPlaying(int id)
	{
		for (int i = 0; i < _activeAnimations.Count; i++)
		{
			if (_activeAnimations[i].Id == id)
				return true;
		}

		return false;
	}

	public void Stop(int id)
	{
		for (int i = _activeAnimations.Count - 1; i >= 0; i--)
		{
			if (_activeAnimations[i].Id != id)
				continue;

			_activeAnimations.RemoveAt(i);
		}
	}

	public void Pause(int id, bool isPaused)
	{
		for (int i = 0; i < _activeAnimations.Count; i++)
		{
			if (_activeAnimations[i].Id == id)
				_activeAnimations[i].IsPaused = isPaused;
		}
	}

	public void Reverse(int id)
	{
		for (int i = 0; i < _activeAnimations.Count; i++)
		{
			ActiveAnimation animation = _activeAnimations[i];
			if (animation.Id != id)
				continue;

			animation.Options.Reverse = !animation.Options.Reverse;
		}
	}

	private void ApplyAnimations(float dt)
	{
		Vector3 position = _target.GetPosition();
		Vector2 anchoredPosition = _target.GetAnchoredPosition();
		Vector2 size = _target.GetScale();
		float alpha = _target.GetAlpha();

		Vector3 additiveOffset = Vector3.zero;
		Vector2 additiveSize = Vector2.zero;

		bool movesAnchored = false;
		bool movesWorld = false;

		for (int i = _activeAnimations.Count - 1; i >= 0; i--)
		{
			ActiveAnimation animation = _activeAnimations[i];
			if (animation.IsPaused)
				continue;

		float speed = Mathf.Abs(animation.Options.Speed);
			float direction = animation.Options.Reverse ? -1f : 1f;
			animation.Time += dt * speed * direction;

			if (!TryComputeNormalizedTime(animation, out float normalizedTime, out bool completed))
				continue;

			EvaluateAnimation(_target, animation, normalizedTime,
									ref position,
									ref anchoredPosition,
									ref movesAnchored,
									ref movesWorld,
									ref size,
									ref alpha,
									ref additiveOffset,
									ref additiveSize);

			if (!completed)
				continue;

			_activeAnimations.RemoveAt(i);
			animation.Options.OnComplete?.Invoke();
		}

		position += additiveOffset;
		size += additiveSize;


		if (movesAnchored)
			_target.SetAnchoredPosition(anchoredPosition);
		if (movesWorld)
			_target.SetPosition(position);
		
		_target.SetScale(size);
		_target.SetAlpha(alpha);
	}

	private static bool TryComputeNormalizedTime(ActiveAnimation animation, out float normalizedTime, out bool completed)
	{
		completed = false;
		float duration = Mathf.Max(animation.Clip.Duration, 0.0001f);
		float delay = animation.Clip.Delay;
		float raw = (animation.Time - delay) / duration;

		switch (animation.Clip.LoopMode)
		{
			case UIAnimationLoopMode.Loop:
				normalizedTime = Mathf.Repeat(raw, 1f);
				return true;
			case UIAnimationLoopMode.PingPong:
				normalizedTime = Mathf.PingPong(raw, 1f);
				return true;
			case UIAnimationLoopMode.None:
			default:
				normalizedTime = Mathf.Clamp01(raw);
				if (animation.Options.Reverse)
					completed = raw <= 0f;
				else
					completed = raw >= 1f;
				return raw >= 0f || completed;
		}
	}

	private static void EvaluateAnimation(
						UIAnimationTarget target,
						ActiveAnimation animation,
						float t,
						ref Vector3 position,
						ref Vector2 anchoredPosition,
						ref bool movesAnchored,
						ref bool movesWorld,
						ref Vector2 size,
						ref float alpha,
						ref Vector3 additiveOffset,
						ref Vector2 additiveSize)
	{
		UIAnimationClip clip = animation.Clip;
		float clipEasedT = UIEase.Evaluate(clip.DefaultEase, t, clip.CustomCurve);

		PositionTrack[] positionTracks = clip.PositionTracks;
		for (int i = 0; i < positionTracks.Length; i++)
		{
			PositionTrack track = positionTracks[i];
			if (!track.Enabled)
				continue;

			movesAnchored |= track.Space == UIPositionSpace.Anchored;
			movesWorld |= track.Space == UIPositionSpace.World;

			float easedT = EvaluateTrackT(track.Ease, clip, t, clipEasedT);
			if (movesAnchored) 
				anchoredPosition = Vector2.LerpUnclamped(track.From, track.To, easedT);
			if (movesWorld)
				position = Vector3.LerpUnclamped(track.From, track.To, easedT);
		}

		ScaleTrack[] scaleTracks = clip.ScaleTracks;
		for (int i = 0; i < scaleTracks.Length; i++)
		{
			ScaleTrack track = scaleTracks[i];
			if (!track.Enabled)
				continue;

			float easedT = EvaluateTrackT(track.Ease, clip, t, clipEasedT);
			size = Vector3.LerpUnclamped(track.From, track.To, easedT);
		}

		AlphaTrack[] alphaTracks = clip.AlphaTracks;
		for (int i = 0; i < alphaTracks.Length; i++)
		{
			AlphaTrack track = alphaTracks[i];
			if (!track.Enabled)
				continue;

			float easedT = EvaluateTrackT(track.Ease, clip, t, clipEasedT);
			alpha = Mathf.LerpUnclamped(track.From, track.To, easedT);
		}
	}

	private static float EvaluateTrackT(UIEaseType trackEase, UIAnimationClip clip, float rawT, float clipEasedT)
	{
		if (trackEase == clip.DefaultEase)
			return clipEasedT;

		return UIEase.Evaluate(trackEase, rawT, clip.CustomCurve);
	}
}
