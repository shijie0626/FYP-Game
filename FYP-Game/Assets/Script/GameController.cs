using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public float MoveSpeed = 5f; // 正常移动速度
    public float RunSpeed = 10f;  // 奔跑速度
    public float JumpSpeed = 10f; // 跳跃力量
    public Rigidbody2D myrigidbody;
    public Transform groundCheck;  // 地面检查的位置
    public LayerMask groundLayer;  // 定义什么是地面
    public float groundCheckRadius;
    public LayerMask whatIsGround;


    bool isGround;
    public Animator myAnimator;

    public Vector3 RespawnPoint;

    public AudioSource JumpSound;
    public AudioSource DeadSound;



    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isClimbing;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // 检查玩家是否在地面上
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        

        // 处理移动和跳跃
        Move();

        // 处理跳跃
        if (Input.GetButtonDown("Jump") && isGrounded) 
        {
            Jump();
        }

        
    }

    void Move()
    {
        float moveDirection = Input.GetAxis("Horizontal");
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : MoveSpeed;
        myAnimator.SetBool("walk", moveDirection != 0);
        // 移动玩家
        rb.velocity = new Vector3(moveDirection * currentSpeed, rb.velocity.y);
        if (moveDirection > 0.01f) 
        { 
            transform.localScale = Vector3.one; 
        }
           
        else if (moveDirection < -0.01f)
        { 
            transform.localScale = new Vector3(-1, 1, 1); 
        }
        
        
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0f, JumpSpeed), ForceMode2D.Impulse);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "DeadZone")
        {
            gameObject.transform.position = RespawnPoint;
            FindObjectOfType<Health>().currentHealth -= 1;

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Ladder")
        {
            myrigidbody.gravityScale = 0;
            myAnimator.SetBool("walk", false);
        }
        if (Input.GetAxisRaw("Vertical") > 0 && other.tag == "Ladder")
        {
            myrigidbody.velocity = new Vector3(0, MoveSpeed, 0);
            myAnimator.SetBool("isClimb", true);
            myAnimator.SetBool("down", false);
        
        }

        else if (Input.GetAxisRaw("Vertical") < 0 && other.tag == "Ladder")
        {
            myrigidbody.velocity = new Vector3(0, -MoveSpeed, 0);
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", true);
 
        }
        else if (other.tag == "Ladder")
        {
            myrigidbody.velocity = new Vector3(0, 0, 0);
        }
        if (Input.GetAxisRaw("Vertical") == 0 && other.tag == "Ladder")
        {
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", false);
        }


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Ladder")
        {
            myrigidbody.gravityScale = 1f;
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", false);
        }
    }
}
