﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;

namespace HSRP
{
    // Could make this an abstract class for PvE + PvP. So saving methods are abstract but properties
    // and other stuff (oh GAWD the other stuff) is done here.
    public class Strife
    {
        public static List<int> ActiveStrifes = new List<int>();

        public int ID;
        public bool Active;

        /// <summary>
        /// Whether the Attackers team is taking their turn or not.
        /// </summary>
        private bool attackTurn;
        /// <summary>
        /// Who in the current team's (Attackers or Targets) turn it is. Starts at 0.
        /// </summary>
        private int turn;

        /// <summary>
        /// Log of events that have occurred in the strife so far.
        /// </summary>
        public List<string> Logs { get; set; }
        /// <summary>
        /// Log of current event.
        /// </summary>
        private string log;

        /// <summary>
        /// The enitites on the team that initiated the strife.
        /// </summary>
        public List<IEntity> Attackers { get; set; }
        public List<IEntity> Targets { get; set; }
        public IEnumerable<IEntity> Entities
        {
            get
            {
                return Attackers.Concat(Targets);
            }
        }

        /// <summary>
        /// Returns the entity who is currently ready for their turn.
        /// </summary>
        public IEntity CurrentEntity
        {
            get
            {
                if (attackTurn && Attackers.Count > turn)
                {
                    return Attackers[turn];
                }
                else if (!attackTurn && Targets.Count > turn)
                {
                    return Targets[turn];
                }

                throw new NullReferenceException($"Current entity is null. (TURN: {turn}, ATTACKTURN: {attackTurn.ToString()})");
            }
        }
        /// <summary>
        /// The user who is responsible for the next turn.
        /// </summary>
        public IEntity CurrentTurner { private set; get; }

        public bool Errored;

        public Strife()
        {
            Logs = new List<string>();
            Attackers = new List<IEntity>();
            Targets = new List<IEntity>();
        }

        public Strife(string filePath) : this()
        {
            string path = filePath.Contains(Dirs.Strifes)
                ? filePath + ".xml"
                : Path.Combine(Dirs.Strifes, filePath) + ".xml";

            XDocument doc = XmlToolbox.TryLoadXml(path);
            if (doc == null || doc.Root == null)
            {
                Console.WriteLine("[WHITE LINE] Strife " + path + " did not load!");
                Errored = true;
                return;
            }

            ID = XmlToolbox.GetAttributeInt(doc.Root, "id", 0);
            Active = XmlToolbox.GetAttributeBool(doc.Root, "active", false);

            foreach (XElement ele in doc.Root.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "status":
                        attackTurn = XmlToolbox.GetAttributeBool(ele, "attackTurn", true);
                        turn = XmlToolbox.GetAttributeInt(ele, "turn", 0);
                        break;

                    case "logs":
                        foreach (XElement logEle in ele.Elements())
                        {
                            string text = logEle.ElementInnerText();
                            Logs.Add(text);
                        }
                        break;

                    case "attackers":
                        foreach (XElement atkEle in ele.Elements())
                        {
                            switch (atkEle.Name.LocalName)
                            {
                                case "player":
                                    Player plyr = new Player(XmlToolbox.GetAttributeUnsignedLong(atkEle, "id", 0));
                                    if (!plyr.Errored)
                                    {
                                        Attackers.Add(plyr);
                                    }
                                    else
                                    {
                                        Console.WriteLine("[WHITE LINE] Attacker " + plyr.ID + " did not load!");
                                    }
                                    break;

                                case "npc":
                                    Attackers.Add(new NPC(atkEle));
                                    break;
                            }
                        }
                        break;

                    case "targets":
                        foreach (XElement atkEle in ele.Elements())
                        {
                            switch (atkEle.Name.LocalName)
                            {
                                case "player":
                                    Player plyr = new Player(XmlToolbox.GetAttributeUnsignedLong(atkEle, "id", 0));
                                    if (!plyr.Errored)
                                    {
                                        Targets.Add(plyr);
                                    }
                                    else
                                    {
                                        Console.WriteLine("[WHITE LINE] Target " + plyr.ID + " did not load!");
                                    }
                                    break;

                                case "npc":
                                    Targets.Add(new NPC(atkEle));
                                    break;
                            }
                        }
                        break;
                }
            }

