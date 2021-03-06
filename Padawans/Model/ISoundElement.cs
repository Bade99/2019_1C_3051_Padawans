﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public interface ISoundElement
    {
        void Update();
        bool Terminado();
        void Terminar();
        string GetID();
        void Dispose();
        void Pause();
        void Resume();
        void Play();
        void Stop();
        bool IsStoppeado();
        void mutear();
        void unmute();
    }
}
