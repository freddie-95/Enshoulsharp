using EnsoulSharp;
using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using SharpDX;

namespace AutoInt
{
    internal class AutoInt
    {
        private static Vector3 bottomLeftFountain = new Vector3(604f, 612f, 183.5748f);
        private static Vector3 topRightFountain = new Vector3(14102f, 14194f, 171.9777f);

        private static Vector3 intingVector;

        private static AIHeroClient Me => ObjectManager.Player;

        public static Menu MyMenu;
        
        
        public static void OnLoad()
        {
            MyMenu = new Menu("autoInt", "Auto Int", true);
            MyMenu.Add(new MenuBool("doInt", "Activate").SetValue(false));
            MyMenu.Attach();

            if (Me.Position.Distance(bottomLeftFountain) < Me.Position.Distance(topRightFountain))
            {
                intingVector = topRightFountain;
            }
            else
            {
                intingVector = bottomLeftFountain;
            }
            

            Game.OnUpdate += GameOnUpdate;
        }

        private static void GameOnUpdate(EventArgs args)
        {

            if (MyMenu.GetValue<MenuBool>("doInt"))
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, intingVector);
            }

        }
        
    }
    
}

