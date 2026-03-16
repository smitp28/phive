using UnityEngine;

public interface ICompanionMovement
{
    void SetMoveDirection(Vector2 direction);
    Vector2 Position { get; }
}