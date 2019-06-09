using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public static class VariablesGlobales
    {
        public static string mediaDir;
        public static string shadersDir;
        public static TgcSceneLoader loader;
        public static Microsoft.DirectX.DirectSound.Device soundDevice;
        public static float elapsedTime;
        public static SoundManager managerSonido;
        public static PhysicsEngine physicsEngine;
        public static TemporaryElementManager managerElementosTemporales;
        public static EnemyManager managerEnemigos;
        public static bool BULLET=true;
        public static Xwing xwing;
        public static bool POSTPROCESS = true;
        public static PostProcess postProcess;
        public static bool SOUND=true;
        public static float time = 5;//para testeos con temporizador
        public static bool SHADERS = true;
        public static Effect shader;//effect que todos los objetos a renderizar deben usar
    }
}
