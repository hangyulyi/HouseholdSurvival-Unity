using UnityEngine;
using UnityEngine.InputSystem;

public class SubScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public float jumpStrength;
    public LogicManagerScript logic;
    public bool subIsAlive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
