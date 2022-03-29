using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerData data;
	[SerializeField] private ParticleSystem _dustTrail;
	[SerializeField] private ParticleSystem _jumpDust;
	[SerializeField] private Transform statsUI;

	private Coroutine _dashGhostRoutine = null;
	private Animator _playerAnimator;
	private SpriteRenderer _playerRenderer;
	private PlayerStat _playerStat;

	#region COMPONENTS
	private Rigidbody2D rb;
	#endregion

	#region STATE PARAMETERS
	public bool IsFacingRight;
	public bool _isJumping;
	public bool _isDashing;


	private float _lastOnGroundTime;
	private int _dashesLeft;
	private float _dashStartTime;
	private Vector2 _lastDashDir;
	private bool _dashAttacking;
	#endregion

	#region INPUT PARAMETERS
	public Vector2 _moveInput;
	private float _lastPressedJumpTime;
	private float _lastPressedDashTime;
	#endregion

	#region CHECK PARAMETERS
	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize;
	#endregion

	#region Layers & Tags
	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		_playerAnimator = this.GetComponent<Animator>();
		_playerRenderer = this.GetComponent<SpriteRenderer>();
		_playerStat = this.GetComponent<PlayerStat>();
	}

    private void Start()
    {
		SetGravityScale(data.gravityScale);
		IsFacingRight = true;
	}

    private void Update()
	{
		if (Time.timeScale < 0.01f)
			return;

		#region EFFECT CHECKS
		if (data.enableDustTrail && Mathf.Abs(rb.velocity.x) > 0.01f && _lastOnGroundTime > 0 && !_isJumping)
        {
			if(!_dustTrail.isPlaying) _dustTrail.Play();
        }
        #endregion

		#region INPUT HANDLER
		_moveInput.x = Input.GetAxisRaw("Horizontal");
		_moveInput.y = Input.GetAxisRaw("Vertical");

		if (Input.GetButtonDown("Jump"))
		{
			_lastPressedJumpTime = data.jumpBufferTime;
		}

		if (Input.GetButtonUp("Jump"))
		{
			if (CanJumpCut())
				JumpCut();
		}

		if (Input.GetButtonDown("Dash"))
		{
			_lastPressedDashTime = data.dashBufferTime;
		}
		#endregion

		#region TIMERS
		_lastOnGroundTime -= Time.deltaTime;
		_lastPressedJumpTime -= Time.deltaTime;
		_lastPressedDashTime -= Time.deltaTime;
		#endregion

		#region GENERAL CHECKS
		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);
		#endregion

		#region PHYSICS CHECKS
		if (!_isDashing && !_isJumping)
		{
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
				_lastOnGroundTime = data.coyoteTime;
		}
		#endregion

		#region ANIMATION
		_playerAnimator.SetFloat("velocity_x", Mathf.Abs(rb.velocity.x));
		_playerAnimator.SetFloat("velocity_y", rb.velocity.y);
		_playerAnimator.SetBool("onGround", (_lastOnGroundTime > 0));
		#endregion

		#region GRAVITY
		if (!_isDashing)
		{
			if (rb.velocity.y >= 0)
				SetGravityScale(data.gravityScale);
			else if (_moveInput.y < 0)
				SetGravityScale(data.gravityScale * data.quickFallGravityMult);
			else
				SetGravityScale(data.gravityScale * data.fallGravityMult);
		}
		#endregion

		#region JUMP CHECKS
		if (_isJumping && rb.velocity.y < 0)
			_isJumping = false;


		if (!_isDashing)
		{
			if (CanJump() && _lastPressedJumpTime > 0)
			{
				_isJumping = true;
				Jump();
			}
		}
		#endregion

		#region DASH CHECKS
		if (DashAttackOver())
		{
			if (_dashAttacking)
			{
				_dashAttacking = false;
				StopDash(_lastDashDir);
			}
			else if (Time.time - _dashStartTime > data.dashAttackTime + data.dashEndTime)
			{
				_isDashing = false;
			}
		}

		if (CanDash() && _lastPressedDashTime > 0)
		{
			if (_moveInput != Vector2.zero)
				_lastDashDir = _moveInput;
			else
				_lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

			_dashStartTime = Time.time;
			_dashesLeft--;
			_dashAttacking = true;

			_isDashing = true;
			_isJumping = false;

			StartDash(_lastDashDir);
		}
        #endregion
    }

    private void FixedUpdate()
	{
		#region DRAG
		if (_isDashing)
			Drag(DashAttackOver() ? data.dragAmount : data.dashAttackDragAmount);
		else if (_lastOnGroundTime <= 0)
			Drag(data.dragAmount);
		else
			Drag(data.frictionAmount);
		#endregion

		#region RUN
		if (!_isDashing)
		{
			Run(1);
		}
		else if (DashAttackOver())
		{
			Run(data.dashEndRunLerp);
		}
		#endregion
	}

	#region MOVEMENT METHODS
	public void SetGravityScale(float scale)
	{
		rb.gravityScale = scale;
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

	private void Run(float lerpAmount)
	{
		float targetSpeed = _moveInput.x * data.runMaxSpeed;
		float speedDif = targetSpeed - rb.velocity.x;

		#region Acceleration Rate
		float accelRate;

		if (_lastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel : data.runDeccel;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel * data.accelInAir : data.runDeccel * data.deccelInAir;

		if (((rb.velocity.x > targetSpeed && targetSpeed > 0.01f) || (rb.velocity.x < targetSpeed && targetSpeed < -0.01f)) && data.doKeepRunMomentum)
		{
			accelRate = 0;
		}
		#endregion

		#region Velocity Power
		float velPower;
		if (Mathf.Abs(targetSpeed) < 0.01f)
		{
			velPower = data.stopPower;
		}
		else if (Mathf.Abs(rb.velocity.x) > 0 && (Mathf.Sign(targetSpeed) != Mathf.Sign(rb.velocity.x)))
		{
			velPower = data.turnPower;
		}
		else
		{
			velPower = data.accelPower;
		}
		#endregion

		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
		movement = Mathf.Lerp(rb.velocity.x, movement, lerpAmount);

		rb.AddForce(movement * Vector2.right);

		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);
	}

	private void Turn()
	{
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
		scale = statsUI.localScale;
		scale.x *= -1;
		statsUI.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}

	private void Jump()
	{
		_lastPressedJumpTime = 0;
		_lastOnGroundTime = 0;

		#region Perform Jump
		float force = data.jumpForce;
		if (rb.velocity.y < 0)
			force -= rb.velocity.y;

		rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion

		_jumpDust.Play();
	}

	private void JumpCut()
	{
		rb.AddForce(Vector2.down * rb.velocity.y * (1 - data.jumpCutMultiplier), ForceMode2D.Impulse);
	}

	private void StartDash(Vector2 dir)
	{
		_playerStat.damageable = false;

		_lastOnGroundTime = 0;
		_lastPressedDashTime = 0;

		SetGravityScale(0);

		rb.velocity = dir.normalized * data.dashSpeed;

		GameMaster.Instance.CameraNoise.m_FrequencyGain = data.dashShakeFrequency;
		GameMaster.Instance.CameraNoise.m_AmplitudeGain = data.dashShakeAmount;
		_dashGhostRoutine = StartCoroutine(DashGhost());
	}

	private void StopDash(Vector2 dir)
	{
		_playerStat.damageable = true;

		SetGravityScale(data.gravityScale);

		if (dir.y > 0)
		{
			if (dir.x == 0)
				rb.AddForce(Vector2.down * rb.velocity.y * (1 - data.dashUpEndMult), ForceMode2D.Impulse);
			else
				rb.AddForce(Vector2.down * rb.velocity.y * (1 - data.dashUpEndMult) * .7f, ForceMode2D.Impulse);
		}

		GameMaster.Instance.CameraNoise.m_FrequencyGain = 0f;
		GameMaster.Instance.CameraNoise.m_AmplitudeGain = 0f;
		StopCoroutine(_dashGhostRoutine);
	}
	#endregion

	#region CHECK METHODS
	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	public bool CanJump()
	{
		return _lastOnGroundTime > 0 && !_isJumping;
	}

	private bool CanJumpCut()
	{
		return _isJumping && rb.velocity.y > 0;
	}

	private bool CanDash()
	{
		if (_dashesLeft < data.dashAmount && _lastOnGroundTime > 0)
			_dashesLeft = data.dashAmount;

		return _dashesLeft > 0;
	}

	private bool DashAttackOver()
	{
		return _isDashing && Time.time - _dashStartTime > data.dashAttackTime;
	}
    #endregion


    #region EFFECTS
	IEnumerator DashGhost()
    {
        while (true)
        {
			StartCoroutine(SpawnDashGhost());
			yield return new WaitForSeconds(data.dashGhostInterval);
        }
    }

	IEnumerator SpawnDashGhost()
    {
		GameObject ghost = Instantiate(data.ghostPrefab, this.transform.position, this.transform.rotation);
		ghost.transform.localScale = this.transform.localScale;
		SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
		ghostRenderer.sprite = _playerRenderer.sprite;
		float elapsed = 0f;
		while(elapsed < data.dashGhostFadeTime)
        {
			ghostRenderer.color = new Color(
				ghostRenderer.color.r,
				ghostRenderer.color.g,
				ghostRenderer.color.b,
				Mathf.Lerp(1, 0, elapsed / data.dashGhostFadeTime)
				);
			elapsed += Time.deltaTime;
			yield return null;
        }
		Destroy(ghost);
	}

    #endregion
}
