using UnityEngine;
// interface for interaction requirements, probably should
// pass a data object instead of player
public abstract class InteractionRequirement : MonoBehaviour
{
    public abstract bool IsMet(PlayerController player);
}