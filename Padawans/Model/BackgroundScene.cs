using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class BackgroundScene : IShaderObject
    {
        List<TgcMesh> naves;
        Random rnd;
        float timer=0;
        TGCVector3 posInicial, posFinal;
        float velocidad=10;
        //bool active = false;

        public BackgroundScene(TGCVector3 posInicial,TGCVector3 largo,int cantElements)//ej (10,5,0) y (100,100,100) -> de (10,5,0) a (110,105,100)
        {
            naves = new List<TgcMesh>();
            this.posInicial = posInicial;
            this.posFinal = posInicial + largo;
            rnd = new Random();
            CrearBackground(posInicial, largo,cantElements);

            VariablesGlobales.shaderManager.AddObject(this);
        }

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.DEFAULT:
                    //if (!active) return;
                    naves.ForEach(n => n.Render());
                    break;
            }
        }

        public void Update()
        {
            /*
            if (!CommonHelper.InDistance((posInicial+posFinal)*.5f, VariablesGlobales.xwing.GetPosition(), 5000))
            {
                active = false;
                return;
            }
            else active = true;
            */
            timer += VariablesGlobales.elapsedTime;
            naves.ForEach(n => n.Position += n.Rotation*velocidad*VariablesGlobales.elapsedTime);
            if (timer > 10)
            {
                naves.ForEach(n=> { if (SePasa(n.Position)) n.Rotation *= -1; });
                timer = 0;
            }
        }

        private bool SePasa(TGCVector3 pos)
        {
            if (pos.X < posInicial.X || pos.X > posFinal.X) return true;
            if (pos.Y < posInicial.Y || pos.Y > posFinal.Y) return true;
            if (pos.Z < posInicial.Z || pos.Z > posFinal.Z) return true;
            return false;
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo){}

        private void CrearBackground(TGCVector3 inicio,TGCVector3 largo,int cantElems)
        {
            for(int i = 0; i < cantElems; i++)
            {
                string rndNave = GetRndName();
                TgcMesh nave = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Misil\\" + rndNave).Meshes[0];
                nave.Position = inicio + 
                        new TGCVector3( largo.X * (float)rnd.NextDouble(), 
                                        largo.Y * (float)rnd.NextDouble(),
                                        largo.Z * (float)rnd.NextDouble());
                nave.Rotation = new TGCVector3(FastMath.PI * (float)rnd.NextDouble(),
                                               FastMath.PI * (float)rnd.NextDouble(),
                                               FastMath.PI * (float)rnd.NextDouble());
                nave.Scale = TGCVector3.One *.5f * (float)rnd.NextDouble();
                naves.Add(nave);
            }
        }
        private string GetRndName()
        {
            switch (rnd.Next(1, 4))
            {
                case 1:
                    return "misil_torreta.xml";
                case 2:
                    return "misil_xwing_enemigo-TgcScene.xml";
                default:
                    return "misil_xwing-TgcScene.xml";
            }
        }
    }
}
