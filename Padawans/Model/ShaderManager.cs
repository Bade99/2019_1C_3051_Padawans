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
        List<TgcMesh> normal_meshes;
        List<TgcMesh> shadow_meshes;
        Effect shader;

        public ShaderManager()
        {
            normal_meshes = new List<TgcMesh>();
            shadow_meshes = new List<TgcMesh>();
        }

        public void RenderMesh(MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case MESH_TYPE.DEFAULT: normal_meshes.ForEach(m => m.Render()); break;
                case MESH_TYPE.SHADOW: shadow_meshes.ForEach(m => m.Render()); break;
            }
        }

        public void AgregarMesh(TgcMesh mesh, MESH_TYPE tipo)
        {
            
            switch (tipo)
            {
                case MESH_TYPE.DEFAULT:normal_meshes.Add(mesh);break;
                case MESH_TYPE.SHADOW:shadow_meshes.Add(mesh); SetEffect(mesh); break;
            }
        }

        public void Effect(Effect shader)//el effect q se va a usar, nunca cambia
        {
            this.shader = shader;
        }

        public void SetTechnique(string technique,MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case MESH_TYPE.DEFAULT: normal_meshes.ForEach(m => m.Technique = technique); break;
                case MESH_TYPE.SHADOW: shadow_meshes.ForEach(m => m.Technique = technique); break;
            }
            
        }

        private void SetEffect(TgcMesh mesh)
        {
            mesh.Effect = shader;
        }

        public enum MESH_TYPE
        {
            DEFAULT,SHADOW
        }
    }
}
