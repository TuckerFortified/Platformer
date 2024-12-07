using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidbody;

    //Hprizontal Movement
    public float Speed;
    public float MaxSpeed;
    public float AccelerationTime;
    public float DecelerationTime;
    Vector2 playerInput;
    private float lastXPos;
    private float currentXPos;
    private bool leftOrRight;
    private bool onGroundIs;
    public enum FacingDirection
    {
        left, right
    }
    private bool walkingLeft;
    private bool walkingRight;
    
    //Vertical Movement
    private bool jumping;
    public float jumpVel;
    private float velocity;
    public float gravityScale;
    public float TerminalSpeed;
    public float coyoteTime;
    public float coyoteTimer;
    private float spacePressed;
    private bool jumpOnce;
    private bool jumpTwice;

    public int currentHealth;

    //Dashing
    private float dashWindow;
    private bool dashLeft;
    private bool dashRight;
    private bool dashing;
    private float dashDuration;
    private bool canDash;
    private float dashCooldown;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = new Vector2(0, 0);
        jumping = false;
        velocity = 0;
        walkingLeft = false;
        walkingRight = false;
        dashWindow = 0;
        dashing = false;
        canDash = true;
    }

    
    void Update()
    {

        //Changing animation states
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

        //Managing player inputs for walking and dashing (does not actually change the transform)
        if (Input.GetKey(KeyCode.A))
        {
            walkingLeft = true;
            if (dashWindow < 0.2f && !dashing && dashLeft && dashCooldown < 0 && canDash)
            {
                dashing = true;
                dashDuration = 0;
            }
        }
        else
        {
            walkingLeft = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            walkingRight = true;
            if (dashWindow < 0.2f && !dashing && dashRight && dashCooldown < 0 && canDash)
            {
                dashing = true;
                dashDuration = 0;
            }
        }
        else
        {
            walkingRight = false;
        }
        

        //Coyote Timer Incrementing
        coyoteTimer += Time.deltaTime;
        if (onGroundIs)
        {
            coyoteTimer = 0;
        }



        //Player dashing input - this is continued from the code earlier (it is technically the first part of the dashing code but it is lower down in the document)
        dashWindow += Time.deltaTime;

        if (Input.GetKeyUp(KeyCode.A))
        {
            dashWindow = 0;
            dashLeft = true;
            dashRight = false;
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            dashWindow = 0;
            dashRight = true;
            dashLeft = false;
        }

        //Jumping Input Management (Does not actually move the player)
        if ((IsGrounded() || (coyoteTimer < coyoteTime && coyoteTimer != 0)) && Input.GetKey(KeyCode.Space))
        {
            jumping = true;
            jumpOnce = true;
            velocity = jumpVel;
            spacePressed = 0;
        }
        else if (!IsGrounded() && jumpOnce && Input.GetKey(KeyCode.Space) && spacePressed > 0 && !jumpTwice)
        {
       
            jumping = true;
            jumpTwice = true;
            velocity = jumpVel;
        }
        else if (coyoteTimer > coyoteTime)
        {
            
            jumpOnce = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && jumpOnce)
        {
            
            spacePressed += 1;
        }
        
    }

    public void FixedUpdate()
    {

        //Changing the player facing direction.
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


        //Updating the player movement and position in the level based on the input from update.
        float acceleration = MaxSpeed / AccelerationTime;
        float decaleration = MaxSpeed / DecelerationTime;

        //Left and right acceleration.
        if (walkingLeft && !walkingRight)
        {
            playerInput.x -= acceleration * Time.deltaTime;
        }
        else if (walkingRight && !walkingLeft)
        {
            playerInput.x += acceleration * Time.deltaTime;
        }
        else
        {
            //Left and right decaleration (I can't spell).
            if (leftOrRight)
            {
                playerInput.x -= decaleration * Time.deltaTime;
                playerInput.x = Mathf.Clamp(playerInput.x, 0, MaxSpeed);
            }
            if (!leftOrRight)
            {
                playerInput.x += decaleration * Time.deltaTime;
                playerInput.x = Mathf.Clamp(playerInput.x, -MaxSpeed, 0);
            }
        }
        playerInput.x = Mathf.Clamp(playerInput.x, -MaxSpeed, MaxSpeed);
        

        //If the player isn't dashing: update gravity, jumping, and horizontal movement.
        if (!dashing)
        {
            velocity = Mathf.Clamp(velocity, -TerminalSpeed, 1000);
            rigidbody.MovePosition(new Vector2(transform.position.x + (Time.deltaTime * Speed * playerInput.x), transform.position.y + (velocity * Time.deltaTime)));
            velocity = velocity - (gravityScale * Time.deltaTime);
            dashCooldown -= 1;
        }

        //Player dashing movement update.
        if (dashing)
        {
            if (dashDuration > 5 && dashRight || dashDuration > 7 && dashLeft)
            { 
                dashing = false;
                dashCooldown = 15;
                canDash = false;
                dashLeft = false;
                dashRight = false;
            }
            float dashMovement = 0.5f;
            if (dashLeft == true)
            {
                dashMovement = -0.5f;
            }
            rigidbody.MovePosition(new Vector2(transform.position.x + dashMovement, transform.position.y));
            dashDuration += 1;
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

        //When player lands on trampoline
        if (collision.tag == "Trampoline")
        {
            velocity = 20;
        }
        else
        {
            //When Player lands on ground
            jumping = false;
            velocity = 0;
            if (!canDash)
            {
                canDash = true;
            }

            jumpOnce = false;
            jumpTwice = false;
            spacePressed = 0;
        }


        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onGroundIs = false;
        
    }

}
