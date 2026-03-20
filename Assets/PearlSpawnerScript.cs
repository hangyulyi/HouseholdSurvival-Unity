using UnityEngine;

public class PearlSpawnerScript : MonoBehaviour
{
    public GameObject pearl;
    public float spawnRate = 5;
    public float heightOffset = 10;
    public LogicManagerScript logic;  // assign in Inspector

    private float _timer = 0;

    void OnEnable()
    {
        _timer = 0;
        SpawnPearl();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnRate)
        {
            SpawnPearl();
            _timer = 0;
        }
    }

    void SpawnPearl()
    {
        float lo = transform.position.y - heightOffset;
        float hi = transform.position.y + heightOffset;
        var obj = Instantiate(pearl,
            new Vector3(transform.position.x, Random.Range(lo, hi), 0),
            transform.rotation);

        // Pass logic reference directly — no tag lookup on the pearl side
        var trigger = obj.GetComponent<PearlTriggerScript>();
        if (trigger != null) trigger.logic = logic;
    }
}
