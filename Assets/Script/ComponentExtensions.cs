// Copyright 2020 Aumoa.lib. All right reserved.

using UnityEngine;

public static class ComponentExtensions
{
    public static T AddActorComponent<T>(this GameObject @this, params object[] args) where T : ActorComponent
    {
        var actorComponent = @this.AddComponent<T>();
        actorComponent.Construct(args);
        return actorComponent;
    }

    public static T AddSceneComponent<T>(this GameObject @this, string name = null) where T : Component
    {
        var gameObject = new GameObject(name ?? typeof(T).Name);
        gameObject.transform.parent = @this.transform;

        var sceneComponent = gameObject.AddComponent<T>();
        return sceneComponent;
    }
}