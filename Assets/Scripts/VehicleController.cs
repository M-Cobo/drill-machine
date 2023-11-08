﻿using UnityEngine;
using Rewired;

public class VehicleController : MonoBehaviour {
	
	// Main vehicle component
	
	public bool controllable = true;
	public int playerId = 0;

	[Header("Components")]
	
	public Transform vehicleModel;
	public Rigidbody sphere;
	
	[Header("Controls")]
	
	public KeyCode jump;
	
	[Header("Parameters")]
	
	[Range(5.0f, 40.0f)] public float acceleration = 30f;
	[Range(20.0f, 160.0f)] public float steering = 80f;
	[Range(0.0f, 179.0f)] public float maxTurnRight = 45f;
	[Range(0.0f, -179.0f)] public float maxTurnLeft = -45f;
	[Range(0.0f, 1.0f)] public float airControl = 0.1f;
	[Range(50.0f, 80.0f)] public float jumpForce = 60f;
	[Range(0.0f, 20.0f)] public float gravity = 10f;
	[Range(0.0f, 1.0f)] public float drift = 1f;
	[Range(0.0f, 2.0f)] public float velocityMultiplier = 1.5f;
	
	[Header("Switches")]
	
	public bool jumpAbility = false;
	public bool steerInAir = true;
	public bool motorcycleTilt = false;
	public bool alwaysSmoke = false;
	
	// Vehicle components
	
	Transform container, wheelFrontLeft, wheelFrontRight;
	Transform body;
	TrailRenderer trailLeft, trailRight;
	
	ParticleSystem smoke;
	VehicleEnhancers vehicleEnhancers;
	
	// Private

	int layer_mask;
	
	float speed, speedTarget;
	float rotate, rotateTarget;
	
	bool nearGround, onGround;
	
	Vector3 containerBase;
	[HideInInspector] public Player player;
	
	// Functions
	
	void Awake(){
		
		foreach(Transform t in GetComponentsInChildren<Transform>()){
			
			switch(t.name){
				
				// Vehicle components
				
				case "wheelFrontLeft": wheelFrontLeft = t; break;
				case "wheelFrontRight": wheelFrontRight = t; break;
				case "body": body = t; break;
				
				// Vehicle effects
				
				case "smoke": smoke = t.GetComponent<ParticleSystem>(); break;
				case "trailLeft": trailLeft = t.GetComponent<TrailRenderer>(); break;
				case "trailRight": trailRight = t.GetComponent<TrailRenderer>(); break;
				
			}
			
		}
		
		container = vehicleModel.GetChild(0);
		containerBase = container.localPosition;

		player = ReInput.players.GetPlayer(playerId);
		
		layer_mask = LayerMask.GetMask("Road");

		vehicleEnhancers = this.GetComponent<VehicleEnhancers>();

	}
	
	void Update(){
		
		// Acceleration
		
		speedTarget = Mathf.SmoothStep(speedTarget, speed, Time.deltaTime * 12f); speed = 0f;
		
		if(player.GetButton("Accelerate")){ ControlAccelerate(); }
		if(player.GetButton("Brake")){ ControlBrake(); }
		
		// Steering
		
		rotateTarget = Mathf.Lerp(rotateTarget, rotate, Time.deltaTime * 4f); rotate = 0f;
		
		ControlSteer(player.GetAxis("Steering"));
		
		transform.rotation = Quaternion.Slerp(
			transform.rotation,
			Quaternion.Euler(new Vector3(0, (transform.eulerAngles.y + rotateTarget).ClampAngle(maxTurnLeft, maxTurnRight), 0)), 
			Time.deltaTime * 2.0f
		);

		// Jump
		
		if(Input.GetKeyDown(jump)){ ControlJump(); }
		
		// Wheel and body tilt
		
		if(wheelFrontLeft != null){  wheelFrontLeft.localRotation  = Quaternion.Euler(0, rotateTarget / 2, 0); }
		if(wheelFrontRight != null){ wheelFrontRight.localRotation = Quaternion.Euler(0, rotateTarget / 2, 0); }
		
		body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.Euler(new Vector3(speedTarget / 4, 0, rotateTarget / 6)), Time.deltaTime * 4.0f);
		
