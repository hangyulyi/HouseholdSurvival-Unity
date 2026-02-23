using UnityEngine;
using UnityEngine.InputSystem;


public class ClickToMove : MonoBehaviour

{

    public float moveSpeed = 5f;


    private Vector3 targetPos;

    private bool moving = false;


    private void Update()

    {

        if (Mouse.current.leftButton.wasPressedThisFrame)

        {

            Vector3 mousePos = Mouse.current.position.ReadValue();

            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 worldPos = Camera.main.ScreenToViewportPoint(mousePos);

            targetPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);

            moving = true;

        }


        if (moving)

        {

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);


            if (Vector3.Distance(transform.position, targetPos) < 0.05f)

            {

                moving = false;

            }

        }

    }


}