
using UnityEngine;
using UnityEngine.UI;

public class Food : MonoBehaviour
{
	[Range(1, 100)]
	public int bitesNeeded;
	private int _currentHealth;

	[SerializeField] private Mask mask;

	void Awake()
	{
		_currentHealth = bitesNeeded;
	}

	public bool TakeBite()
	{
		_currentHealth = Mathf.Max(_currentHealth - 1, 0);
		return _currentHealth == 0;
	}

	public float GetPercentage()
	{
		return (float)_currentHealth / (float)bitesNeeded;
	}
}