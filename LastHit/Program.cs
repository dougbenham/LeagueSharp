using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace LastHit
{
    public static class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static double GetOnHitDamage(Obj_AI_Base target)
        {
            double dmgItem = 0;

            if (Items.HasItem((int)ItemId.Sheen) && (Items.CanUseItem((int)ItemId.Sheen) || ObjectManager.Player.HasBuff("sheen")))
            {
                dmgItem = ObjectManager.Player.BaseAttackDamage * 1.0;
            }
            else if (Items.HasItem((int)ItemId.Iceborn_Gauntlet) && (Items.CanUseItem((int)ItemId.Iceborn_Gauntlet) || ObjectManager.Player.HasBuff("itemfrozenfist")))
            {
                dmgItem = ObjectManager.Player.BaseAttackDamage * 1.0;
            }
            else if (Items.HasItem((int)ItemId.Trinity_Force) && (Items.CanUseItem((int)ItemId.Trinity_Force) || ObjectManager.Player.HasBuff("sheen")))
            {
                dmgItem = ObjectManager.Player.BaseAttackDamage * 2.0;
            }

            if (Items.HasItem((int) ItemId.Blade_of_the_Ruined_King))
            {
                var d = 0.06*target.Health;
                if (target is Obj_AI_Minion)
                    d = Math.Min(d, 60);
                dmgItem += d;
            }

            return dmgItem;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            
            /*var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff2 => buff2.Name == "talentreaperdisplay");
            if (buff != null)
            {
                Drawing.DrawText(20, 20, Color.White, "Targon stacks: " + buff.Count + ", " + buff.IsActive + ", " + buff.IsPositive + ", " + buff.IsValid + " - Can use targon item: " + Items.CanUseItem(3302).ToString());
            }*/

            // TODO: Add renekton

            bool aa = false;
            bool onhit = false;
            SpellSlot spell;
            switch (ObjectManager.Player.ChampionName)
            {
                // Q
                case "Ezreal":
                case "Irelia":
                    aa = false;
                    onhit = true;
                    spell = SpellSlot.Q;
                    break;
                case "LeBlanc":
                case "Caitlyn":
                case "MasterYi":
                case "Pantheon":
                case "Riven":
                case "Warwick":
                case "Teemo":
                    aa = false;
                    onhit = false;
                    spell = SpellSlot.Q;
                    break;
                case "Trundle":
                case "Garen":
                case "Nasus":
                case "Vayne":
                    onhit = true;
                    aa = true;
                    spell = SpellSlot.Q;
                    break;

                // W
                case "Ashe":
                    aa = false;
                    onhit = false;
                    spell = SpellSlot.W;
                    break;
                default:
                    return;
            }

            foreach (
                var minion in
                    MinionManager.GetMinions(2000, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(
                            m =>
                                m.IsValidTarget() &&
                                m.Health <=
                                ObjectManager.Player.GetSpellDamage(m, spell) +
                                (aa ? ObjectManager.Player.GetAutoAttackDamage(m) : 0) +
                                (onhit ? GetOnHitDamage(m) : 0)))
            {
                Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius + 8, Color.Blue, 3);
            }
        }
    }
}
