using UnityEngine;

public class MainPlayerController : MonoBehaviour
{
    private const string MoveXParam = "moveX";
    private const string MoveYParam = "moveY";
    private const float MoveThresholdSqr = 0.0001f;

    public bool FacingLeft
    {
        get => facingLeft;
        private set => facingLeft = value;
    }

    [SerializeField] private float moveSpeed = 1f;
    public float CurrentMoveSpeed => moveSpeed;

    private MainPlayerInput playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animatorRef;
    private SpriteRenderer spriteRendererRef;
    private Camera mainCamera;

    private bool facingLeft = false;

    private void Awake()
    {
        playerControls = new MainPlayerInput();
        rb = GetComponent<Rigidbody2D>();
        animatorRef = GetComponent<Animator>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetPlayerMoveSfxActive(false);
        }
    }

    private void OnDestroy()
    {
        playerControls.Dispose();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
        UpdateMoveSfxState();
    }

    private void ReadInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        animatorRef.SetFloat(MoveXParam, movement.x);
        animatorRef.SetFloat(MoveYParam, movement.y);
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = mainCamera.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            spriteRendererRef.flipX = true;
            FacingLeft = true;
        }
        else
        {
            spriteRendererRef.flipX = false;
            FacingLeft = false;
        }
    }

    public void AddMoveSpeedMultiplier(float multiplier)
    {
        moveSpeed *= multiplier;
    }

    private void UpdateMoveSfxState()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        bool isMoving = movement.sqrMagnitude > MoveThresholdSqr;
        AudioManager.Instance.SetPlayerMoveSfxActive(isMoving);
    }

}
