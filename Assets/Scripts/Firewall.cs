using UnityEngine;

public class Firewall : Entity
{
    private int health = 5;

    public override void Onhit(Vector2Int attackDirection)
    {
        if (GridManager == null)
        {
            return;
        }
        health--;
        if (health <= 0)
        {
            Die();
        }   
    }
}
