using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    public static class VariablesGlobales
    {
        public static string mediaDir;
        public static string shadersDir;
        public static TgcSceneLoader loader;
        public static Microsoft.DirectX.DirectSound.Device soundDevice;
        public static float elapsedTime;
        public static MenuManager managerMenu;
        public static SoundManager managerSonido;
        public static PhysicsEngine physicsEngine;
        public static TemporaryElementManager managerElementosTemporales;
        public static EnemyManager managerEnemigos;
        public static ShaderManager shaderManager;
        public static bool BULLET=true;
        public static Xwing xwing;
        public static bool POSTPROCESS = true;
        public static PostProcess postProcess;
        public static bool SOUND=false;
        public static float timer = 5;//para testeos con temporizador
        public static bool SHADERS = true;
        public static Effect shader;//effect que todos los objetos a renderizar deben usar
        public static bool DameLuz = true;
        public static EndgameManager endgameManager;
        public static TGCVector2 cues_relative_position;
        public static float cues_relative_scale;
        public static int vidas = 4;
        public static int bombas = 1;
        private static int max_bombas = 1;
        public static bool MODO_DIOS = false;
        public static bool debugMode = false;
        public static MiniMap miniMap;
        public static bool mostrarMiniMapa = false;
        public static void RestarVida()
        {
            vidas = FastMath.Max(0, vidas - 1);
        }
        public static void SumarBomba()
        {
            bombas = FastMath.Min(bombas+1, max_bombas);
        }
        public static void RestarBomba()
        {
            bombas = FastMath.Max(bombas - 1, 0);
        }
    }
}
