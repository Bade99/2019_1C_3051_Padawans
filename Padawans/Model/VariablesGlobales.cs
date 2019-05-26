using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;

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
        public static GameModel gameModel;
        public static PhysicsEngine physicsEngine;
    }
}
