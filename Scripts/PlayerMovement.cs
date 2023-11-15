using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject point;
    [Header("Animator Controller")]
    public bool isRun;
    public bool isWalk;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpForceOnSlope;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody thRB;

    //移动状态
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    void Start()
    {
        thRB = GetComponent<Rigidbody>();
        thRB.freezeRotation = true;
        
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    void Update()
    {
        //检查是否在地面上
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.4f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //处理摩擦
        if (grounded)
        {
            thRB.drag = groundDrag;
        }
        else
        {
            thRB.drag = 0;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    /*
     * 用于处理输入内容并执行相应功能
     * MyInput()：处理按键输入并执行相应功能；
     * MovePlayer()：移动玩家，根据是否在地面或者是否在斜坡上进行判定；
     * SpeedControl()：限制移动速度，根据在斜坡上或者在地面上对最大速度进行限制。
     */
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //处理跳跃功能
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //处理下蹲功能
        bool isCanCrouch = CanCrouch();

        Debug.Log(isCanCrouch);

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            thRB.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        } 
        else if (Input.GetKeyUp(crouchKey) && isCanCrouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        } 
        else if (!Input.GetKey(crouchKey))
        {
            if(isCanCrouch)
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            else if(!isCanCrouch && transform.localScale.y == crouchYScale)
            {
                moveSpeed = crouchSpeed;
            }
        }


    }
    private void MovePlayer()
    {
        //计算运动方向
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //如果在斜坡上
        if (OnSlope() && !exitingSlope)
        {
            thRB.AddForce(GetSloopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

            if (thRB.velocity.y > 0)
            {
                thRB.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //判断是否在地面
        if (grounded)
        {
            thRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        //在空中
        else if (!grounded)
        {
            thRB.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //让玩家不会顺着斜坡滑落
        thRB.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        //控制在斜坡上的速度
        if (OnSlope() && !exitingSlope)
        {
            if (thRB.velocity.magnitude > moveSpeed)
                thRB.velocity = thRB.velocity.normalized * moveSpeed;
        }

        //限制在地面和空中的速度
        else
        {
            Vector3 flatVel = new Vector3(thRB.velocity.x, 0f, thRB.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                thRB.velocity = new Vector3(limitedVel.x, thRB.velocity.y, limitedVel.z);
            }
        }
    }

    public bool CanCrouch()
    {
        //获取任务头顶的高度
        Vector3 sphereLoctation = transform.position + Vector3.up * transform.localScale.y;
        
        point.transform.position = sphereLoctation;

        bool isCanCrouch = (Physics.OverlapSphere(sphereLoctation, startYScale / 2, whatIsGround)).Length == 0;

        if (isCanCrouch)
        {
            return true;
        }

        return false;
    }
    /*
     * 速度状态机，根据不同的状态来决定移动速度。
     */
    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    /*
     * 处理跳跃的函数
     * Jump()： 向上跳跃，给y值一个冲击力。
     * ResetJump()：重置跳跃状态，避免空中一直跳。
     */
    private void Jump()
    {
        exitingSlope = true;

        //重置y值
        thRB.velocity = new Vector3(thRB.velocity.x,0f, thRB.velocity.z);

        thRB.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    /*
     * 处理斜坡上的运动
     * OnSlope(): 判断是否在斜坡上；
     * GetSloopeMoveDirection()：获取在斜坡上的移动方向。
     */
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.4f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSloopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }


}
