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

    [Header("Jump and Dash")]
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check and Other Misc")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Vector3 input;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    private float currentSpeed;
    private Vector3 currentVelocity;

    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        input = new Vector3(inputVector.x, 0, inputVector.y);
    }
    void OnJump(InputValue value)
    {
        if(isGrounded && !isDashing)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    void OnDash(InputValue value)
    {
        if(dashCooldownTimer <= 0 && !isDashing && input != Vector3.zero)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            dashDirection = matrix.MultiplyPoint3x4(input).normalized;
        }
    }
    void FixedUpdate()
    {
        // Check Ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        Move();
    }
    void Update()
    {
        if (!isDashing)
        {
            Look();
        }
        Dash();
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
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
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

            if (input != Vector3.zero)
            {
                targetSpeed = isSprinting ? speed * sprintMultiplier : speed;
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
                
                var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
                var moveDirection = matrix.MultiplyPoint3x4(input).normalized;

                currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * currentSpeed, acceleration * Time.deltaTime);

                rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);

                if(currentSpeed > 0.01f)
                {
                    rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
                }
            }
        }
       
    }
}
