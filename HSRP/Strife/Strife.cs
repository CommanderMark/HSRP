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

        public Strife()
        {
            Logs = new List<string>();
            Attackers = new List<IEntity>();
            Targets = new List<IEntity>();
        }

        // Player targeting NPC. Other way around is done in a function overload. (same for PvP if that's ever a thing)

        // Physical: XDSTR --> XDCON.
        // If XDSTR rolls higher than difference between both is the damage on the target.
        // Assuming they're not equal, then the target has a chance to counter attack if they rolls higher.
        // XDSTR <-- XDPER: Debuff of the difference between both roles is applied to the attacker's strength.
        private void PhysicalAttack(ref StrifePlayer attacker, ref NPC target)
        {
            log = $"{Syntax.ToCodeLine(attacker.Name)} attacks {Syntax.ToCodeLine(target.Name)}.\n\n";

            // Player XDY roll.
            int atkX = attacker.TotalDamage;
            int atkY = attacker.TotalAbilityStats.Strength;

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
                    // If the strength modifier is already debuffed by a number equal to or greater
                    // than the target's persuasion then don't debuff.
                    if (attacker.Modifiers.Strength <= (-tarY))
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
                        int nonBuff = attacker.Modifiers.Strength;
                        int debuff = Math.Max(nonBuff - (tar - atk), -tarY);
                        attacker.Modifiers.Strength = debuff;
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
        private void SpeechAttack(ref StrifePlayer attacker, ref NPC target)
        {
            log = Toolbox.GetRandomMessage("speechAttackStart", attacker.Name, target.Name) + "\n\n";

            // Player XDY roll.
            int atkX = attacker.TotalDamage;
            int atkY = attacker.TotalAbilityStats.Intimidation + attacker.TotalAbilityStats.Strength;

            // NPC XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Persuasion + target.Abilities.Fortitude;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

            // TODO: effects.
        }

        // Guard CON += XDCON
        private void Guard(ref StrifePlayer plyr)
        {
            log = $"{plyr.Name} is guarding.\n";

            AbilitySet mod = new AbilitySet();
            mod.Constitution = Toolbox.DiceRoll(1, plyr.Abilities.Constitution);
            plyr.AddTempMod(mod, 0);
        }
    }
}
