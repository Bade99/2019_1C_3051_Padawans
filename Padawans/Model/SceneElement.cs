using System;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;
/// <summary>
///     Clase abstracta que deberian implementar todos los objetos que van a estar en escena, y que sean renderizables y/o modificados en el update
/// </summary>

public abstract class SceneElement
{
    public abstract void Render();

    public abstract void RenderBoundingBox();

    public abstract void Update();

    public abstract void Dispose();
}
