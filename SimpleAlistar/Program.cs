using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleAlistar
{
    class Program
    {
        private Menu menu;
        private Spell Q, W, E;

        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Alistar") return;

            Q = new Spell(SpellSlot.Q, 365);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 575);

            W.SetTargetted(0.5f, float.MaxValue);

            menu = new Menu("Simple Alistar", "SimpleAlistar", true);
            menu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.Red)));
            menu.AddItem(new MenuItem("DrawW", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.Red)));
            menu.AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(new Circle(false, System.Drawing.Color.Green)));
            menu.AddItem(new MenuItem("ComboKey", "WQ Combo Key").SetValue(new KeyBind('C', KeyBindType.Press)));
            TargetSelector.AddToMenu(menu);
            menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        void Game_OnUpdate(EventArgs args)
        {
            if (menu.Item("ComboKey").GetValue<KeyBind>().Active)
            {
                Obj_AI_Base target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (Q.IsReady() && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.CastOnUnit(target);
                    var jumpTime = Math.Max(0, ObjectManager.Player.Distance(target) - 500) * 10 / 25 + 25;
                    Utility.DelayAction.Add((int) jumpTime, () => Q.Cast());
                }
            }
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("DrawQ").GetValue<Circle>().Active)
                Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, menu.Item("DrawQ").GetValue<Circle>().Color);
            if (menu.Item("DrawW").GetValue<Circle>().Active)
                Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, menu.Item("DrawW").GetValue<Circle>().Color);
            if (menu.Item("DrawE").GetValue<Circle>().Active)
                Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, menu.Item("DrawE").GetValue<Circle>().Color);
        }
    }
}
