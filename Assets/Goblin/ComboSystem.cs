
using System;
using TMPro;
using UnityEngine;

[Serializable]
public class ComboSystem
{
	[Range(5, 50)][SerializeField] private int MaxCombo;
	private int _currentCombo;

	[SerializeField] private TextMeshProUGUI _comboText;

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
	}

	public void ResetCombo()
	{
		_currentCombo = 0;
		if (_comboText.gameObject.activeSelf)
			_comboText.gameObject.SetActive(false);
	}
}
