using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*.................. VARIABLES ......................*/
    public float speed;
    public float jumpForce;

    private bool isOnGround;
    private Rigidbody2D playerRig;

    private Vector3 originalPos;
    /*.................. DEFAULT METHODS ......................*/

    void Start()
    {
        originalPos = transform.position;
        playerRig = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //does not change rotation
        transform.rotation = new Quaternion(0, 0, 0, 0);

        //move left and right
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector2.right * Time.deltaTime * horizontalInput * speed);

        //jump
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            isOnGround = false;
            playerRig.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
        // respawn player
        if (transform.position.y < -20) transform.position = originalPos;
    }
    
    /*.................. SUPPORT METHODS ......................*/

    // check if player is on the ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }
}
