using Godot;
using System;
using System.Diagnostics;
using System.Security.Cryptography;

public partial class Player : CharacterBody3D
{
	public const float Speed = 8.0f;

	public const float slideSpeedup = 5f;
	
	public const float JumpVelocity = 10f;

	public const float sensivivity = 0.003f;

	public const float bobFreq = 2.0f;
	public const float bobAmp = 0.08f;
	public float bobLoc = 0.0f;

	public const float baseFOV = 75.0f;

	public const float FOVChange = 1.5f;

	public Vector3 slideDir = Vector3.Zero;

	public bool isSliding = false;

	public bool superjump = false;

	public int airJumpCount = 1;

	public int currentAirJumps;

	[Export] 
	public Node3D head;

	[Export]
	public AnimationPlayer anim;

	public Camera3D cam;

	public MultiplayerSynchronizer ms;

	private Vector3 syncPos = new Vector3(0,0,0);


    public override void _Ready()
    {	
		ms = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		ms.SetMultiplayerAuthority(int.Parse(Name));
		if (ms.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) {
			return;
		}
		cam = head.GetChild<Camera3D>(0);
        Input.MouseMode = Input.MouseModeEnum.Captured;
		currentAirJumps = airJumpCount;
		cam.MakeCurrent();
    }

    public override void _UnhandledInput(InputEvent @event)
    {	
		if (ms.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) {
			return;
		}

        if (@event.GetType() == typeof(InputEventMouseMotion)) {
			InputEventMouseMotion mMovement = (InputEventMouseMotion)@event;
			head.RotateY(- mMovement.Relative.X * sensivivity);
			cam.RotateX(- mMovement.Relative.Y * sensivivity);
            cam.Rotation = new Vector3(Mathf.Clamp(cam.Rotation.X, Mathf.DegToRad(-60), Mathf.DegToRad(60)),cam.Rotation.Y,cam.Rotation.Z);

		}
    }

    public override void _Process(double delta)
    {
		if (ms.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) {
			return;
		}

        if (currentAirJumps < airJumpCount) {
			if (IsOnFloor()) {
				currentAirJumps = airJumpCount;
			}
		}
    }


    


	public override void _PhysicsProcess(double delta)
	{	
		if (ms.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) {
			GlobalPosition = GlobalPosition.Lerp(syncPos, 0.5f);
			return;
		}

		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		Vector3 direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && !isSliding)
		{
			if (IsOnFloor()) {
				velocity.Y = JumpVelocity;
			} else if (currentAirJumps > 0) {
				// Jump and add additional directional air control with the jump
				velocity.Y = JumpVelocity;
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
				currentAirJumps -= 1;
			}
			
		}
		
		//handle slide init
		if (Input.IsActionJustPressed("slide") && IsOnFloor() && !isSliding) {
			anim.Play("slide");
			isSliding = true;
			if (direction == Vector3.Zero) {
				inputDir = new Vector2(0, -1);
				direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			}

			slideDir = direction;
			
		}
		//handle in slide
		if (isSliding) {
			velocity.X = slideDir.X * (Speed + slideSpeedup);
			velocity.Z = slideDir.Z * (Speed + slideSpeedup);
			if (superjump){
				if (Input.IsActionJustPressed("jump")) {
					velocity.Y = JumpVelocity* 1.5f;
				}
			}
		//handle move
		} else if (IsOnFloor()){
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * Speed, delta*7.0); 
				velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * Speed, delta*7.0);
			}
		} else {
			velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * Speed, delta*2.0); 
			velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * Speed, delta*2.0);
		}
			
		
		Velocity = velocity;
		MoveAndSlide();
		bobLoc += (float)(delta * velocity.Length() * (IsOnFloor() ? 1:0));
		cam.Transform = new Transform3D(cam.Transform.Basis, Headbob(bobLoc));
		var vel_clamp = Mathf.Clamp(velocity.Length(), 0.5, Speed*2);
		var tar_fov = baseFOV + FOVChange * vel_clamp;
		cam.Fov = (float)Mathf.Lerp(cam.Fov, tar_fov,delta*12.0);
		syncPos = GlobalPosition;
	}

	public void SlideFinished() {
		slideDir = Vector3.Zero;
		isSliding = false;
	}

	public void toggleSuperJump(){
		superjump = !superjump;
	}

	private static Vector3 Headbob(float loc) {
		var pos = Vector3.Zero;
		pos.Y = Mathf.Sin(loc * bobFreq) * bobAmp;
		pos.X = Mathf.Cos(loc * bobFreq/2) * bobAmp;	
		return pos;
	}
}
