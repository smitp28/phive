using UnityEngine;

public interface ICompanionAttack
{
    /// <summary>AI calls this when enemy is in range.</summary>
    void TriggerAttack(Transform target);

    /// <summary>Each attack type defines its own range.</summary>
    float AttackRange { get; }

    bool IsReady { get; }
}