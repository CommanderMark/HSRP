using System;
using System.Collections.Generic;
using System.Text;

namespace HSRP
{
    // Could make this an abstract class for PvE + PvP. So saving methods are abstract but properties
    // and other stuff (oh GAWD the other stuff) is done here.
    public class Strife
    {
        public static List<Strife> Strifes { get; set; }

        /// <summary>
        /// Whether the Attackers team is taking their turn or not.
        /// </summary>
        public bool AttackTurn { get; set; }
        /// <summary>
        /// Who in the current team's (Attackers or Targets) turn it is. Starts at 0.
        /// </summary>
        public int Turn { get; set; }

        /// <summary>
        /// Log of events that have occurred in the strife so far.
        /// </summary>
        public List<string> Logs { get; set; }
        /// <summary>
        /// Log of current event.
        /// </summary>
        private string log;

        /// <summary>
        /// The IDs of every attacker's character.
        /// </summary>
        public List<IEntity> Attackers { get; set; }
        public List<IEntity> Targets { get; set; }

        private Dictionary<ulong, AbilitySet> Modifiers { get; set; }

        public Strife()
        {
            Logs = new List<string>();
            Attackers = new List<IEntity>();
            Targets = new List<IEntity>();
            Modifiers = new Dictionary<ulong, AbilitySet>();
        }

        // Player --> NPC. Other way around is done in a function overload. (same for PvP if that's ever a thing)
        // Physical: XDSTR --> XDCON. Counter: XDSTR <-- XDPER: Debuff
        private void PhysicalAttack(ref Player attacker, ref NPC target)
        {
            log = $"{Syntax.ToCodeLine(attacker.Name)} attacks {Syntax.ToCodeLine(attacker.Name)}.\n\n";

            // Player XDY roll.
            int atkX = attacker.TotalDamage;
            int atkY = attacker.Abilities.Strength;

            // NPC XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Constitution;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

            // If attacker rolled higher inflict damage.
            if (atk > tar)
            {
                int dmg = atk - tar;
                target.Health -= dmg;
                log = log.AddLine($"\n{Syntax.ToCodeLine(target.Name)} took {dmg} hitpoints of damage.");
            }
            // If target rolled higher begin counter attack.
            else if (atk < tar)
            {
                log = log.AddLine($"\n{Syntax.ToCodeLine(target.Name)} is counter attacking.");
                // 50% chance to counter.
                if (Toolbox.RandInt(2) == 1)
                {
                    tarY = target.Abilities.Persuasion;
                    if ((-tarY) >= Modifiers[attacker.ID].Strength)
                    {
                        log = log.AddLine($"Maximum amount of counter attacks reached.");
                        return;
                    }

                    atk = Toolbox.DiceRoll(atkX, atkY);
                    tar = Toolbox.DiceRoll(tarX, tarY);
                    log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
                    log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

                    // Counter failed.
                    if (atk >= tar)
                    {
                        log = log.AddLine("\nCounter attack blocked.");
                    }
                    // Counter suceeded, debuff strength.
                    else
                    {
                        int nonBuff = Modifiers[attacker.ID].Strength;
                        int debuff = Math.Max(nonBuff - (tar - atk), -tarY);
                        Modifiers[attacker.ID].Strength = debuff;
                        log = log.AddLine($"\nStrenth debuff of {debuff - nonBuff} inflicted on {Syntax.ToCodeLine(attacker.Name)}.");
                    }
                }
                else
                {
                    log = log.AddLine($"...");
                    log = log.AddLine(Toolbox.GetRandomMessage("failedNPCCounter"));
                }
            }
            // Equal rolls, do nothing.
            else
            {
                log = log.AddLine("\nNothing happened.");
            }
        }

        // Mental: XDPSI --> XDFOR
        // Speech: XD(INT+STR) --> XD(PER+FOR)
        // Guards CON += XDCON
    }
}
