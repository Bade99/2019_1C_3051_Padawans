using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Esta interfaz deberian implementarla todos los objetos que necesitan interaccion de teclado o mouse
    /// </summary>
    public interface InteractiveElement
    {
        void UpdateInput(TgcD3dInput input,float ElapsedTime);
    }
}