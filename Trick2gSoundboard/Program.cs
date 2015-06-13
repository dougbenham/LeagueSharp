using System;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using Trick2gSoundboard.Properties;

namespace Trick2gSoundboard
{
    class Program
    {
        /*
            beast - close to dying, no damage taken for x seconds and no enemy champions near
            bronzecalls - dying solo under turret
            datdamage, makinplays - deal 50%+ dmg of enemy in 2 seconds
            freelo - aced, given gates breached
            gatesopened - first inhib tower of the game taken
            getoffbaron - low ally near baron
            lookatthis - 
            payday - more than 1 cannon minion in same wave
            respectsurrender - enemy surrendered
            rip - % chance to play when hero dies
            towersdontaffect - tower dmg received, deals < 15% dmg. cooldown on this
            wtf2 - death but enemy champ is <10% hp
            wtfamiwatching
            yeahbitch - 3+ ally heroes (large radius check) and 1 enemy hero (smaller radius check)
         */
        private static DateTime lastPlay = DateTime.MinValue;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (DateTime.Now - lastPlay > TimeSpan.FromSeconds(10))
            {
                lastPlay = DateTime.Now;

                Console.WriteLine("played");
                new SoundPlayer(Resources.beast).Play();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
        }
    }
}
