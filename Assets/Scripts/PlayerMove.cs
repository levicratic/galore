using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{

    public Rigidbody rb;
    public float movementSpeed;

    Vector3 mousePosition;
    Vector3 relativeDirection;
    Vector3 lookDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // get x, y coords of mouse
        mousePosition = Mouse.current.position.ReadValue();

        // convert to world point
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // direction relative to the ship object
        relativeDirection = mousePosition - transform.position;

        // rotation of the Z-axis, get angle with arctan(o/a)
        float relZ = Mathf.Atan(relativeDirection.x / Mathf.Abs(relativeDirection.y)) / Mathf.PI * 360;
        relZ = Mathf.Clamp(relZ*Mathf.Log10(relativeDirection.magnitude/10), -90, 90);

        transform.LookAt(mousePosition);
        transform.Rotate(0, 0, relZ);

    }

    void FixedUpdate()
    {
        rb.AddForce((relativeDirection + relativeDirection.normalized * 10f) * movementSpeed, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision");
        SceneManager.LoadScene(0);
        
    }
}
