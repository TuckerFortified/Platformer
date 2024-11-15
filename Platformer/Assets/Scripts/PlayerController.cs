using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    public float Speed;
    public float MaxSpeed;
    public float AccelerationSpeed;
    public float DecelerationSpeed;
    Vector2 playerInput;
    public enum FacingDirection
    {
        left, right
    }

    

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {

        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        
        if (Input.GetKey(KeyCode.A))
        {
            playerInput.x = playerInput.x + (-AccelerationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerInput.x = playerInput.x + (AccelerationSpeed * Time.deltaTime);
        }
        else
        {
            //playerInput.x = playerInput.x * 0.99f;
            
        }
        playerInput.x = Mathf.Clamp(playerInput.x, -MaxSpeed, MaxSpeed);
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Debug.Log(playerInput);
        rigidbody.MovePosition(new Vector2(transform.position.x + (Time.deltaTime * Speed * playerInput.x), transform.position.y));
    }


    public bool IsWalking()
    {
        return false;
    }
    public bool IsGrounded()
    {
        return true;
    }

    public FacingDirection GetFacingDirection()
    {
        return FacingDirection.left;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        
    }
}
