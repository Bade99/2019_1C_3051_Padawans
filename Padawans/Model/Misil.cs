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
        private float distanciaOrigenMisil = 100;
        private readonly float velocidadGeneral = 900f;
        private bool terminado = false;

        private TGCVector3 rotacionBase;
        private TGCVector3 rotacionNave;
       

        public Misil(TGCVector3 posicionNave, CoordenadaEsferica coordenadaEsfericaP, TGCVector3 rotacionNave,string pathScene)
        {
            this.rotacionNave = rotacionNave;
            this.coordenadaEsferica = coordenadaEsfericaP;
            
            misil = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];//crear un mesh caja
            misil.AutoTransformEnable = false;
                    
            rotacionBase = new TGCVector3(FastMath.PI_HALF,0,0);
            escala = new TGCVector3(.2f, .2f, 8f);
            posicion = calcularPosicionInicialMisil(posicionNave);
            
        }
        /**
         * Devuelve la posicion inicial desde donde sale el misil en funcion de la posicion y la rotacion de la nave.
         * */
        private TGCVector3 calcularPosicionInicialMisil(TGCVector3 posicionNave)
        {
            Random random = new Random();
            int signo = Math.Sign(random.NextDouble() - 0.5);
            CoordenadaEsferica direccionAlaDesdeElCentro = new CoordenadaEsferica(coordenadaEsferica.acimutal + 0.1f * signo, coordenadaEsferica.polar);
            TGCVector3 deltaInicioMisil = new TGCVector3(direccionAlaDesdeElCentro.GetXCoord() * distanciaOrigenMisil,
                direccionAlaDesdeElCentro.GetYCoord() * distanciaOrigenMisil, direccionAlaDesdeElCentro.GetZCoord() * distanciaOrigenMisil);
            return CommonHelper.SumarVectores(posicionNave, deltaInicioMisil);
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

        }

        public bool Terminado()
        {
            return terminado;
        }

        public void Render()
        {
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
