using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
	//PHYSICS
	[Header("Gravity")]
	public float gravityScale;
	public float fallGravityMult;
	public float quickFallGravityMult;

	[Header("Drag")]
	public float dragAmount;
	public float frictionAmount;

	[Header("Other Physics")]
	[Range(0, 0.5f)] public float coyoteTime;


	//GROUND
	[Header("Run")]
	public float runMaxSpeed;
	public float runAccel;
	public float runDeccel;
	[Range(0, 1)] public float accelInAir;
	[Range(0, 1)] public float deccelInAir;
	[Space(5)]
	[Range(.5f, 2f)] public float accelPower;
	[Range(.5f, 2f)] public float stopPower;
	[Range(.5f, 2f)] public float turnPower;


	//JUMP
	[Header("Jump")]
	public float jumpForce;
	[Range(0, 1)] public float jumpCutMultiplier;
	[Space(10)]
	[Range(0, 0.5f)] public float jumpBufferTime;


	//ABILITIES
	[Header("Dash")]
	public int dashAmount;
	public float dashSpeed;
	[Space(5)]
	public float dashAttackTime;
	public float dashAttackDragAmount;
	[Space(5)]
	public float dashEndTime;
	[Range(0f, 1f)] public float dashUpEndMult;
	[Range(0f, 1f)] public float dashEndRunLerp;
	[Space(5)]
	[Range(0, 0.5f)] public float dashBufferTime;
	
	[Header("Effects")]
	public float dashShakeAmount;
	public float dashShakeFrequency;
	public float dashGhostInterval;
	public float dashGhostFadeTime;
	public GameObject ghostPrefab;
	public bool enableDustTrail;
	public ParticleSystem dustTrail;


	//OTHER
	[Header("Other Settings")]
	public bool doKeepRunMomentum;
}