            ulong id = XmlToolbox.GetAttributeUnsignedLong(doc.Root, "currentTurn", 0);
            foreach (IEntity ent in Entities)
            {
                if (id == ent.ID)
                {
                    CurrentTurner = ent;
                }
            }

            if (CurrentTurner == null)
            {
                CurrentTurner = CurrentEntity;
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument();
            XElement strife = new XElement("strife",
                new XAttribute("id", ID),
                new XAttribute("active", Active.ToString()),
                new XAttribute("currentTurn", CurrentTurner.ID)
                );

            XElement status = new XElement("status",
                new XAttribute("attackTurn", attackTurn),
                new XAttribute("turn", 0)
                );

            XElement logs = new XElement("logs");
            foreach (string log in Logs)
            {
                logs.Add(new XElement("log", new XText(log)));
            }

            XElement attackers = new XElement("attackers");
            foreach (IEntity ent in Attackers)
            {
                if (ent is Player plyr)
                {
                    plyr.Save();
                    attackers.Add(new XElement("player", new XAttribute("id", plyr.ID)));
                }
                else if (ent is NPC npc)
                {
                    attackers.Add(npc.Save());
                }
            }

            XElement targets = new XElement("targets");
            foreach (IEntity ent in Targets)
            {
                if (ent is Player plyr)
                {
                    plyr.Save();
                    targets.Add(new XElement("player", new XAttribute("id", plyr.ID)));
                }
                else if (ent is NPC npc)
                {
                    targets.Add(npc.Save());
                }
            }

            strife.Add(status, logs, attackers, targets);
            doc.Add(strife);

            XmlToolbox.WriteXml(this.ToXmlPath(), doc);

            XDocument config = new XDocument();
            XElement strifes = new XElement("strifes");
            foreach(int i in ActiveStrifes)
            {
                strifes.Add(new XElement("strife", new XAttribute("id", i)));
            }
            config.Add(strifes);
            XmlToolbox.WriteXml(ToConfigActiveStrifes(), config);
        }

        public string ToXmlPath() => Path.Combine(Dirs.Strifes, ID.ToString() + ".xml");
        public static string ToConfigActiveStrifes() => Path.Combine(Dirs.Config, "active_strifes.xml");

        /// <summary>
        /// Returns a string detailing the status of each entity on each team of the strife.
        /// </summary>
        /// <returns>A string detailing the status of each entity on each team of the strife.</returns>
        public string Display()
        {
            string txt = "Team A:\n";
            for (int i = 0; i < Attackers.Count; i++)
            {
                IEntity ent = Attackers[i];
                txt = txt.AddLine($"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}");
            }

            txt = txt.AddLine("\nTeam T: ");
            for (int i = 0; i < Targets.Count; i++)
            {
                IEntity ent = Targets[i];
                txt = txt.AddLine($"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}");
            }

            return txt;
        }

        /// <summary>
        /// Activates the strife. Notifies every player in the strife that they have been engaged.
        /// </summary>
        /// <returns>A string detailing the entities on each team of the strife.</returns>
        public async Task ActivateStrife()
        {
            Active = true;
            ActiveStrifes.Add(ID);

            turn = 0;
            attackTurn = true;
            CurrentTurner = CurrentEntity;

            log = Display();
            AddLog();

            // Notify any player involved.
            foreach (IEntity ent in Attackers)
            {
                if (ent is Player plyr)
                {
                    plyr.StrifeID = ID;
                    IGuildUser user =  await plyr.GuildUser;
                    await DiscordToolbox.DMUser(user, $"{plyr.Name} has engaged in a strife!");
                }
            }

            log = "\nTeam T: ";
            foreach (IEntity ent in Targets)
            {
                if (ent is Player plyr)
                {
                    plyr.StrifeID = ID;
                    IGuildUser user =  await plyr.GuildUser;
                    await DiscordToolbox.DMUser(user, $"{plyr.Name} has engaged in a strife!");
                }
            }
        }

