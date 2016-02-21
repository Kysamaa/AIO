using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace AJS.Champions.LeeSin
{
    class Program
    {
        public static Spell.Skillshot QSKILL;

        public static void Execute(EventArgs args)
        {
            QSKILL = new Spell.Skillshot(SpellSlot.Q, 1080, SkillShotType.Linear, 500, 1900, 70);

            Game.OnTick += Tick;
        }

        private static void Tick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }       
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(QSKILL.Range, DamageType.Physical);
        }
    }
}
