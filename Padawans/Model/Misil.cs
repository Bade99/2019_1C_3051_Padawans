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
        TgcMesh misil;
        private TGCVector3 escala;
        private TGCVector3 posicion;
        //Como el misil nunca cambia la trayectoria, guardo las coordenadas cartesianas de la coordenada
        //esferica para no calcular tantos senos y cosenos
        private float xCoordEsferica;
        private float yCoordEsferica;
        private float zCoordEsferica;
        private float tiempoDeVida = 10f;
        private readonly float velocidadGeneral = 1000f;
        private bool terminado = false;
        private TGCVector3 rotacionNave;
       
        /**
         * Posicion nave: Posicion inicial del misil
         * coordenadaEsfericaP: Direccion y sentido del misil
         * Rotacion nave: Constante con el que misil se rota inicialmente para estar alineado a la trayectoria
         * pathscene: Path donde esta ubicado el mesh
         * */
        public Misil(TGCVector3 posicionNave, CoordenadaEsferica coordenadaEsferica, TGCVector3 rotacionNave, string pathScene, Color color)
        {
            this.rotacionNave = rotacionNave;
            this.xCoordEsferica = coordenadaEsferica.GetXCoord();
            this.yCoordEsferica = coordenadaEsferica.GetYCoord();
            this.zCoordEsferica = coordenadaEsferica.GetZCoord();
            this.posicion = posicionNave;
            misil = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];
            misil.AutoTransformEnable = false;
            misil.setColor(color);//dice q es poco performate hacer esto, si necesitamos performance aca queda algo @
            escala = new TGCVector3(.2f, .2f, 4f);
            
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
                    xCoordEsferica * velocidadGeneral * ElapsedTime,
                    yCoordEsferica * velocidadGeneral * ElapsedTime,
                    zCoordEsferica * velocidadGeneral * ElapsedTime);

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
