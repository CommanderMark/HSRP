using System;
using System.Collections.Generic;
using System.Reflection;
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
            int atkX = attacker.DiceRolls;
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
                    else if (!attacker.TempMods.ContainsKey(1) || attacker.TempMods[1].Strength > -tarY)
                    {
                        int nonBuff = attacker.TempMods.ContainsKey(1)
                            ? attacker.TempMods[1].Strength
                            : 0;
                        int debuff = Math.Max(nonBuff - (tar - atk), -tarY);
                        attacker.TempMods.Remove(1);
                        ApplyTempMod(ref attacker, "strength", debuff, 1);
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
        // Random chance to do 3 things on success:
        // Roll 1DINT to debuff STR for 3 turns.
        // Roll 1DPER to debuff FOR for 1 turn.
        // Roll 1DINT to debuff INT for 1 turn.
        // If STR or FOR reach 0 they leave the strife.
        private void SpeechAttack(ref StrifePlayer attacker, ref NPC target)
        {
            log = Toolbox.GetRandomMessage("speechAttackStart", attacker.Name, target.Name) + "\n\n";

            // Player XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.Abilities.Intimidation + attacker.Abilities.Strength;

            // NPC XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Persuasion + target.Abilities.Fortitude;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

            // TODO;
            // Log messages.
            // Attack rolls higher, random chance begins.
            if (atk > tar)
            {
                switch (Toolbox.RandInt(2))
                {
                    // Roll 1DINT to debuff STR for 3 turns.
                    case 0:
                        {
                            int y = attacker.Abilities.Intimidation;
                            int debuff = -Toolbox.DiceRoll(1, y);
                            ApplyTempMod(ref target, "strength", debuff, 2);
                        } break;

                    // Roll 1DPER to debuff FOR for 1 turn.
                    case 1:
                        {
                            int y = attacker.Abilities.Persuasion;
                            int debuff = -Toolbox.DiceRoll(1, y);
                            ApplyTempMod(ref target, "strength", debuff, 0);
                        } break;
                }
            }
        }

        // Guard CON += XDCON
        private void Guard(ref StrifePlayer plyr)
        {
            log = $"{plyr.Name} is guarding.\n";

            AbilitySet mod = new AbilitySet();
            ApplyTempMod(ref plyr, "constitution", Toolbox.DiceRoll(1, plyr.Abilities.Constitution), 0);
        }

        private void ApplyTempMod(ref StrifePlayer ent, string stat, int value, int turns)
        {
            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (prop.Name.Contains(stat, StringComparison.OrdinalIgnoreCase))
                {
                    prop.SetValue(set, value);
                    ent.AddTempMod(set, turns);

                    string plural = turns == 1
                        ? "1 turn"
                        : (turns + 1).ToString() + " turns";
                    log = log.AddLine($"{ent.Name} was inflicted with {value} {prop.Name} for {plural}.");

                    return;
                }
            }
        }

        private void ApplyTempMod(ref NPC ent, string stat, int value, int turns)
        {
            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (prop.Name.Contains(stat, StringComparison.OrdinalIgnoreCase))
                {
                    prop.SetValue(set, value);
                    ent.AddTempMod(set, turns);

                    string plural = turns == 1
                        ? "1 turn"
                        : (turns + 1).ToString() + " turns";
                    log = log.AddLine($"{ent.Name} was inflicted with {value} {prop.Name} for {plural}.");

                    return;
                }
            }
        }
    }
}
