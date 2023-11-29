using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public float runningMaxSpeed = 7f;
    public float jumpForce = 8f;
    public float mouseSpeedX = 6.0f;
    public float mouseSpeedY = 4.0f;

    public Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.4f);
    public float maxDistance = 0.95f;
    public LayerMask layerMask;

    private Camera camera;
    private Rigidbody rigidbody;
    private GameObject body;
    private Animator bodyAnimator;

    private bool isJumping = false;
    private bool canWalk = true;
    private bool isFalling = false;

    //Animations hash
    int isRunningHash;
    int isRunningFasterHash;
    int isJumpingHash;
    int isWalkingLeftHash;
    int isWalkingRightHash;
    int isWalkingBackHash;
    int fallingStateHash;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;

        camera = GetComponentInChildren<Camera>();

        body = gameObject.transform.Find("Body").gameObject;
        bodyAnimator = GetComponentInChildren<Animator>();

        isRunningHash = Animator.StringToHash("isRunning");
        isRunningFasterHash = Animator.StringToHash("isRunningFaster");
        isJumpingHash = Animator.StringToHash("isJumping");
        isWalkingLeftHash = Animator.StringToHash("isWalkingLeft");
        isWalkingRightHash = Animator.StringToHash("isWalkingRight");
        isWalkingBackHash = Animator.StringToHash("isWalkingBack");
        fallingStateHash = Animator.StringToHash("fallingState");

        isFalling = true;
        bodyAnimator.SetInteger(fallingStateHash, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (GroundCheck() && isFalling)
        {
            StartCoroutine(FallHitGround());
        }

        MovementHandler();
        LockMouse();
    }

    private void FixedUpdate()
    {
        MouseHandler();
    }

    private void MouseHandler()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeedX;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSpeedY;

        transform.RotateAround(body.transform.position, body.transform.up, mouseX * mouseSpeedX);

        float newCameraX = camera.transform.eulerAngles.x + mouseY;
        float restOfNewX = newCameraX > 360 ? (newCameraX - 360) : (newCameraX < 0 ? newCameraX * -1 : 0);

        newCameraX = newCameraX > 360 ? restOfNewX : (newCameraX < 0 ? 360 - restOfNewX : newCameraX);
        if ((newCameraX <= 70 && newCameraX >= 0) || newCameraX <= 360 && newCameraX >= 330)
        {
            camera.transform.RotateAround(body.transform.position, camera.transform.right, mouseY);
        }
    }

    private void MovementHandler()
    {
        BasicWalk();
        JumpingHandler();
    }

    private void BasicWalk()
    {
        if (!canWalk)
        {
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift) && GroundCheck())
            {
                bodyAnimator.SetBool(isRunningFasterHash, true);

                if (rigidbody.velocity.magnitude <= runningMaxSpeed)
                {
                    rigidbody.velocity += camera.transform.forward * (speed + 10f) * Time.deltaTime;
                }
                return;
            }

            bodyAnimator.SetBool(isRunningFasterHash, false);
            bodyAnimator.SetBool(isRunningHash, true);
            transform.position += camera.transform.forward * speed * Time.deltaTime;
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            bodyAnimator.SetBool(isWalkingLeftHash, true);
            transform.position += -transform.right * (speed - 2f) * Time.deltaTime;
            return;
        }

        if (Input.GetKey(KeyCode.S))
        {
            bodyAnimator.SetBool(isWalkingBackHash, true);
            transform.position += -camera.transform.forward * (speed - 3f) * Time.deltaTime;
            return;
        }

        if (Input.GetKey(KeyCode.D))
        {
            bodyAnimator.SetBool(isWalkingRightHash, true);
            transform.position += transform.right * (speed - 2f) * Time.deltaTime;
            return;
        }

        ResetMovementAnimations();
    }

    private void JumpingHandler()
    {
        if (!GroundCheck())
        {
            isJumping = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && GroundCheck())
        {
            StartCoroutine(Jump());
            return;
        }

        if (GroundCheck() && isJumping)
        {
            isJumping = false;
            StartCoroutine(FallJump());
        }
    }

    IEnumerator Jump()
    {
        bodyAnimator.SetBool(isJumpingHash, true);

        yield return new WaitForSeconds(0.3f);

        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator FallJump()
    {
        canWalk = false;
        bodyAnimator.SetBool(isJumpingHash, false);
        yield return new WaitForSeconds(1);
        canWalk = true;
    }

    private void LockMouse()
    {
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }

    /**
     * To Works you need to create a Layer called Ground
     * add to the terrain you want to check the collision
     * And add the layer on layerMask in this script
     */
    private bool GroundCheck()
    {
        return Physics.BoxCast(
            transform.position,
            boxSize, -transform.up,
            transform.rotation,
            maxDistance,
            layerMask
            );
    }

    private void ResetMovementAnimations()
    {
        bodyAnimator.SetBool(isRunningHash, false);
        bodyAnimator.SetBool(isRunningFasterHash, false);
        bodyAnimator.SetBool(isWalkingLeftHash, false);
        bodyAnimator.SetBool(isWalkingRightHash, false);
        bodyAnimator.SetBool(isWalkingBackHash, false);
    }

    IEnumerator FallHitGround()
    {
        bodyAnimator.SetInteger(fallingStateHash, 2);
        yield return new WaitForSeconds(1f);
        bodyAnimator.SetInteger(fallingStateHash, 3);
        isFalling = false;
        yield return new WaitForSeconds(8f);
        bodyAnimator.SetInteger(fallingStateHash, 0);
    }
}