        /// <summary>
        /// Updates the strife by checking if any AI-controlled characters need to take their turn.
        /// </summary>
        /// <param name="ntty">Returns the character whose turn is the next non-AI one.</param>
        /// <returns>A string containing the log of events that transpired when updating the strife.</returns>
        public string UpdateStrife(out Player ntty)
        {
            // Check who the next user is.
            IEntity turner = CurrentEntity;
            CurrentTurner = turner;
            string txt;

            // Are they being mind-controlled?
            if (turner.Controller > 0)
            {
                bool match = false;
                foreach (IEntity ent in Entities)
                {
                    if (turner.Controller == ent?.ID)
                    {
                        log = log.AddLine($"{turner.Name}, controlled by {ent.Name}, is taking their turn!");
                        CurrentTurner = ent;
                        match = true;
                        break;
                    }
                }
                // Their controller isn't in the strife anymore (hopefully).
                if (!match)
                {
                    log = log.AddLine($"{turner.Name} is no longer mind-controlled!");
                    turner.Controller = 0;
                }

                txt = log;
                AddLog();
                
                // AI is taking a turn, do that until a human is found.
                if (CurrentTurner is NPC)
                {
                    TakeAITurn();
                    txt += UpdateStrife(out Player ent);
                    CurrentTurner = ent;
                    AddLog();
                }
            }
            else
            {
                txt = log;
                AddLog();

                log = log.AddLine($"{turner.Name} is taking their turn!");
                // AI is taking a turn, do that until a human is found.
                if (turner is NPC)
                {
                    TakeAITurn();
                    txt += UpdateStrife(out Player ent);
                    CurrentTurner = ent;
                    AddLog();
                }
            }

            ntty = (Player)CurrentTurner;
            return txt;
        }

        /// <summary>
        /// Let's an entity take their turn. Also handles whether they die this turn or not.
        /// </summary>
        /// <param name="action">The type of action the attacker is taking.</param>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <returns>A string containing the log of events that transpired when taking this turn.</returns>
        public string TakeTurn(StrifeAction action, int targetNum, bool targetingAttackers)
        {
            IEntity attacker = CurrentEntity;
            IEntity target = targetingAttackers ? Attackers[targetNum] : Targets[targetNum];

            switch (action)
            {
                case StrifeAction.PhysicalAttack:
                    PhysicalAttack(ref attacker, ref target);
                    break;

                case StrifeAction.MindControl:
                    MindControl(ref attacker, ref target);
                    break;

                case StrifeAction.OpticBlast:
                    OpticBlast(ref attacker, ref target);
                    break;

                case StrifeAction.SpeechAttack:
                    SpeechAttack(ref attacker, ref target);
                    break;

                case StrifeAction.Guard:
                    Guard(ref attacker);
                    break;
                
                // TODO: Whoops.
                case StrifeAction.Abscond:
                    break;
            }

            if (attacker?.Health < 1)
            {
                LeaveStrife(ref attacker);
            }
            if (target?.Health < 1)
            {
                LeaveStrife(ref target);
            }

            // Update turn.
            if (attackTurn)
            {
                Attackers[turn] = attacker;
                if (targetingAttackers)
                {
                    Attackers[targetNum] = target;
                }
                else
                {
                    Targets[targetNum] = target;
                }

                // If the current index lands on a user that is no longer in the strife continue incrementing.
                do
                {
                    // Rotate turn.
                    ++turn;
                    if (turn >= Attackers.Count)
                    {
                        // Reached the end of the attackers list.
                        attackTurn = false;
                        turn = 0;

                        log = log.AddLine("Targets are now taking their turns.");
                    }
                } while (Attackers[turn] == null && attackTurn);
            }
            else
            {
                Targets[turn] = attacker;
                if (targetingAttackers)
                {
                    Attackers[targetNum] = target;
                }
                else
                {
                    Targets[targetNum] = target;
                }

                // If the current index lands on a user that is no longer in the strife continue incrementing.
                do
                {
                    // Rotate turn.
                    ++turn;
                    if (turn >= Attackers.Count)
                    {
                        // Reached the end of the attackers list.
                        attackTurn = true;
                        turn = 0;

                        log = log.AddLine("Targets are now taking their turns.");
                    }
                } while (Attackers[turn] == null && !attackTurn);
            }

            return log;
        }

