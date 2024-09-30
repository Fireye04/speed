using Godot;
public interface IDamageable {
    public void Damage(int amount, Vector3 knockbackDir, float knockbackStr);

    public void Heal(int amount);

}