﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class StoryMenu : IMenu
    {
        private Drawer2D drawer2D;
        private CustomSprite texto1, texto2, texto3, texto_skip;
        private CustomBitmap b1,b2,b3,skip;
        private bool isCurrent = true;
        private Microsoft.DirectX.DirectInput.Key skipKey;
        float timer=0;
        public StoryMenu(Microsoft.DirectX.DirectInput.Key skipKey) {
            drawer2D = new Drawer2D();
            texto1 = new CustomSprite();
            texto2 = new CustomSprite();
            texto3 = new CustomSprite();
            texto_skip = new CustomSprite();
            this.skipKey = skipKey;
            b1 = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\Start\\1.png", D3DDevice.Instance.Device);
            b2 = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\Start\\2.png", D3DDevice.Instance.Device);
            b3 = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\Start\\3.png", D3DDevice.Instance.Device);
            skip = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\Start\\skip.png", D3DDevice.Instance.Device);
            texto1.Position = new TGCVector2(D3DDevice.Instance.Width * .1f, D3DDevice.Instance.Height * .1f);
            texto2.Position = new TGCVector2(D3DDevice.Instance.Width * .5f, D3DDevice.Instance.Height * .3f);
            texto3.Position = new TGCVector2(D3DDevice.Instance.Width * .2f, D3DDevice.Instance.Height * .6f);
            texto_skip.Position = new TGCVector2(D3DDevice.Instance.Width * .9f, D3DDevice.Instance.Height * .8f);
            texto1.Scaling = CommonHelper.CalculateRelativeScaling(b1,.3f);
            texto2.Scaling = CommonHelper.CalculateRelativeScaling(b2, .3f);
            texto3.Scaling = CommonHelper.CalculateRelativeScaling(b3, .3f);
            texto_skip.Scaling = CommonHelper.CalculateRelativeScaling(skip, .05f);
            texto1.Bitmap = b1;
            texto2.Bitmap = b2;
            texto3.Bitmap = b3;
            texto_skip.Bitmap = skip;
            VariablesGlobales.managerSonido.PauseAll();
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.FORCE_THEME);
        }
        public bool CheckStartKey(TgcD3dInput input)
        {
            return false;
        }
        public void Update(TgcD3dInput input)
        {
            timer += VariablesGlobales.elapsedTime;
            if (input.keyPressed(skipKey) || timer>138)
            {
                isCurrent = false;
                VariablesGlobales.managerSonido.StopID(SoundManager.SONIDOS.FORCE_THEME);
                VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.FLYBY_2);
                VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.XWING_ENGINE);
                VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.BACKGROUND_BATTLE);
                VariablesGlobales.managerSonido.StopID(SoundManager.SONIDOS.EXPLOSION_FINAL);
                Dispose();
            }
        }
        public void Render()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);

            drawer2D.BeginDrawSprite();

            if(CommonHelper.Between(timer,1,5))
                drawer2D.DrawSprite(texto1);
            if(CommonHelper.Between(timer,4,8))
                drawer2D.DrawSprite(texto2);
            if(CommonHelper.Between(timer,7,11))
                drawer2D.DrawSprite(texto3);
            if(CommonHelper.Between(timer,13,138))
                drawer2D.DrawSprite(texto_skip);

            drawer2D.EndDrawSprite();
        }
        public void Dispose()
        {
            texto1.Dispose();
            texto2.Dispose();
            texto3.Dispose();
            texto_skip.Dispose();
        }
        public bool IsCurrent()
        {
            return isCurrent;
        }
    }
}
