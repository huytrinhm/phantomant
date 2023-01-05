using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigEnemy : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform statsUI;


    [SerializeField] private float activeHorizontalRadius = 10f;
    [SerializeField] private float activeVerticalRadius = 10f;
    [SerializeField] private float deadzoneRadius = .1f;
    
    [SerializeField] private float knockbackHorizontalForce = 3f;
    [SerializeField] private float knockbackVerticalForce = 1.5f;
    [SerializeField] private int strength = 10;

    [Range(0f, 1f)]  private float dashUpEndMult;
    public float dashAttackTime = 0.7f;
    public float dashGhostInterval = 1;
    public float dashGhostFadeTime;
    public GameObject ghostPrefab;
    
    public float dashPerSec = 2;
    public float hitPerSec = 2;

    private bool _isDashing;
    private float _dashStartTime;

    private float dashCooldown;
    private float attackCooldown;
    private bool attackedPlayer = true;
    private bool _isFacingRight;
    private bool isAttacking;

    private Coroutine _dashGhostRoutine = null;
    private SpriteRenderer _bigEnemyRenderer;
    private PlayerStat targetStat;
    private Rigidbody2D rb;
    private Vector2 movingDirection;
    private Vector2 currentPosition;
    private Animator animator;

    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize;
    [SerializeField] private LayerMask _groundLayer;
    public float dashAttackDragAmount;
    public float dragAmount;
    public float frictionAmount;
    private bool onGround;

    public float dashSpeed;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        targetStat = target.GetComponent<PlayerStat>();
        _bigEnemyRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        movingDirection = Vector2.zero;
        _isFacingRight = false;
        isAttacking = false;
    }

    private void Update()
    {
        if (Time.timeScale < 0.01f)
            return;

        if (isAttacking)
            return;

        if (movingDirection.x != 0)
            CheckDirectionToFace(movingDirection.x > 0);

        onGround = Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer);

        if (!_isDashing)
        {
            if (target.position.x < this.transform.position.x && _isFacingRight)
                Turn();
            else if (target.position.x > this.transform.position.x && !_isFacingRight)
                Turn();
        }

        if (dashCooldown > 0)
            dashCooldown -= Time.deltaTime;

        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (DashAttackOver())
        {
            _isDashing = false;
            animator.SetBool("Attack", false);
            StopDash(movingDirection);
        }

        if (TargetInActiveRange())
        {
            if (!TargetInDeadzone())
                MoveToTarget();
            else
            {
                //if(_isDashing)
                //{
                //    _isDashing = false;
                //    animator.SetBool("Attack", false);
                //    StopDash(movingDirection);
                //}
                //movingDirection = Vector2.zero;
                if ((!_isDashing && attackCooldown <= 0) || (_isDashing && !attackedPlayer))
                    Attack();
            }
        } 
        else
        {
            movingDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale < 0.01f)
            return;

        if (_isDashing)
            Drag(DashAttackOver() ? dragAmount : dashAttackDragAmount);
        else if (onGround)
            Drag(frictionAmount);
        else
            Drag(dragAmount);

        if (!_isDashing && movingDirection != Vector2.zero && dashCooldown <= 0) 
        {
            attackedPlayer = false;
            dashCooldown = 1 / dashPerSec;
            _dashStartTime = Time.time;
            _isDashing = true;
            animator.SetBool("Attack", true);
            Dash();
        }
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

    //RANGE DETECT PLAYER
    private bool TargetInActiveRange()
    {
        return
            (Mathf.Abs(target.position.x - this.transform.position.x) <= activeHorizontalRadius) &&
            (Mathf.Abs(target.position.y - this.transform.position.y) <= activeVerticalRadius);
    }

    private bool TargetInDeadzone()
    {
        return (Mathf.Abs(target.position.x - this.transform.position.x) <= deadzoneRadius) &&
                (Mathf.Abs(this.transform.position.x) > Mathf.Abs(target.position.x));
    }

    //MOVE TO PLAYER
    private void MoveToTarget()
    {
        movingDirection = new Vector2(Mathf.Sign(target.position.x - this.transform.position.x), 0);
    }

    //DASH TO PLAYER
    private void Dash()
    {
        StartCoroutine(DashShake());
        StartDash(movingDirection);
    }

    IEnumerator DashShake()
    {
        GameMaster.Instance.CameraNoise.m_FrequencyGain = 0.02f;
        GameMaster.Instance.CameraNoise.m_AmplitudeGain = 2.5f;
        yield return new WaitForSeconds(0.2f);

        GameMaster.Instance.CameraNoise.m_FrequencyGain = 0f;
        GameMaster.Instance.CameraNoise.m_AmplitudeGain = 0f;
    }
    
    private void StartDash(Vector2 dir)
    {
        rb.velocity = dir.normalized * dashSpeed;

        /*float step = dashSpeed * Time.deltaTime;
        currentPosition = new Vector2 (transform.position.x,target.position.y);
        transform.position = Vector2.MoveTowards(currentPosition, target.position,step);*/
        AudioManager.Instance.PlaySoundEffect("brute_dash");
        _dashGhostRoutine = StartCoroutine(DashGhost());

    }

    private void StopDash(Vector2 dir)
    {
        if (dir.y > 0)
        {
            if (dir.x == 0)
                rb.AddForce(Vector2.down * rb.velocity.y * (1 - dashUpEndMult), ForceMode2D.Impulse);
            else
                rb.AddForce(Vector2.down * rb.velocity.y * (1 - dashUpEndMult) * .7f, ForceMode2D.Impulse);
        }
        StopCoroutine(_dashGhostRoutine);
    }

    //DASH ANIMATION
    IEnumerator DashGhost()
    {
        while (true)
        {
            StartCoroutine(SpawnDashGhost());
            yield return new WaitForSeconds(dashGhostInterval);
        }
    }

    IEnumerator SpawnDashGhost()
    {
        GameObject ghost = Instantiate(ghostPrefab, this.transform.position, this.transform.rotation);
        Destroy(ghost, dashGhostFadeTime);
        ghost.transform.localScale = this.transform.localScale;
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        ghostRenderer.sprite = _bigEnemyRenderer.sprite;
        float elapsed = 0.1f;
        while(elapsed < dashGhostFadeTime && ghostRenderer != null)
        {
            ghostRenderer.color = new Color(
                ghostRenderer.color.r,
                ghostRenderer.color.g,
                ghostRenderer.color.b,
                Mathf.Lerp(1, 0, elapsed / dashGhostFadeTime)
                );
            elapsed += Time.deltaTime;
            yield return null;
        }
        
    }

    //ATTACK
    private void Attack()
    {
        attackCooldown = 1 / hitPerSec;
        attackedPlayer = true;
        if(!_isFacingRight)
            targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.left);
        else
            targetStat.Damage(knockbackHorizontalForce, knockbackVerticalForce, strength, Vector2.right);

        if(!_isDashing)
            AudioManager.Instance.PlaySoundEffect("brute_attack");
    }


    //FACE TO PLAYER
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != _isFacingRight)
            Turn();
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

    //ADDITIONAL FUNCTION
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector2(activeHorizontalRadius * 2, activeVerticalRadius * 2));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(deadzoneRadius * 2, activeVerticalRadius * 2));
    }

    private bool DashAttackOver()
    {
        return _isDashing && Time.time - _dashStartTime > dashAttackTime;
    }
}
