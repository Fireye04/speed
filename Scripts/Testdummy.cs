using Godot;
using System;

public partial class Testdummy : CharacterBody3D, IDamageable
{
	[Export]
	public int maxHealth = 100;
	public int health;

	public Vector3 knockback = Vector3.Zero;

    public override void _Ready()
    {
        health = maxHealth;
    }

    public override void _PhysicsProcess(double delta)
    {	
		Vector3 velocity = Velocity;
		
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (knockback != Vector3.Zero) {
			// Apply knockback
			velocity = Vector3.Zero + knockback;

			// Reduce knockback and cut it off when it gets small enough
			if (Math.Abs(knockback.X) > 0.5 && Math.Abs(knockback.Y) > 0.5 && Math.Abs(knockback.Z) > 0.5) {
				knockback = Vector3.Zero;
			} else {
				knockback = LerpV3(knockback, Vector3.Zero, 0.1f);
			}
		} else {
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}else {
				velocity = Vector3.Zero;
			}
		}

		

		
		Velocity = velocity;
		MoveAndSlide();
		
		
		
    }

	public Vector3 LerpV3(Vector3 from, Vector3 to, float inc) {
		return new Vector3(Mathf.Lerp(from.X, to.X, inc), Mathf.Lerp(from.Y, to.Y, inc), Mathf.Lerp(from.Z, to.Z, inc));
	}

    public void Damage(int amount, Godot.Vector3 kb, float Str)
    {
        health -= amount;
		var kbFinal = kb * (Str);
		kbFinal.Y += 10;
		knockback = kbFinal;
		if (health <= 0) {
			GD.Print("FUCK I'M DEAD");
		} else {
			GD.Print("ow");
		}
    }

    public void Heal(int amount)
    {
        health += amount;
		if (health > maxHealth) {
			GD.Print("full health");
			health = maxHealth;
		} else {
			GD.Print("heal");
		}
    }
}