        /// <summary>
        /// Let's an AI take their turn. Also handles whether they die this turn or not.
        /// Note that no string is returned here as that should be handled by the UpdateStrife() function.
        /// </summary>
        private void TakeAITurn()
        {
            IEntity ai = CurrentEntity;
            if (ai == null)
            {
                throw new NullReferenceException($"Current entity is null. (TURN: {turn}, ATTACKTURN: {attackTurn.ToString()})");
            }
            int targetID = attackTurn
                ? Toolbox.RandInt(Targets.Count)
                : Toolbox.RandInt(Attackers.Count);

            // TODO: More AI-y stuff.
            if (ai is NPC npc)
            {
                // TODO: Guard when some arbitrary event occurs?
                switch (npc.Type)
                {
                    case NPCType.Lusus:
                    case NPCType.Normal:
                        TakeTurn(StrifeAction.PhysicalAttack, targetID, !attackTurn);
                        break;

                    case NPCType.Psionic:
                        TakeTurn(StrifeAction.OpticBlast, targetID, !attackTurn);
                        break;
                }
            }
        }

        private void LeaveStrife(ref IEntity ent)
        {
            if (ent is Player plyr)
            {
                plyr.StrifeID = 0;
                plyr.Save();
            }
            ent = null;
            log = log.AddLine(ent.Name + " is no longer in the strife.");
        }

        // Physical: XDSTR --> XDCON.
        // If XDSTR rolls higher than difference between both is the damage on the target.
        // Assuming they're not equal, then the target has a chance to counter attack if they rolls higher.
        // XDSTR <-- XDPER: Debuff of the difference between both roles is applied to the attacker's strength.
        private void PhysicalAttack(ref IEntity attacker, ref IEntity target)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} attacks {Syntax.ToCodeLine(target.Name)}.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.Abilities.Strength;

            // Target XDY roll.
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

