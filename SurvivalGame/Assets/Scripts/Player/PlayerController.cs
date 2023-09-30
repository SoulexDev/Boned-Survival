using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;

    [SerializeField] private float moveSpeed = 5;
    private float sensitivity => PlayerPrefs.GetFloat("Sensitivity", 2.5f);
    [SerializeField] private float _jumpHeight = 5;
    private float jumpHeight => _jumpHeight;
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float airMoveAmount = 40;
    [SerializeField] private float gravity = 15;

    [SerializeField] private Transform camPos;
    private Rigidbody rb;

    private Transform cam;
    private float x, z;
    public float camX, camY;
    private float camRot;

    Vector3 lastDir;
    Vector3 moveDir;
    Vector3 controllerMoveDir;
    Vector3 yVel;
    Vector3 groundNormal;

    private bool grounded => Grounded();
    private bool groundedPrevFrame;
    public bool isMovingAndGrounded => grounded && moving;

    private bool moving;
    private bool jumping;
    private bool running;

    private bool ceilChecked;

    public enum MoveState { GroundMove, AirMove, Grappling, Dashing }
    public MoveState moveState;

    void Awake()
    {
        cam = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        //rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (moveSpeed > maxSpeed)
            maxSpeed = moveSpeed;
    }

    void Update()
    {
        SetMoveState();
        PlayerInput();
        Movement();
    }
    private void LateUpdate()
    {
        CamMovement();
    }
    public void ResetForces()
    {
        lastDir = Vector3.zero;
        controllerMoveDir = Vector3.zero;
        yVel = Vector3.zero;
    }
    void SetMoveState()
    {
        moveState = grounded ? MoveState.GroundMove : MoveState.AirMove;
    }
    void PlayerInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        camX += Input.GetAxis("Mouse X") * sensitivity * Time.timeScale;
        camY += Input.GetAxis("Mouse Y") * sensitivity * Time.timeScale;
        camY = Mathf.Clamp(camY, -90, 90);

        running = Input.GetKey(KeyCode.LeftShift);

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            jumping = true;
            yVel.y = jumpHeight;

            lastDir += controller.velocity;

            Invoke(nameof(ResetJump), 0.4f);
        }
    }
    void Movement()
    {
        SlopeCheck();

        moveDir = (Vector3.Cross(transform.right, groundNormal) * z - Vector3.Cross(transform.forward, groundNormal) * x).normalized;

        moveDir *= moveSpeed;

        Debug.DrawRay(transform.position - Vector3.up, moveDir, Color.red, 0.1f);

        moving = new Vector3(moveDir.x, 0, moveDir.z).magnitude > 0;

        switch (moveState)
        {
            case MoveState.GroundMove:
                MoveGround();
                break;
            case MoveState.AirMove:
                MoveAir();
                break;
            default:
                break;
        }

        if (!controller.enabled)
            return;
        controllerMoveDir += yVel;
        controller.Move(controllerMoveDir * Time.deltaTime);
    }
    void MoveGround()
    {
        if (!jumping)
            yVel.y = 0;

        if (!moving && !jumping)
        {
            lastDir = Vector3.Lerp(lastDir, Vector3.zero, Time.deltaTime * 10);
            controller.Move(lastDir * Time.deltaTime);
        }
        if (moving)
            lastDir = new Vector3(moveDir.x, 0, moveDir.z);

        controllerMoveDir = moveDir;
    }
    void MoveAir()
    {
        if (!controller.enabled)
            return;

        moveDir.y = 0;
        if (moving)
        {
            lastDir += moveDir * Time.deltaTime * airMoveAmount;

            if (lastDir.magnitude > maxSpeed)
                lastDir *= maxSpeed / lastDir.magnitude;
        }
        else if (!jumping)
        {
            lastDir.x = Mathf.Abs(controller.velocity.x) < Mathf.Abs(lastDir.x) ? controller.velocity.x : lastDir.x;
            lastDir.z = Mathf.Abs(controller.velocity.z) < Mathf.Abs(lastDir.z) ? controller.velocity.z : lastDir.z;
        }
        CheckCeiling();

        yVel.y -= gravity * Time.deltaTime;
        controllerMoveDir = lastDir;
    }
    void CamMovement()
    {
        transform.rotation = Quaternion.Euler(0, camX, 0);

        camRot = Mathf.Lerp(camRot, x * -2.5f, Time.deltaTime * 5);

        camPos.rotation = Quaternion.Euler(-camY, camX, 0);

        camPos.localRotation *= Quaternion.Euler(0, 0, camRot);

        cam.SetPositionAndRotation(camPos.position, camPos.rotation);
    }
    void ResetJump()
    {
        jumping = false;
    }
    public void AddForce(Vector3 force)
    {
        jumping = true;

        controllerMoveDir += new Vector3(force.x, 0, force.z);
        yVel.y += force.y;

        Invoke(nameof(ResetJump), 0.2f);
    }
    public bool Grounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, 0.35f, Vector3.down, out hit, 0.85f, ~LayerMask.GetMask("Player", "Ignore Player", "Ignore Raycast"));
    }
    void CheckCeiling()
    {
        if (controller.collisionFlags == CollisionFlags.Above)
        {
            if (!ceilChecked)
                yVel.y = -0.1f;
            ceilChecked = true;
        }
        else
            ceilChecked = false;
    }
    public void SlopeCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, ~LayerMask.GetMask("Player", "Ignore Player", "Ignore Raycast")))
        {
            groundNormal = hit.normal;
        }
        else
            groundNormal = Vector3.up;
    }
}