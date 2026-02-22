
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[Serializable]
public class ComboSystem
{
	[Range(5, 50)][SerializeField] private int MaxCombo;
	private int _currentCombo;

	[SerializeField] private TextMeshProUGUI _comboText;

	[SerializeField] private UIAnimator _meterAnimator;
	[SerializeField] private UIAnimationTarget _meterTarget;
	private UIAnimHandle _meterHandle;

	public int CurrentCombo => _currentCombo;
	public bool IsMaxCombo => CurrentCombo >= MaxCombo;

	public void IncreaseCombo()
	{
		_currentCombo = Mathf.Min(_currentCombo + 1, MaxCombo);
		if(_currentCombo >= 2)
		{
			if (!_comboText.gameObject.activeSelf)
				_comboText.gameObject.SetActive(true);
			
			_comboText.text = _currentCombo == MaxCombo ? "Max Combo" : $"X{_currentCombo} Combo";
		}
		AnimateGoblinMeter(0.4f);
	}

	public void ResetCombo()
	{
		_currentCombo = 0;
		if (_comboText.gameObject.activeSelf)
			_comboText.gameObject.SetActive(false);

		AnimateGoblinMeter(1.2f);
	}

	public void AnimateGoblinMeter(float duration)
	{
		if (_meterHandle != null && _meterHandle.IsPlaying)
			_meterHandle.Stop();

		float t = Mathf.Clamp01(_currentCombo / (float)MaxCombo);
		var targetScale = _meterTarget.GetScale();
		targetScale.y = t;
		var clip = UIAnimationClip.CreateRuntimeScaleClip(_meterTarget.GetScale(), targetScale, duration, UIEaseType.SineInOut);
		_meterHandle = _meterAnimator.Play(clip);
	}
}