        // Mental: XDPSI --> XDFOR 3 times in a row.
        // TODO: Mind control can break somehow?
        private void MindControl(ref IEntity attacker, ref IEntity target)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} attempts to mind control {Syntax.ToCodeLine(target.Name)}.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.Abilities.Psion;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Fortitude;

            // Dice rolls.
            int[] atk = new int[3];
            int[] tar = new int[3];

            for (int i = 0; i < 3; i++)
            {
                atk[i] = Toolbox.DiceRoll(atkX, atkY);
                tar[i] = Toolbox.DiceRoll(tarX, tarY);
            }

            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk[0]}, {atk[1]}, {atk[2]}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar[0]}, {tar[1]}, {tar[2]}!");

            int atkTotal = atk[0] + atk[1] + atk[2];
            int tarTotal = tar[0] + tar[1] + tar[2];
            // Mind control wooo.
            if (atkTotal > tarTotal)
            {
                target.Controller = attacker.ID;
                log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} has successfully mind controlled {Syntax.ToCodeLine(target.Name)}.");
            }
            else
            {
                log = log.AddLine("Mind control failed.");
            }
        }
        private void OpticBlast(ref IEntity attacker, ref IEntity target)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} is preparing an Optic Blast.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.Abilities.Psion;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Fortitude;

            // Dice rolls.
            int[] atk = new int[3];
            int[] tar = new int[3];

            for (int i = 0; i < 3; i++)
            {
                atk[i] = Toolbox.DiceRoll(atkX, atkY);
                tar[i] = Toolbox.DiceRoll(tarX, tarY);
            }

            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk[0]}, {atk[1]}, {atk[2]}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar[0]}, {tar[1]}, {tar[2]}!");

            int atkTotal = atk[0] + atk[1] + atk[2];
            int tarTotal = tar[0] + tar[1] + tar[2];
            // Pew pew pew wooo.
            if (atkTotal > tarTotal)
            {
                int dmg = atkTotal - tarTotal;
                target.Health -= dmg;
                log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} struck {Syntax.ToCodeLine(target.Name)} for {dmg} hitpoints.");
            }
            else
            {
                log = log.AddLine("Optic Blast missed.");
            }
        }

        // Speech: XD(INT+STR) --> XD(PER+FOR)
        // Random chance to do 3 things on success:
        // Roll 1DINT to debuff STR for 3 turns.
        // Roll 1DPER to debuff FOR for 1 turn.
        // Roll 1DINT to debuff INT for 1 turn.
        // If STR or FOR reach 0 they leave the strife.
        private void SpeechAttack(ref IEntity attacker, ref IEntity target)
        {
            log = log.AddLine(Toolbox.GetRandomMessage("speechAttackStart", attacker.Name, target.Name) + "\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.Abilities.Intimidation + attacker.Abilities.Strength;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.Abilities.Persuasion + target.Abilities.Fortitude;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

            // Log messages.
            // Attack rolls higher, random chance begins.
            if (atk > tar)
            {
                int rng = Toolbox.RandInt(2, true);
                switch (rng)
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
                            ApplyTempMod(ref target, "fortitude", debuff, 0);
                        } break;
                    
                    // Roll 1DINT to debuff INT for 1 turn.
                    case 2:
                        {
                            int y = attacker.Abilities.Intimidation;
                            int debuff = -Toolbox.DiceRoll(1, y);
                            ApplyTempMod(ref target, "intimidation", debuff, 0);
                        } break;
                }

                // If STR or FOR reach 0 they leave the strife.
                if (target.Abilities.Strength < 1 && rng == 0)
                {
                    log = log.AddLine($"{target.Name.ToApostrophe()} strength has fallen below 1.");
                    log = log.AddLine(Toolbox.GetRandomMessage("speechKill", target.Name));

                    LeaveStrife(ref target);
                }
                else if (target.Abilities.Fortitude < 1 && rng == 1)
                {
                    log = log.AddLine($"{target.Name.ToApostrophe()} fortitude has fallen below 1.");
                    log = log.AddLine(Toolbox.GetRandomMessage("speechKill", target.Name));

                    LeaveStrife(ref target);
                }
            }
        }

        // Guard CON += XDCON
        private void Guard(ref IEntity plyr)
        {
            log = log.AddLine($"{plyr.Name} is guarding.");

            AbilitySet mod = new AbilitySet();
            ApplyTempMod(ref plyr, "constitution", Toolbox.DiceRoll(1, plyr.Abilities.Constitution), 0);
        }

        private string AddLog()
        {
            log = log.AddLine("\n------------\n");
            string str = log;

            Logs.Add(log);
            log = "";

            // Return the original so it can be used for reply messages.
            return str;
        }

        private void ApplyTempMod(ref IEntity ent, string stat, int value, int turns)
        {
            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (prop.Name.Contains(stat, StringComparison.OrdinalIgnoreCase))
                {
                    prop.SetValue(set, value);
                    ent.AddTempMod(set, turns);

                    string plural = turns == 0
                        ? "1 turn"
                        : (turns + 1).ToString() + " turns";
                    log = log.AddLine($"{ent.Name} was inflicted with {value} {prop.Name} for {plural}.");

                    return;
                }
            }
        }
    }
}
