using UnityEngine;
using UnityEngine.InputSystem;

public class SubScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public float jumpStrength;
    public LogicManagerScript logic;
    public bool subIsAlive = true;

    private Vector3 _startPosition;

    void Awake()
    {
        _startPosition = transform.position;
    }

    // OnEnable fires every time the MinigameRoot is shown — resets sub state
    void OnEnable()
    {
        subIsAlive = true;
        transform.position = _startPosition;
        if (myRigidbody != null)
            myRigidbody.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && subIsAlive)
        {
            myRigidbody.linearVelocity = Vector2.up * jumpStrength;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        logic.gameOver();
        subIsAlive = false;
    }
}
