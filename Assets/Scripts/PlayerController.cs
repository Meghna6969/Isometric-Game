using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 5;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float turnSpeed = 360;
    [SerializeField] private float airControlMultiplier = 0.5f;

    [Header("Noise")]
    [SerializeField] private float walkNoiseRadius;
    [SerializeField] private float walkNoiseIntensity;
    [SerializeField] private float noiseDuration = 2f;
    [SerializeField] private float runNoiseInterval = 2f;
    private float lastWalkNoiseTime;

    [Header("Jump and Dash")]
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float offsetAngle;

    [Header("Ground Check and Other Misc")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip runningSound;


    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Vector3 input;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    private float currentSpeed;
    private Vector3 currentVelocity;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private bool isJumping;
    private bool jumpInputReleased = true;

    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        input = new Vector3(inputVector.x, 0, inputVector.y);
    }
    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpInputReleased = false;
        }
        else
        {
            jumpInputReleased = true;
            if(rb.linearVelocity.y > 0 && isJumping)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier, rb.linearVelocity.z); 
            }
        }
    }
    void OnDash(InputValue value)
    {
        if(dashCooldownTimer <= 0 && !isDashing && input != Vector3.zero)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, offsetAngle, 0));
            dashDirection = matrix.MultiplyPoint3x4(input).normalized;
        }
    }
    void FixedUpdate()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        if(isGrounded)
        {
            isJumping = false;
        }
        HandleJumpBetter();
        ApplyMyGravity();
        Move();
    }
    void Update()
    {
        if (!isDashing)
        {
            Look();
        }
        Dash();
        UpdateJumpingTimers();
        UpdateAnimations();
        HandleSounds();
    }
    void UpdateAnimations()
    {
        if(animator == null) return;
        bool isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isMoving = input != Vector3.zero;

        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isDashing", false);

        if (isJumping || !isGrounded)
        {
            animator.SetBool("isJumping", true);
        }
        else if (isDashing)
        {
            animator.SetBool("isDashing", true);
        }
        else if(isMoving && isSprinting)
        {
            if(Time.time - lastWalkNoiseTime >= runNoiseInterval)
            {
                NoiseManager.Instance.MakeNoise(transform.position, walkNoiseIntensity, walkNoiseRadius, noiseDuration);
            }
            animator.SetBool("isRunning", true);
        } else if (isMoving)
        {
            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isIdle", true);
        }
    }
    void UpdateJumpingTimers()
    {
        if(jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    void HandleJumpBetter()
    {
        bool canJump = (isGrounded || coyoteTimeCounter > 0) && !isDashing && !isJumping;
        if(jumpBufferCounter > 0 && canJump)
        {
            isJumping = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    void ApplyMyGravity()
    {
        if(rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        } else if(rb.linearVelocity.y > 0 && jumpInputReleased)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    void Dash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if(dashTimer <= 0)
            {
                isDashing = false;
            }
        }
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    void Look()
    {
        if (input != Vector3.zero)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, offsetAngle, 0));
            var skewedInput = matrix.MultiplyPoint3x4(input);

           var targetRotation = Quaternion.LookRotation(skewedInput, Vector3.up);
           transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }


    }
    void Move()
    {
        if (isDashing)
        {
            rb.MovePosition(transform.position + dashDirection * dashSpeed * Time.deltaTime);
            currentSpeed = dashSpeed;
        }else
        {
            bool isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            float targetSpeed;

            float currentAcceleration = isGrounded ? acceleration : acceleration * airControlMultiplier;
            float currentDecelearation = isGrounded ? deceleration : deceleration * airControlMultiplier;

            if (input != Vector3.zero)
            {
                targetSpeed = isSprinting ? speed * sprintMultiplier : speed;
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, currentAcceleration * Time.deltaTime);
                
                var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, offsetAngle, 0));
                var moveDirection = matrix.MultiplyPoint3x4(input).normalized;

                currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * currentSpeed, currentAcceleration * Time.deltaTime);

                rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, currentDecelearation * Time.deltaTime);
                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, currentDecelearation * Time.deltaTime);

                if(currentSpeed > 0.01f)
                {
                    rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
                }
            }
        }
       
    }
    private void HandleSounds()
    {
        if(audioSource == null || runningSound == null) Debug.LogError("PlayerController: No audio source");
         bool isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isMoving = input != Vector3.zero;
        bool shouldPlaySound = isGrounded && isMoving && isSprinting && !isDashing;

        if (shouldPlaySound)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = runningSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