		// Vehicle tilt
		
		float tilt = 0.0f; if(motorcycleTilt){ tilt = -rotateTarget / 1.5f; }
		
		container.localPosition = containerBase + new Vector3(0, Mathf.Abs(tilt) / 2000, 0);
		container.localRotation = Quaternion.Slerp(container.localRotation, Quaternion.Euler(0, rotateTarget / 8, tilt), Time.deltaTime * 10.0f);
		
		// Effects
		
		if(!motorcycleTilt){ smoke.transform.localPosition = new Vector3(-rotateTarget / 100, smoke.transform.localPosition.y, smoke.transform.localPosition.z); }
		
		ParticleSystem.EmissionModule smokeEmission = smoke.emission;
		smokeEmission.enabled = onGround && sphere.velocity.magnitude > (acceleration / 4) && (Vector3.Angle(sphere.velocity, vehicleModel.forward) > 30.0f || alwaysSmoke);
		
		if(trailLeft != null){   trailLeft.emitting = smoke.emission.enabled; }
		if(trailRight != null){ trailRight.emitting = smoke.emission.enabled; }
		
		// Stops vehicle from floating around when standing still
		
		if(speed == 0 && sphere.velocity.magnitude < 4f){ sphere.velocity = Vector3.Lerp(sphere.velocity, Vector3.zero, Time.deltaTime * 2.0f); }
		
	}

	// Physics update
	
	void FixedUpdate(){
		
		RaycastHit hitOn;
		RaycastHit hitNear;
		
		onGround   = Physics.Raycast(transform.position, Vector3.down, out hitOn, 1.1f * transform.localScale.x, layer_mask);
		nearGround = Physics.Raycast(transform.position, Vector3.down, out hitNear, 2.0f * transform.localScale.x, layer_mask);
		
		// Normal

		vehicleModel.up = Vector3.Lerp(vehicleModel.up, hitNear.normal, Time.deltaTime * 10.0f);
		vehicleModel.Rotate(0, transform.eulerAngles.y, 0);

		// Movement
		
		if(nearGround){
			
			sphere.AddForce(vehicleModel.forward * speedTarget, ForceMode.Acceleration);
			
		}else{
			
			sphere.AddForce(vehicleModel.forward * (speedTarget * airControl), ForceMode.Acceleration);
			
			// Simulated gravity
			
			sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
			
		}
		
		transform.localPosition = sphere.transform.localPosition;
		
		// Simulated drag on ground thanks to Adam Hunt
		
		Vector3 localVelocity = transform.InverseTransformVector(sphere.velocity);
		localVelocity.x *= 0.9f + (drift / 10);
		
		if(nearGround){
			
			sphere.velocity = transform.TransformVector(localVelocity);
			
		}
		
	}
	
	// Controls
	
	public void ControlAccelerate(){
		
		if(!controllable){ return; }
		
		speed = !vehicleEnhancers.onFever ? acceleration : acceleration * velocityMultiplier;
		
	}
	
	public void ControlBrake(){
		
		if(!controllable){ return; }
		
		speed = -acceleration;
		
	}
	
	public void ControlJump(){
		
		if(!controllable){ return; }
		
		if(jumpAbility && nearGround){
			
			sphere.AddForce(Vector3.up * (jumpForce * 20), ForceMode.Impulse);
			
		}
		
	}
	
	public void ControlSteer(float direction){
		
		if(!controllable){ return; }
		
		if(nearGround || steerInAir){ rotate = steering * direction; }
		
	}
	
	// Functions
	
	public void SetPosition(Vector3 position, Quaternion rotation){
		
		// Stop vehicle
		
		speed = rotate = 0.0f;
		
		sphere.velocity = Vector3.zero;
		sphere.position = position;
		
		// Set new position
		
		transform.position = position;
		transform.rotation = rotation;
		
	}
	
}