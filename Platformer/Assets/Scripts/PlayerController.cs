using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    public float Speed;
    public float MaxSpeed;
    public float AccelerationSpeed;
    public float DecelerationSpeed;
    Vector2 playerInput;
    private float lastXPos;
    private float currentXPos;
    private bool leftOrRight;
    private bool onGroundIs;
    public enum FacingDirection
    {
        left, right
    }

    private bool jumping;
    public float jumpVel;
    private float velocity;
    public float gravityScale;
    public float TerminalSpeed;
    public float coyoteTime;
    public float coyoteTimer;

    private bool walkingLeft;
    private bool walkingRight;

    public int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = new Vector2(0, 0);
        jumping = false;
        velocity = 0;
        walkingLeft = false;
        walkingRight = false;
    }

    // Update is called once per frame
    void Update()
    {

        previousState = currentState;
        
        if (IsDead())
        {
            currentState = CharacterState.die;
        }

        switch(currentState)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentState = CharacterState.walk;
                }
                if (!IsGrounded())
                {
                    currentState = CharacterState.jump;
                }
                break;

            case CharacterState.walk:
                if (!IsWalking())
                {
                    currentState = CharacterState.idle;
                }
                if (!IsGrounded())
                {
                    currentState = CharacterState.jump;
                }
                break;
            
            case CharacterState.jump:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        currentState = CharacterState.walk;
                    }
                    else
                    {
                        currentState = CharacterState.idle;
                    }
                    
                }
                break;

            case CharacterState.die:


                break;
        }

        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        if (Input.GetKey(KeyCode.A))
        {
            walkingLeft = true;
        }
        else
        {
            walkingLeft = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            walkingRight = true;
        }
        else
        {
            walkingRight = false;
        }
        
        

        //For coyote time
        coyoteTimer += Time.deltaTime;

        if (jumping)
        {
            coyoteTimer = 0;
        }

        if (onGroundIs)
        {
            coyoteTimer = 0;
        }

        Debug.Log(coyoteTimer);
    }

    public void FixedUpdate()
    {
        lastXPos = currentXPos;
        currentXPos = transform.position.x;

        if ((currentXPos - lastXPos) > 0)
        {
            //right
            leftOrRight = true;
        }
        else if ((currentXPos - lastXPos) < 0)
        {
            //left
            leftOrRight = false;
        }

        //Movement
        if (walkingLeft && !walkingRight)
        {
            playerInput.x = playerInput.x + (-AccelerationSpeed * Time.deltaTime);
        }
        else if (walkingRight && !walkingLeft)
        {
            playerInput.x = playerInput.x + (AccelerationSpeed * Time.deltaTime);
        }
        else
        {
            playerInput.x = 0;
        }
        playerInput.x = Mathf.Clamp(playerInput.x, -MaxSpeed, MaxSpeed);
        MovementUpdate(playerInput);

        velocity = Mathf.Clamp(velocity, -TerminalSpeed, 1000);
        rigidbody.MovePosition(new Vector2(transform.position.x + (Time.deltaTime * Speed * playerInput.x), transform.position.y + (velocity * Time.deltaTime)));
        velocity = velocity - (gravityScale * Time.deltaTime);

    }

    private void MovementUpdate(Vector2 playerInput)
    {

        if ((IsGrounded() || (coyoteTimer < coyoteTime && coyoteTimer != 0)) && Input.GetKey(KeyCode.Space))
        {
            jumping = true;
            velocity = jumpVel;
        }
        else if (IsGrounded())
        {
            jumping = false;
            velocity = 0;
        }

    }


    public bool IsWalking()
    {
        if (transform.position.x == lastXPos)
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    public bool IsGrounded()
    {
        if (onGroundIs)
        {

            return true;
        }
        else
        {

            return false;
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }

    public CharacterState currentState = CharacterState.idle;
    public CharacterState previousState = CharacterState.idle;



    public FacingDirection GetFacingDirection()
    {
        
        if (leftOrRight == true)
        {
            return FacingDirection.right;
        }
        else
        {
            return FacingDirection.left;
        }
        
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        onGroundIs = true;
        Debug.Log(onGroundIs);
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onGroundIs = false;
        Debug.Log(onGroundIs);
    }

}
