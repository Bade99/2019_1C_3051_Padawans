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
    ///     Mantiene todos los objetos susceptibles a que sea renderizado su boundingBox y gestiona su renderizado
    /// </summary>
    public class BoundingBoxHelper : InteractiveElement
    {
        private SceneElement[] parameters;
        private ActiveElementManager[] parametrosTemporales;
        private bool renderBoundingBoxes;
        /// <summary>
        ///    Este constructor tiene parametros variables, se pueden agregar con comas cuantos objetos se quiera
        /// </summary>
        public BoundingBoxHelper(SceneElement[] parameters,ActiveElementManager[] parametrosTemporales)
        {
            this.parameters = parameters;
            this.parametrosTemporales = parametrosTemporales;
        }

        public void RenderBoundingBoxes()
        {
            if (renderBoundingBoxes)
            {
                foreach (SceneElement element in parameters)
                {
                    element.RenderBoundingBox();
                }
                foreach (IActiveElement element in parametrosTemporales)
                {
                    element.RenderBoundingBox();
                }
            }
        }

        public void UpdateInput(TgcD3dInput input,float ElapsedTime)
        {
            if (input.keyPressed(Key.F))
            {
                renderBoundingBoxes = !renderBoundingBoxes;
            }
        }
    }
}