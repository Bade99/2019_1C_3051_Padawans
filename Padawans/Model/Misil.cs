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
        private CoordenadaEsferica coordenadaEsferica;
        private float tiempoDeVida = 10f;
        private readonly float velocidadGeneral = 400f;
        private bool terminado = false;
        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE = 84;
        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL = 6;
        private float EXTRA_ANGULO_POLAR_CANION_ABAJO = 0.3f;
        private float EXTRA_DISTANCIA_CANION_ABAJO = 0.5f;

        private TGCVector3 rotacionBase;
        private TGCVector3 rotacionNave;
       
        /**
         * Posicion nave: Posicion inicial del misil
         * coordenadaEsfericaP: Direccion y sentido del misil
         * Rotacion nave: Constante con el que misil se rota inicialmente para estar alineado a la trayectoria
         * pathscene: Path donde esta ubicado el mesh
         * desfasajeAlas: True para calcular un desfasaje extra desde el inicio. Unicamente para xwing
         * */
        public Misil(TGCVector3 posicionNave, CoordenadaEsferica coordenadaEsfericaP, TGCVector3 rotacionNave, string pathScene, bool desfasajeAlas)
        {
            this.rotacionNave = rotacionNave;
            this.coordenadaEsferica = coordenadaEsfericaP;
            
            misil = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];
            misil.AutoTransformEnable = false;
                    
            rotacionBase = new TGCVector3(FastMath.PI_HALF,0,0);
            escala = new TGCVector3(.2f, .2f, 8f);

            //Calcula un pequenio desfasaje para dar el efecto de que el misil sale desde las alas.
            //Solo setear en true para el xwing
            if (desfasajeAlas)
            {
                posicion = calcularPosicionInicialMisil(posicionNave);
            } else
            {
                posicion = posicionNave;
            }
            
        }
        /**
         * Devuelve la posicion inicial desde donde sale el misil en funcion de la posicion y la rotacion de la nave.
         * */
        private TGCVector3 calcularPosicionInicialMisil(TGCVector3 posicionNave)
        {
            //Random de las 4 opciones posibles de caniones
            Random random = new Random();
            int canion = (int)Math.Round(random.NextDouble() * 4);
            int signo = 0;
            bool canionInferior = false;
            //Caniones izquierdos o derechos
            if (canion==0 || canion ==2)
            {
                signo = 1;
            } else
            {
                signo = -1;
            }
            //Caniones inferiores o superiores
            if (canion == 2 || canion == 3)
            {
                canionInferior = true;
            } else
            {
                canionInferior = false;
            }
            //Delta en la direccion nave (para que no parezca que sale de atras)
            TGCVector3 deltaDireccionNave = new TGCVector3(coordenadaEsferica.GetXCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE,
                coordenadaEsferica.GetYCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE, coordenadaEsferica.GetZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE);
            //Delta en la direccion ortogonal a la direccion de la nave (para que salga desde alguna de las alas)
            //Caniones inferiores
            if (canionInferior)
            {
                CoordenadaEsferica direccionOrtogonal = new CoordenadaEsferica(coordenadaEsferica.acimutal + (FastMath.PI / 2) * signo, FastMath.PI / 2 + EXTRA_ANGULO_POLAR_CANION_ABAJO);
                TGCVector3 deltaOrtogonalNave =
                    new TGCVector3(direccionOrtogonal.GetXCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetYCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL);
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(posicionNave, deltaDireccionNave), deltaOrtogonalNave);
            } else
            //Caniones superiores
            {
                CoordenadaEsferica direccionOrtogonal = new CoordenadaEsferica(coordenadaEsferica.acimutal + (FastMath.PI / 2) * signo, FastMath.PI / 2);
                TGCVector3 deltaOrtogonalNave =
                    new TGCVector3(direccionOrtogonal.GetXCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetYCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL);
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(posicionNave, deltaDireccionNave), deltaOrtogonalNave);
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
