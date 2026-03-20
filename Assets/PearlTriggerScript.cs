using UnityEngine;

public class PearlTriggerScript : MonoBehaviour
{
    public float moveSpeed = 5;
    public float deadZone = -45;

    // Set by PearlSpawnerScript when instantiating — no tag lookup needed
    [HideInInspector] public LogicManagerScript logic;

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < deadZone)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            if (logic != null) logic.addScore();
            Destroy(gameObject);
        }
    }
}
