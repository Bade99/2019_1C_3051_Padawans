using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;

namespace TGC.Group.Model
{
    public class HUD
    {
        private CustomSprite HUD_misiles,HUD_vidas;
        private List<CustomBitmap> vidas;
        private List<CustomBitmap> misiles;
        private Drawer2D drawer2D;
        private TGCVector2 posInit;
        private MiniMap miniMap;
        //mira en el centro?

        private string misiles1 = "Bitmaps\\HUD\\HUD_missiles_1.png";
        private string misiles0 = "Bitmaps\\HUD\\HUD_missiles_0.png";
        private string vidas0 = "Bitmaps\\HUD\\HUD_health_0.png";
        private string vidas1 = "Bitmaps\\HUD\\HUD_health_1.png";
        private string vidas2 = "Bitmaps\\HUD\\HUD_health_2.png";
        private string vidas3 = "Bitmaps\\HUD\\HUD_health_3.png";
        private string vidas4 = "Bitmaps\\HUD\\HUD_health_4.png";
        private string vidas5 = "Bitmaps\\HUD\\HUD_health_5.png";
        private string vidas6 = "Bitmaps\\HUD\\HUD_health_6.png";
        private string vidas7 = "Bitmaps\\HUD\\HUD_health_7.png";
        private string vidas8 = "Bitmaps\\HUD\\HUD_health_8.png";

        public HUD()
        {
            drawer2D = new Drawer2D();

            VariablesGlobales.miniMap = new MiniMap(this.drawer2D);
            this.miniMap = VariablesGlobales.miniMap;

            misiles = new List<CustomBitmap>
            {
                new CustomBitmap(VariablesGlobales.mediaDir + misiles0, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + misiles1, D3DDevice.Instance.Device)
            };

            vidas = new List<CustomBitmap>
            {
                new CustomBitmap(VariablesGlobales.mediaDir + vidas0, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas1, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas2, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas3, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas4, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas5, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas6, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas7, D3DDevice.Instance.Device),
                new CustomBitmap(VariablesGlobales.mediaDir + vidas8, D3DDevice.Instance.Device)
            };

            HUD_misiles = new CustomSprite();

            HUD_vidas = new CustomSprite();

            HUD_misiles.Scaling = CommonHelper.CalculateRelativeScaling(misiles[0],.1f);
            HUD_vidas.Scaling = CommonHelper.CalculateRelativeScaling(vidas[0], .1f);

            HUD_misiles.Position = 
                new TGCVector2(D3DDevice.Instance.Width * .1f, D3DDevice.Instance.Height * .8f);

            HUD_vidas.Position =
                new TGCVector2(HUD_misiles.Position.X, HUD_misiles.Position.Y + HUD_misiles.Scaling.Y * misiles[0].Height); //+ D3DDevice.Instance.Height *.05f);
        }

        private CustomBitmap Lives()
        {
            return vidas[VariablesGlobales.vidas];
            //ta tirando excepcion x las multi colisiones
        }

        private CustomBitmap Missiles()
        {
            return misiles[VariablesGlobales.bombas];
            //ta tirando excepcion x las multi colisiones
        }

        public void Render()
        {
            HUD_vidas.Bitmap = Lives();
            HUD_misiles.Bitmap = Missiles();
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(HUD_misiles);
            drawer2D.DrawSprite(HUD_vidas);
            drawer2D.EndDrawSprite();
            if(VariablesGlobales.mostrarMiniMapa)
                this.miniMap.Render();
        }

        public void UpdateMiniMap()
        {
            this.miniMap.Update();
        }

        public void Dispose()
        {
            HUD_misiles.Dispose();
            HUD_vidas.Dispose();
        }
    }
}
