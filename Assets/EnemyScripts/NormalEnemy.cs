using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : MonoBehaviour
{
    [SerializeField] private float activeHorizontalRadius = 10f;
    [SerializeField] private float activeVerticalRadius = 10f;    
    [SerializeField] private float knockbackHorizontalForce = 3f;
    [SerializeField] private float knockbackVerticalForce = 1.5f;
    [SerializeField] private float deadzoneRadius = 10f;
    [SerializeField] private Transform target;
    [SerializeField] private Transform statsUI;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float runAccel = 1f;
    [SerializeField] private int strength = 10;

    [SerializeField] private Vector2 leftHome;
    [SerializeField] private Vector2 rightHome;
    private bool _isMovingToRightHome;

    public float hitPerSec = 2;
    private float attackCooldown;
	private bool _isFacingRight;
    private bool isAttacking;
    private PlayerStat targetStat;

	private Rigidbody2D rb;

    private Vector2 movingDirection;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        targetStat = target.GetComponent<PlayerStat>();
    }

    private void Start()
    {
        movingDirection = Vector2.zero;
		_isFacingRight = true;
        isAttacking = false;
        _isMovingToRightHome = true;
    }

    private void Update()
    {
		if (Time.timeScale < 0.01f)
			return;

        if (isAttacking)
            return;

		if (movingDirection.x != 0)
			CheckDirectionToFace(movingDirection.x > 0);

		if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (TargetInActiveRange())
        {
            if (!TargetInDeadzone())
                MoveToTarget();
            else
            {
                movingDirection = Vector2.zero;
                if (attackCooldown <= 0)
                    Attack();
            }
        } 
        else
        {
            if (this.transform.position.x <= leftHome.x)
            {
                movingDirection = Vector2.right;
                _isMovingToRightHome = true;
            }
            else if (this.transform.position.x >= rightHome.x)
            {
                movingDirection = Vector2.left;
                _isMovingToRightHome = false;
            }
            else if (_isMovingToRightHome)
            {
                movingDirection = Vector2.right;
            }
            else
            {
                movingDirection = Vector2.left;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale < 0.01f)
            return;

		if (movingDirection != Vector2.zero)
			Run();
    }

    private void MoveToTarget()
    {
        movingDirection = new Vector2(Mathf.Sign(target.position.x - this.transform.position.x), 0);
    }

    private void Attack()
    {
        attackCooldown = 1 / hitPerSec;
        if (this.transform.position.x - target.position.x >= 0)
            targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.left);
        else
            targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.right);
    }

    private bool TargetInActiveRange()
    {
        return
            (Mathf.Abs(target.position.x - this.transform.position.x) <= activeHorizontalRadius) &&
            (Mathf.Abs(target.position.y - this.transform.position.y) <= activeVerticalRadius);
    }

    private bool TargetInDeadzone()
    {
        return (Mathf.Abs(target.position.x - this.transform.position.x) <= deadzoneRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector2(activeHorizontalRadius * 2, activeVerticalRadius * 2));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(deadzoneRadius * 2, activeVerticalRadius * 2));
        Gizmos.DrawIcon(leftHome, "sv_icon_dot10_pix16_gizmo");
        Gizmos.DrawIcon(rightHome, "sv_icon_dot10_pix16_gizmo");
    }
	
	public void SetGravityScale(float scale)
	{
		rb.gravityScale = scale;
	}

	private void Run()
	{
		float targetSpeed = movingDirection.x * maxSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		
		float movement = Mathf.Abs(speedDif) * runAccel * Mathf.Sign(speedDif);

		rb.AddForce(movement * Vector2.right);

		if (movingDirection.x != 0)
			CheckDirectionToFace(movingDirection.x > 0);
	}

	private void Turn()
	{
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
        scale = statsUI.localScale;
        scale.x *= -1;
        statsUI.localScale = scale;

        _isFacingRight = !_isFacingRight;
	}

	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != _isFacingRight)
			Turn();
	}
}
