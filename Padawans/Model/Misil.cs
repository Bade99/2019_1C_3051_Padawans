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
        private TGCBox misil;
        private TGCVector3 escala;
        private TGCVector3 posicion;
        private CoordenadaEsferica coordenadaEsferica;
        private float tiempoDeVida=10f;
        private readonly float velocidadGeneral = 0.05f;
        private bool terminado = false;
        private TgcMp3Player sonido;
        string mediaDir;
        private float tiempoDeVidaSonido=1f;
        private bool sonidoTerminado = false;
        public Misil(TGCVector3 posicionXwing, TGCVector3 offset, CoordenadaEsferica coordenadaEsferica)
        {
            
            misil = new TGCBox();
            escala = new TGCVector3(100f, 100f, 100f);
            posicion = posicionXwing;
            this.coordenadaEsferica = coordenadaEsferica;
            misil.AutoTransformEnable = false;
            misil.Color = Color.Red;
            misil.Enabled = true;
            sonido = new TgcMp3Player();
            sonido.FileName = mediaDir+"\\Sonidos\\TIE_fighter_fire_1.mp3";
            //sonido.play(true);
        }

        public void Update(float ElapsedTime)
        {
            tiempoDeVida -= ElapsedTime;
            
            if (tiempoDeVida < 0f)
            {
                //terminado = true;
            }
            else {
                TGCVector3 delta = new TGCVector3(
                    coordenadaEsferica.GetXCoord() * velocidadGeneral * ElapsedTime,
                    coordenadaEsferica.GetYCoord() * velocidadGeneral * ElapsedTime,
                    coordenadaEsferica.GetZCoord() * velocidadGeneral * ElapsedTime);
                posicion = CommonHelper.SumarVectores(posicion, TGCVector3.Empty);
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
            if (!terminado)
            {
                misil.Transform = TGCMatrix.Scaling(escala) * TGCMatrix.RotationYawPitchRoll(1,1,1) * TGCMatrix.Translation(posicion);
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
