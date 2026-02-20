using System;
using UnityEngine;

public struct UIAnimOptions
{
	public float Speed;
	public bool Reverse;
	public Action OnComplete;

	public static UIAnimOptions Default => new UIAnimOptions
	{
		Speed = 1f,
	};
}

public sealed class UIAnimHandle
{
	private readonly UIAnimator _owner;
	private readonly int _id;

	internal UIAnimHandle(UIAnimator owner, int id)
	{
		_owner = owner;
		_id = id;
	}

	public bool IsValid => _owner != null;
	public bool IsPlaying => _owner != null && _owner.IsPlaying(_id);

	public void Stop() => _owner?.Stop(_id);
	public void Pause() => _owner?.Pause(_id, true);
	public void Resume() => _owner?.Pause(_id, false);
	public void Reverse() => _owner?.Reverse(_id);
}
