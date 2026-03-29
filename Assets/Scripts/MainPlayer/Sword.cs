using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Sword : MonoBehaviour
{
    private const string AttackTrigger = "Attack";

    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    [SerializeField] private Transform weaponCollider;
    [SerializeField] private int slashPoolPrewarm = 8;

    private MainPlayerInput playerControls;
    private Animator animatorRef;
    private MainPlayerController playerController;
    private ActiveWeapon activeWeapon;
    private Camera mainCamera;

    private bool isAttacking = false;

    private GameObject slashAnim;
    private readonly Queue<GameObject> slashPool = new Queue<GameObject>();

    private void Awake()
    {
        playerController = GetComponentInParent<MainPlayerController>();
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        animatorRef = GetComponent<Animator>();
        playerControls = new MainPlayerInput();
        mainCamera = Camera.main;

        if (slashAnimPrefab == null)
        {
            return;
        }

        for (int i = 0; i < slashPoolPrewarm; i++)
        {
            GameObject pooled = CreateSlashAnimInstance();
            pooled.SetActive(false);
            slashPool.Enqueue(pooled);
        }
    }

    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.Combat.Attack.started += OnAttackStarted;
    }

    private void OnDisable()
    {
        playerControls.Combat.Attack.started -= OnAttackStarted;
        playerControls.Disable();
    }

    private void OnDestroy()
    {
        playerControls.Dispose();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        MouseFollowWithOffset();
    }

    private void Attack()
    {
        if (isAttacking)
        {
            return;
        }

        if (slashAnimPrefab == null || slashAnimSpawnPoint == null)
        {
            return;
        }

        slashAnim = RentSlashAnim();
        if (slashAnim == null)
        {
            return;
        }

        isAttacking = true;
        animatorRef.SetTrigger(AttackTrigger);
        weaponCollider.gameObject.SetActive(true);
        slashAnim.transform.SetPositionAndRotation(slashAnimSpawnPoint.position, Quaternion.identity);
        slashAnim.SetActive(true);
    }

    private void OnAttackStarted(InputAction.CallbackContext _)
    {
        Attack();
    }

    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
        isAttacking = false;
    }


    public void SwingUpFlipAnimEvent()
    {
        if (slashAnim == null)
        {
            return;
        }

        slashAnim.transform.rotation = Quaternion.Euler(-180, 0, 0);
        SpriteRenderer sprite = slashAnim.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.flipX = false;
        }

        if (playerController.FacingLeft && sprite != null)
        {
            sprite.flipX = true;
        }
    }

    public void SwingDownFlipAnimEvent()
    {
        if (slashAnim == null)
        {
            return;
        }

        slashAnim.transform.rotation = Quaternion.Euler(0, 0, 0);
        SpriteRenderer sprite = slashAnim.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.flipX = false;
        }

        if (playerController.FacingLeft && sprite != null)
        {
            sprite.flipX = true;
        }
    }

    private GameObject CreateSlashAnimInstance()
    {
        GameObject instance = Instantiate(slashAnimPrefab, Vector3.zero, Quaternion.identity);
        instance.transform.SetParent(transform.parent);

        SlashAnim slashAnimComp = instance.GetComponent<SlashAnim>();
        if (slashAnimComp != null)
        {
            slashAnimComp.SetReturnToPoolAction(ReturnSlashAnimToPool);
        }

        return instance;
    }

    private GameObject RentSlashAnim()
    {
        if (slashAnimPrefab == null)
        {
            return null;
        }

        while (slashPool.Count > 0)
        {
            GameObject pooled = slashPool.Dequeue();
            if (pooled != null)
            {
                return pooled;
            }
        }

        return CreateSlashAnimInstance();
    }

    private void ReturnSlashAnimToPool(GameObject slashObject)
    {
        if (slashObject == null)
        {
            return;
        }

        slashObject.SetActive(false);
        slashPool.Enqueue(slashObject);
    }

    private void MouseFollowWithOffset()
    {
        if (mainCamera == null || playerController == null || activeWeapon == null)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = mainCamera.WorldToScreenPoint(playerController.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            activeWeapon.transform.rotation = Quaternion.Euler(0, -180, angle);
            weaponCollider.transform.rotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            activeWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
            weaponCollider.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
