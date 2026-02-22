using UnityEngine;
using UnityEngine.UIElements;

public class LeafSpawner : MonoBehaviour
{
    [SerializeField] private Leaf[] _prefabs;
    [SerializeField] private GameObject _pivot0;
    [SerializeField] private GameObject _pivot1;
    [SerializeField, Min(0)] private float minInterval;
    [SerializeField, Min(0)] private float averageInterval;
    [SerializeField, Min(0)] private float maxInterval;

    private float _timer;
    private float _target;

    void Update()
    {
        if (_target == 0)
        {
            SetTarget();
            _timer = 0;
        }

        _timer += Time.deltaTime;

        if (_timer >= _target)
        {
            SpawnLeaf();
            _target = 0;
        }
    }

    private void SetTarget()
    {
        float avg = -averageInterval * Mathf.Log(Mathf.Max(float.Epsilon, Random.value));
        _target = Mathf.Max(minInterval, Mathf.Min(maxInterval,  avg));
    }

    private void SpawnLeaf()
    {
        float posX = Random.Range(_pivot0.transform.position.x, _pivot1.transform.position.x);
        float posY = Random.Range(_pivot0.transform.position.y, _pivot1.transform.position.y);

        var prefab = _prefabs[Random.Range(0, _prefabs.Length)];
        var leaf = Instantiate(prefab, this.transform);
        leaf.transform.position = new Vector3(posX, posY, 0);
    }
}
