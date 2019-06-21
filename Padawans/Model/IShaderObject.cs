using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public interface IShaderObject
    {
        //1. Llamar a ShaderManager.AskForEffect, te da el efecto que debes asignar para ese tipo de mesh, para MESH_TYPE.DEFAULT devuelve null, no necesitas un efecto especial
        //2. Una vez recibido el effect correspondiente lo tenes que asignar a las meshes que quieras que lo tengan (mesh.Effect=...)
        //3. Llamar a ShaderManager.AddObject para que llame a tu funcion render
        void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo);//necesitas esta funcion para que el manager te pueda decir que technique usar para cada tipo de mesh en cada momento
        void Render(ShaderManager.MESH_TYPE tipo);//renderizas ese tipo de mesh, aca ta la razon de todo esto, asi cada uno puede decidir que renderizar y que no dependiendo de la posicion del xwing,etc...
    }
}
