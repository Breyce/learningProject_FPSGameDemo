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

    //�ƶ�״̬
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
        //����Ƿ��ڵ�����
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.4f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //����Ħ��
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
     * ���ڴ����������ݲ�ִ����Ӧ����
     * MyInput()�����������벢ִ����Ӧ���ܣ�
     * MovePlayer()���ƶ���ң������Ƿ��ڵ�������Ƿ���б���Ͻ����ж���
     * SpeedControl()�������ƶ��ٶȣ�������б���ϻ����ڵ����϶�����ٶȽ������ơ�
     */
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //������Ծ����
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //�����¶׹���
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
        //�����˶�����
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //�����б����
        if (OnSlope() && !exitingSlope)
        {
            thRB.AddForce(GetSloopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

            if (thRB.velocity.y > 0)
            {
                thRB.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //�ж��Ƿ��ڵ���
        if (grounded)
        {
            thRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        //�ڿ���
        else if (!grounded)
        {
            thRB.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //����Ҳ���˳��б�»���
        thRB.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        //������б���ϵ��ٶ�
        if (OnSlope() && !exitingSlope)
        {
            if (thRB.velocity.magnitude > moveSpeed)
                thRB.velocity = thRB.velocity.normalized * moveSpeed;
        }

        //�����ڵ���Ϳ��е��ٶ�
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
        //��ȡ����ͷ���ĸ߶�
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
     * �ٶ�״̬�������ݲ�ͬ��״̬�������ƶ��ٶȡ�
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
     * ������Ծ�ĺ���
     * Jump()�� ������Ծ����yֵһ���������
     * ResetJump()��������Ծ״̬���������һֱ����
     */
    private void Jump()
    {
        exitingSlope = true;

        //����yֵ
        thRB.velocity = new Vector3(thRB.velocity.x,0f, thRB.velocity.z);

        thRB.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    /*
     * ����б���ϵ��˶�
     * OnSlope(): �ж��Ƿ���б���ϣ�
     * GetSloopeMoveDirection()����ȡ��б���ϵ��ƶ�����
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
