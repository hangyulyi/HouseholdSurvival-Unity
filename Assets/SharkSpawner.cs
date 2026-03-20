using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    public GameObject shark;
    public float spawnRate = 2;
    public float heightOffset = 10;

    private float _timer = 0;

    // OnEnable fires every time MinigameRoot becomes active — resets timer and spawns immediately
    void OnEnable()
    {
        _timer = 0;
        SpawnShark();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnRate)
        {
            SpawnShark();
            _timer = 0;
        }
    }

    void SpawnShark()
    {
        float lo = transform.position.y - heightOffset;
        float hi = transform.position.y + heightOffset;
        Instantiate(shark, new Vector3(transform.position.x, Random.Range(lo, hi), 0), transform.rotation);
    }
}
