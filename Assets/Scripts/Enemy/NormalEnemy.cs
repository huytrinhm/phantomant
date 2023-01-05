using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NormalEnemy : MonoBehaviour
{
    [SerializeField] private float activeHorizontalRadius = 10f;
    [SerializeField] private float activeVerticalRadius = 10f;    
    public float knockbackHorizontalForce = 3f;
    public float knockbackVerticalForce = 1.5f;
    [SerializeField] private float deadzoneRadius = 10f;
    [SerializeField] private Transform target;
    [SerializeField] private Transform statsUI;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float runAccel = 1f;
    public int strength = 10;
    public float attackLength;

    public Vector2 leftHome;
    public Vector2 rightHome;
    private Vector2 absLeftHome;
    private Vector2 absRightHome;
    private bool _isMovingToRightHome;

    public float hitPerSec = 2;
    private float attackCooldown;
	private bool _isFacingRight;
    private bool isAttacking;
    private PlayerStat targetStat;


    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize;
    [SerializeField] private LayerMask _groundLayer;
    public float dragAmount;
    public float frictionAmount;
    private bool onGround;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movingDirection;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        targetStat = target.GetComponent<PlayerStat>();
        animator = this.GetComponent<Animator>();

        absLeftHome = (Vector2)this.transform.position + leftHome;
        absRightHome = (Vector2)this.transform.position + rightHome;
    }

    private void Start()
    {
        movingDirection = Vector2.zero;
		_isFacingRight = false;
        isAttacking = false;
        _isMovingToRightHome = false;
    }

    private void Update()
    {
		if (Time.timeScale < 0.01f)
			return;

        if (isAttacking)
            return;

        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

		if (movingDirection.x != 0)
			CheckDirectionToFace(movingDirection.x > 0);

        onGround = Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer);

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
            if (this.transform.position.x <= absLeftHome.x)
            {
                movingDirection = Vector2.right;
                _isMovingToRightHome = true;
            }
            else if (this.transform.position.x >= absRightHome.x)
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

        if (onGround)
            Drag(frictionAmount);
        else
            Drag(dragAmount);

        if (movingDirection != Vector2.zero)
			Run();
    }

    private void Drag(float amount)
    {
        Vector2 force = amount * rb.velocity.normalized;
        force.x = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(force.x));
        force.y = Mathf.Min(Mathf.Abs(rb.velocity.y), Mathf.Abs(force.y));
        force.x *= Mathf.Sign(rb.velocity.x);
        force.y *= Mathf.Sign(rb.velocity.y);

        rb.AddForce(-force, ForceMode2D.Impulse);
    }

    private void MoveToTarget()
    {
        movingDirection = new Vector2(Mathf.Sign(target.position.x - this.transform.position.x), 0);
    }

    private void Attack()
    {
        isAttacking = true;
        attackCooldown = 1 / hitPerSec;
        if (this.transform.position.x >= target.position.x && _isFacingRight)
            Turn();
        else if (this.transform.position.x < target.position.x && !_isFacingRight)
            Turn();
        animator.SetTrigger("Attack");
        StartCoroutine(TurnOffAttack());
        //
        //    targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.left);
        //else
        //    targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.right);
    }

    IEnumerator TurnOffAttack()
    {
        yield return new WaitForSeconds(attackLength);
        isAttacking = false;
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

        Gizmos.DrawIcon((Vector2)this.transform.position + leftHome, "sv_icon_dot10_pix16_gizmo");
        Gizmos.DrawIcon((Vector2)this.transform.position + rightHome, "sv_icon_dot10_pix16_gizmo");
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

#if UNITY_EDITOR
[CustomEditor(typeof(NormalEnemy))]
public class NormalEnemyEditor : Editor
{
    private void OnSceneGUI()
    {
        NormalEnemy currentEnemy = target as NormalEnemy;
        
        EditorGUI.BeginChangeCheck();
        Vector3 newLeftHome = Handles.PositionHandle(currentEnemy.leftHome + (Vector2)currentEnemy.transform.position, Quaternion.identity);
        Vector3 newRightHome = Handles.PositionHandle(currentEnemy.rightHome + (Vector2)currentEnemy.transform.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(currentEnemy, "Change left/right home of LostSoul");
            currentEnemy.leftHome = newLeftHome - currentEnemy.transform.position;
            currentEnemy.rightHome = newRightHome - currentEnemy.transform.position;
        }
    }
}
#endif
