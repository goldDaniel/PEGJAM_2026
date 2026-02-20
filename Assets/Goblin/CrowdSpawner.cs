using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CrowdSpawner : MonoBehaviour
{
	[SerializeField] private CrowdGoblin[] _prefabs;
	[SerializeField] private GameObject _pivot0;
	[SerializeField] private GameObject _pivot1;
	
	void Awake()
	{
		int count = 20;
		for (int i = 0; i < count; ++i)
		{
			float t = (float)i / (float)(count - 1);

			float posX = Mathf.Lerp(_pivot0.transform.position.x, _pivot1.transform.position.x, t);
			float posY = Mathf.Lerp(_pivot0.transform.position.y, _pivot1.transform.position.y, t);

			var prefab = _prefabs[Random.Range(0, _prefabs.Length)];
			var goblin = Instantiate(prefab, this.transform);
			goblin.transform.position = new Vector3(posX, posY, 0);
		}
	}
}
