using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 5;
    [SerializeField] private float turnSpeed = 360;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Vector3 input;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
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
        Look();
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
            var relative = (transform.position + skewedInput) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }


    }
    void Move()
    {
        if(input != Vector3.zero)
        {
            rb.MovePosition(transform.position + transform.forward * speed * Time.deltaTime);
        }
       
    }
}
