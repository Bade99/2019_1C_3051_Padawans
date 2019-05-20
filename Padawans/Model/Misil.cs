using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class Misil : IActiveElement
    {
        TgcMesh misil; //buscar o crear un mesh que sea una caja
        private TGCVector3 escala;
        private TGCVector3 posicion;
        private CoordenadaEsferica coordenadaEsferica;
        private float tiempoDeVida = 10f;
        private readonly float velocidadGeneral = 500f;
        private bool terminado = false;
        private TgcMp3Player sonido;
        private float tiempoDeVidaSonido = 1f;
        private bool sonidoTerminado = false;

        private TGCVector3 rotacionBase;
        private TGCVector3 rotacionNave;

       

        public Misil(TGCVector3 posicionXwingAla, CoordenadaEsferica coordenadaEsferica, TGCVector3 rotacionNave)
        {
            this.rotacionNave = rotacionNave;
            
            misil = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "\\Misil\\Misil-TgcScene.xml").Meshes[0];//crear un mesh caja
            misil.AutoTransformEnable = false;
                    
            rotacionBase = new TGCVector3(FastMath.PI_HALF,0,0);
            escala = new TGCVector3(.5f, .5f, .5f);
            posicion = posicionXwingAla;
            
            this.coordenadaEsferica = coordenadaEsferica;
           
            sonido = new TgcMp3Player();
            sonido.FileName = VariablesGlobales.mediaDir+"\\Sonidos\\TIE_fighter_fire_1.mp3";

            try{
                sonido.play(true);
            } catch (System.Exception e)
            {
                sonidoTerminado = true;
            }
        }

        public void Update(float ElapsedTime)
        {

            tiempoDeVida -= ElapsedTime;
            
            if (tiempoDeVida < 0f)
            {
                terminado = true;
            }
            else {
                TGCVector3 delta = new TGCVector3(
                    coordenadaEsferica.GetXCoord() * velocidadGeneral * ElapsedTime,
                    coordenadaEsferica.GetYCoord() * velocidadGeneral * ElapsedTime,
                    coordenadaEsferica.GetZCoord() * velocidadGeneral * ElapsedTime);

                posicion = CommonHelper.SumarVectores(posicion, delta);

                misil.Transform = TGCMatrix.Scaling(escala) * TGCMatrix.RotationY(FastMath.PI_HALF) * TGCMatrix.RotationYawPitchRoll(rotacionNave.Y,rotacionNave.X,rotacionNave.Z) * TGCMatrix.Translation(posicion);
            }
            if (!sonidoTerminado)
            {
                if (tiempoDeVidaSonido < 0f)
                {
                    sonido.closeFile();
                    sonidoTerminado = true;
                }
                else tiempoDeVidaSonido -= ElapsedTime;
            }

        }

        public bool Terminado()
        {
            return terminado;
        }

        public void Render()
        {

            
            misil.Render();

            if (!terminado)
            {
                misil.Render();
            }
        }

        public void RenderBoundingBox()
        {





            if (!terminado)
            {
                misil.BoundingBox.Render();
            }
        }

        public void Dispose()
        {
            misil.Dispose();
        }

    }
}
