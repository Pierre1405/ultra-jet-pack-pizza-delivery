using System.Collections;
using System.Collections.Generic;
using Assets.scripts.physics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Assets.scripts.physics.JumpMovement;
using static JetPackMovement;

public class PizzaMan : MonoBehaviour
{

    [SerializeField] public LayerMask plateformLayer;
    [SerializeField] public LayerMask zeroGLayer;
    public string zeroGLayerName;
    public string fuelLayerName;

    private Rigidbody2D rb;
    private Collider2D colider;
    public HorizontalMovement horizontalMovement;
    public JumpMovement jumpMovement;
    public JetPackMovement JetPackMovement;
    public float gravityScale;
    public float fallGravityScaleMultiplier;
    public float jetPackfallGravityScaleMultiplier;
    public float maximumFallSpeed;
    public GameObject character;
    private Animator characterAnimator;


    private InputActionPhase jumpButtonState = InputActionPhase.Canceled;
    private float xInput = 0;
    private bool isRefillColided;
    private bool isBouncing;
    private bool startBouncing;
    private bool isGroundedBool = false;
    private bool isZeroGBool = false;
    private Vector2 bouncingVelocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<BoxCollider2D>();
        characterAnimator = character.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        isGroundedBool = isGrounded();

        horizontalMovement.execute(xInput, rb, isGroundedBool);
        JumpStateTransition jumpState = jumpMovement.updateState(jumpButtonState, isGroundedBool, rb);
        JetPackStateTransition jetPackState = JetPackMovement.updateState(jumpButtonState, isGroundedBool, jumpState);

        #region FallgravityScale
        if (jetPackState == JetPackStateTransition.Performed)
        {
            rb.gravityScale = 0;
        } else if (jetPackState == JetPackStateTransition.Pause)
        {
            rb.gravityScale = gravityScale * jetPackfallGravityScaleMultiplier;
        }
        else
        if (rb.velocity.y < 0 && !isBouncing && !isZeroGBool)
        {
            rb.gravityScale = gravityScale * fallGravityScaleMultiplier;
        } else
        {
            rb.gravityScale = gravityScale;
        }
        if (rb.velocity.y < -maximumFallSpeed)
        {
            rb.AddForce((-maximumFallSpeed - rb.velocity.y) * Vector2.up, ForceMode2D.Impulse);
        }

        #region zeroG
        rb.gravityScale = Mathf.Abs(rb.gravityScale) * (isZeroGBool ? -1 : 1);
        #endregion zeroG

        #endregion FallgravityScale

        jumpMovement.execute(rb);
        JetPackMovement.execute(rb, isRefillColided);

        manageAnimatorState(isGroundedBool, jumpState, jetPackState);

        #region rotateCharacter
        if (Mathf.Abs(xInput) > 0.3f)
        {
            character.transform.localScale = new Vector3(Mathf.Sign(xInput), 1, 1);
        }
        #endregion rotateCharacter

        #region bouncing
        if(startBouncing)
        {
            rb.AddForce(Vector2.up * bouncingVelocity.y, ForceMode2D.Impulse);
            startBouncing = false;
        }
        if(isGroundedBool || isZeroGBool || jetPackState == JetPackStateTransition.Performed)
        {
            isBouncing = false;
            bouncingVelocity = Vector2.zero;
        }
        #endregion bouncing
    }

    private bool isGrounded()
    {
        float extraHeight = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(colider.bounds.center, colider.bounds.size, 0f, Vector2.down, extraHeight, plateformLayer);
        bool isGrounded = raycastHit.collider != null;
        return isGrounded;
    }

    private void manageAnimatorState(bool isGroundedBool, JumpStateTransition jumpState, JetPackStateTransition jetPackState)
    {
        bool isMoving = Mathf.Abs(xInput) >= 0.3f;
        if (isGroundedBool)
        {
            characterAnimator.SetBool("grounded", true);
            if (isMoving)
            {
                characterAnimator.SetBool("running", true);
            } else
            {
                characterAnimator.SetBool("running", false);
            }
            characterAnimator.SetBool("jumping", false);
            characterAnimator.SetBool("falling", false);
            characterAnimator.SetBool("jetpack stop", false);
            characterAnimator.SetBool("jetpack fast", false);
        } else
        {
            characterAnimator.SetBool("grounded", false);
            if (rb.velocity.y <= 0f)
            {
                characterAnimator.SetBool("falling", true);
            }
            if(jetPackState == JetPackStateTransition.Stoped || jetPackState == JetPackStateTransition.Pause)
            {
                characterAnimator.SetBool("jetpack stop", false);
                characterAnimator.SetBool("jetpack fast", false);
                if (jumpState == JumpStateTransition.Performed)
                {
                    characterAnimator.SetBool("jumping", true);
                }
            } else
            {
                characterAnimator.SetBool("jumping", false);
                if(isMoving)
                {
                    characterAnimator.SetBool("jetpack fast", true);
                    characterAnimator.SetBool("jetpack stop", false);
                } else
                {
                    characterAnimator.SetBool("jetpack fast", false);
                    characterAnimator.SetBool("jetpack stop", true);
                }
            }
        }
    }

    public void jump(InputAction.CallbackContext context)
    {
        jumpButtonState = context.phase;
    }

    public void move(InputAction.CallbackContext context)
    {
        xInput = context.ReadValue<Vector2>().x;
    }

    public void openMenu(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("menu");
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {        
        if (LayerMask.LayerToName(collision.gameObject.layer) == fuelLayerName)
        {
            isRefillColided = true;
        }

        if (LayerMask.LayerToName(collision.gameObject.layer) == zeroGLayerName)
        {
            isZeroGBool = true;
        }
    }


    public void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == fuelLayerName)
        {
            isRefillColided = false;
        }

        if (LayerMask.LayerToName(collision.gameObject.layer) == zeroGLayerName)
        {
            isZeroGBool = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "death")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else if (collision.collider.tag == "bounce")
        {
            isBouncing = true;
            startBouncing = true;
            if(bouncingVelocity == Vector2.zero)
            {
                bouncingVelocity = collision.relativeVelocity;
            }
            Debug.Log(bouncingVelocity);
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
    }
}