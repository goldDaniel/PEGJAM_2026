
using UnityEngine;
using UnityEngine.UI;

public class Food : MonoBehaviour
{
	[Range(1, 100)]
	public int bitesNeeded;
	private int _currrentHealth;

	[SerializeField] private Mask mask;

	void Awake()
	{
		_currrentHealth = bitesNeeded;
	}

	public bool TakeBite()
	{
		_currrentHealth = Mathf.Max(_currrentHealth - 1, 0);
		return _currentHealth == 0;
	}
}