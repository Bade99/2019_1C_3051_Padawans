using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    class MenuManager : Manager<IMenu>
    {
        IMenu currentMenu;

        public MenuManager(params IMenu[] parameters)//el primer param debe ser el menu inicial para que el programa inicie!
        {
            this.elems = new List<IMenu>();
            this.currentMenu = parameters[0];
            foreach (IMenu p in parameters) elems.Add(p);
        }
        public void Update(TgcD3dInput input) {
             if (currentMenu != null && currentMenu.IsCurrent()) {
                currentMenu.Update(input);
            } 
            else
            {
                elems.ForEach(elem => { if (elem.CheckStartKey(input)) { currentMenu = elem;return; } } );
            }

        }
        public void Render()
        {
            if (IsCurrent()) currentMenu.Render();
        }
        public void Dispose() {
            elems.ForEach(elem => { elem.Dispose(); });
            RemoverTodosLosElementos();
        }
        public bool IsCurrent()
        {
            if (currentMenu!=null && currentMenu.IsCurrent()) return true;
            else return false;
        }
    }
}
