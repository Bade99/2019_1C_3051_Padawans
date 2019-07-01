using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;
namespace TGC.Group.Model
{
    public class ShaderManager
    {
        List<IShaderObject> objs;
        Effect shader;

        public ShaderManager()
        {
            objs = new List<IShaderObject>();
        }

        public void RenderMesh(MESH_TYPE tipo)
        {
            objs.ForEach(obj => obj.Render(tipo));
        }

        public Effect AskForEffect(MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case MESH_TYPE.DEFAULT:return null;
                case MESH_TYPE.SHADOW:return shader;
                case MESH_TYPE.DEAD: return shader;
                case MESH_TYPE.DYNAMIC_ILLUMINATION: return shader;
                case MESH_TYPE.DYNAMIC_ILLUMINATION_METALLIC: return shader;
                default: return null;
            }
        }
        public void SetFloatValue(string param, float value)
        {
            shader.SetValue(param, value);
        }

        public void SetFloatArray3Value(string param,float[] value)
        {
            shader.SetValue(param, value);
        }

        public void AddObject(IShaderObject obj)//cargá tu clase acá
        {
            objs.Add(obj);
        }

        public void Effect(Effect shader)//el effect q se va a usar, nunca cambia
        {
            this.shader = shader;
        }

        public void SetTechnique(string technique,MESH_TYPE tipo)//indica que technique debe usar que tipo de mesh
        {
            objs.ForEach(obj => obj.SetTechnique(technique, tipo));
            //normal_meshes.ForEach(m => m.Technique = technique); 
        }

        /*
        private void SetEffect(TgcMesh mesh)
        {
            mesh.Effect = shader;
        }
        */
        public enum MESH_TYPE
        {
            DEFAULT,SHADOW,DEAD,DYNAMIC_ILLUMINATION, DYNAMIC_ILLUMINATION_METALLIC
        }
    }
}
