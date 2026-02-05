using UnityEngine;

public abstract class ActorComponent : MonoBehaviour
{
    public virtual void Construct(params object[] args)
    {
        IsConstructed = true;
    }

    public bool IsConstructed { get; private set; }
}