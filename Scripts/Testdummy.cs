using Godot;
using System;

public partial class Testdummy : CharacterBody3D, IDamageable
{
	[Export]
	public int maxHealth = 100;
	public int health;

    public override void _Ready()
    {
        health = maxHealth;
    }

    public void Damage(int amount)
    {
        health -= amount;
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
