using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;


namespace TGC.Group.Model
{
    public class MiniMap
    {
        private Drawer2D drawer2D;
        float W = D3DDevice.Instance.Width;
        float H = D3DDevice.Instance.Height;
        string texturaPixel;
        string texturaFondo;
        private float tiempoEntreActualizacion = .2f;
        private float tiempoDesdeUltimaActualizacion = .2f;

        public enum UbicacionMapa
        {
            FUERA_X_IZQ,
            FUERA_X_DER,
            FUERA_Z_ARRIBA,
            FUERA_Z_ABAJO,
            DENTRO,
        }

        public struct Proyeccion
        {
            public ITarget objeto;
            public CustomSprite sprite;
        }

        //ESCENA ORIGINAL
        float origInicialX = 600;
        float origInicialZ = 1000;
        float origTamanioX = 1200;
        float origTamanioZ = 2400;

        //ESCENA MAPA
        float mapaInicialX;
        float mapaInicialZ;
        float mapaTamanioX;
        float mapaTamanioZ;
        float escala = 10;

        Proyeccion objetoTarget;

        private CustomSprite spriteFondo;
        private List<Proyeccion> objetos;


        public MiniMap(Drawer2D drawer)
        {
            this.drawer2D = drawer;
            this.texturaPixel = VariablesGlobales.mediaDir + "Minimap\\pixel3.png";
            this.texturaFondo = VariablesGlobales.mediaDir + "Minimap\\mapaBomba.png";

            this.objetos = new List<Proyeccion>();

            this.spriteFondo = crearMapa(this.origTamanioX,this.origTamanioZ,this.escala);
        }

        public void agregarTarget(ITarget objeto)
        {
            this.objetoTarget = new Proyeccion();
            this.objetoTarget.objeto = objeto;
            this.objetoTarget.sprite = crearCuadrado(1200, 500, 10, 10, texturaPixel, Color.Blue);
        }

        public void agregarObjeto(ITarget objeto)
        {
            Proyeccion objetoProy = new Proyeccion();
            objetoProy.objeto = objeto;
            objetoProy.sprite = crearCuadrado(1200, 500, 10, 10, texturaPixel, Color.Red);
            this.objetos.Add(objetoProy);
        }

        public void agregarObjetoMeta(ITarget objeto)
        {
            Proyeccion objetoProy = new Proyeccion();
            objetoProy.objeto = objeto;
            objetoProy.sprite = crearCuadrado(1200, 500, 10, 10, texturaPixel, Color.Yellow);
            this.objetos.Add(objetoProy);
        }

        public void Update()
        {
            tiempoDesdeUltimaActualizacion += VariablesGlobales.elapsedTime;
            if (tiempoDesdeUltimaActualizacion > tiempoEntreActualizacion)
            {
                tiempoDesdeUltimaActualizacion = 0f;

                //Corro el foco del mapa cuando el target avanza en la pista
                if (dentroDeLimites(this.objetoTarget.objeto.GetPosition()) == UbicacionMapa.FUERA_Z_ARRIBA)
                    this.origInicialZ = this.origInicialZ - this.origTamanioZ;
                if (dentroDeLimites(this.objetoTarget.objeto.GetPosition()) == UbicacionMapa.FUERA_Z_ABAJO)
                    this.origInicialZ = this.origInicialZ + this.origTamanioZ;

                this.objetoTarget.sprite.Position = proyectarEnMapa(this.objetoTarget.objeto.GetPosition());

                this.objetos.ForEach(obj => {
                    if (dentroDeLimites(obj.objeto.GetPosition()) == UbicacionMapa.DENTRO)
                    {
                        obj.sprite.Position = proyectarEnMapa(obj.objeto.GetPosition());
                    }
                });
            } 
        }

        public void Render()
        {
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(spriteFondo);
            if (dentroDeLimites(this.objetoTarget.objeto.GetPosition()) == UbicacionMapa.DENTRO)
                drawer2D.DrawSprite(this.objetoTarget.sprite);

            this.objetos.ForEach(obj => {
                if (dentroDeLimites(obj.objeto.GetPosition()) == UbicacionMapa.DENTRO)
                    drawer2D.DrawSprite(obj.sprite);
            });
            drawer2D.EndDrawSprite();
        }

        private TGCVector2 proyectarEnMapa(TGCVector3 posicion3d)
        {
            //float x = mapaInicialX + (posicion3d.X - origInicialX) / this.escala;
            float x = mapaInicialX + (origInicialX - posicion3d.X) / this.escala;
            //float z = (mapaInicialZ) - (posicion3d.Z - origInicialZ) / this.escala;
            float z = (mapaInicialZ) - (origInicialZ - posicion3d.Z) / this.escala;

            return new TGCVector2(x, z);
        }

        private UbicacionMapa dentroDeLimites(TGCVector3 posicion3d)
        {
            if (posicion3d.Z < this.origInicialZ - this.origTamanioZ) return UbicacionMapa.FUERA_Z_ARRIBA;
            if (posicion3d.Z > this.origInicialZ) return UbicacionMapa.FUERA_Z_ABAJO;
            if (posicion3d.X > this.origInicialX) return UbicacionMapa.FUERA_X_IZQ;
            if (posicion3d.X < this.origInicialX - this.origTamanioX) return UbicacionMapa.FUERA_X_DER;
            return UbicacionMapa.DENTRO;
        }

        private CustomSprite crearMapa(float origTamanioX, float origTamanioZ, float escala)
        {
            int dx = 30;
            int dz = 30;
            float ancho = origTamanioX / escala;
            float largo = origTamanioZ / escala;
            float posx = this.W - ancho - dx;
            float posz = this.H - largo - dz;
            this.mapaTamanioX = ancho;
            this.mapaTamanioZ = largo;
            this.mapaInicialX = posx;
            this.mapaInicialZ = posz + largo;

            return crearCuadrado(posx, posz, ancho, largo, texturaFondo, Color.Green);
        }

        private CustomSprite crearCuadrado(float posicionX, float posicionZ, float ancho, float alto, string textura, Color color)
        {
            CustomSprite sprite = new CustomSprite();
            sprite.Bitmap = new CustomBitmap(textura, D3DDevice.Instance.Device);
            sprite.Position = new TGCVector2(posicionX, posicionZ);
            //No va directo el Alto y Ancho porque depende del tamanio que tiene la textura
            sprite.Scaling = new TGCVector2(ancho/sprite.Bitmap.Width, alto/sprite.Bitmap.Height);
            //sprite.Scaling = new TGCVector2(ancho, alto);
            if (color != Color.Transparent) sprite.Color = color;
            return sprite;
        }

        /*private void dibujarCuadradoRelleno2(float posicionX, float posicionZ, int ancho, int alto, Color color)
        {
            drawer2D.DrawLine(new TGCVector2(posicionX, posicionZ - alto/2), new TGCVector2(posicionX, posicionZ + alto / 2), color, ancho, false);
        }*/
        
    }
}
