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

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return; // don't update transform/rotation

        // 1) get coords of mouse, 2) in the world, 3) relative to the ship
        mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        relativeDirection = mousePosition - transform.position;

        // rotation of the Z-axis, get angle with arctan(o/a), scale by 2 (180 * 2)
        float relZ = Mathf.Atan(relativeDirection.x / Mathf.Abs(relativeDirection.y)) / Mathf.PI * 360;

        // clamp to 80 degrees and throw in a log function for fun
        relZ = Mathf.Clamp(relZ*Mathf.Log10(relativeDirection.magnitude/10), -80, 80);

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
