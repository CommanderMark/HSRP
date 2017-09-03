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
    public class Strife
    {
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
        private int postedLogs;

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

                return null;
            }
        }
        /// <summary>
        /// The user who is responsible for the next turn.
        /// </summary>
        private IEntity CurrentTurner;

        /// <summary>
        /// Gets the entity being targeted based on the inputted values.
        /// </summary>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <returns>The target entity.</returns>
        public IEntity GetTarget(int targetNum, bool targetingAttackers)
        {
            if (targetingAttackers)
            {
                return targetNum >= Attackers.Count ? null : Attackers[targetNum];
            }

            return targetNum >= Targets.Count ? null : Targets[targetNum];
        }

        public bool Errored;

        private Strife()
        {
            Logs = new List<string>();
            Attackers = new List<IEntity>();
            Targets = new List<IEntity>();
        }

        public Strife(string filePath, bool idOnly = true) : this()
        {
            string path = idOnly
                ? Path.Combine(Dirs.Strifes, filePath) + ".xml"
                : filePath;

            XDocument doc = XmlToolbox.TryLoadXml(path);
            if (doc == null || doc.Root == null)
            {
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

                    case "logs":
                        postedLogs = XmlToolbox.GetAttributeInt(ele, "posted", 0);
                        foreach (XElement logEle in ele.Elements())
                        {
                            string text = logEle.ElementInnerText();
                            Logs.Add(text);
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
                new XAttribute("active", Active),
                new XAttribute("currentTurn", CurrentTurner == null ? 0 : CurrentTurner.ID)
                );

            XElement status = new XElement("status",
                new XAttribute("attackTurn", attackTurn),
                new XAttribute("turn", turn)
                );

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
            
            XElement logs = new XElement("logs", new XAttribute("posted", postedLogs));
            foreach (string log in Logs)
            {
                logs.Add(new XElement("log", new XText(log)));
            }

            strife.Add(status, attackers, targets, logs);
            doc.Add(strife);

            XmlToolbox.WriteXml(this.ToXmlPath(), doc);
        }

        public string ToXmlPath() => Path.Combine(Dirs.Strifes, ID.ToString() + ".xml");

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
                string usr = $"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}";
                if (ent.Dead)
                {
                    usr += " (DEFEATED)";
                }
                txt = txt.AddLine(usr);
            }

            txt = txt.AddLine("\nTeam T: ");
            for (int i = 0; i < Targets.Count; i++)
            {
                IEntity ent = Targets[i];
                string usr = $"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}";
                if (ent.Dead)
                {
                    usr += " (DEFEATED)";
                }
                txt = txt.AddLine(usr);
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

            turn = 0;
            attackTurn = true;
            CurrentTurner = CurrentEntity;

            // Add list of users to log but skip it when posting the logs as that's posted separately.
            Logs.Clear();
            log = Display();
            AddLog();
            postedLogs = 1;

            // Notify any player involved.
            foreach (IEntity ent in Entities)
            {
                if (ent is Player plyr)
                {
                    plyr.StrifeID = ID;
                    IGuildUser user =  await plyr.GuildUser;
                    await DiscordToolbox.DMUser(user, $"{Syntax.ToCodeLine(plyr.Name)} has engaged in a strife!");
                }
            }
        }

        public string DeactivateStrife()
        {
            EndStrife();
            return GetLogs();
        }

        /// <summary>
        /// Updates the strife by checking if any AI-controlled characters need to take their turn.
        /// </summary>
        /// <param name="ntty">Returns the character whose turn is the next non-AI one.</param>
        /// <returns>A string containing the log of events that transpired when updating the strife.</returns>
        public string UpdateStrife(out Player ntty, bool returnEmp = false)
        {
            // Check who the next user is.
            IEntity turner = CurrentEntity;
            CurrentTurner = turner;

            // Is the strife still going on?
            if (!Active)
            {
                ntty = null;
                return returnEmp ? string.Empty : GetLogs();
            }

            // Are all the members of one team dead?
            if (Attackers.All(x => x.Dead || (x.Controller > 0 && Targets.Any(y => y.ID == x.Controller))))
            {
                log = log.AddLine("Attackers have been defeated.");
                EndStrife();

                ntty = null;
                return returnEmp ? string.Empty : GetLogs();
            }
            if (Targets.All(x => x.Dead || (x.Controller > 0 && Attackers.Any(y => y.ID == x.Controller))))
            {
                log = log.AddLine("Targets have been defeated.");
                EndStrife();

                ntty = null;
                return returnEmp ? string.Empty : GetLogs();
            }

            // Update turns. Sorry you have to see this.
            if (attackTurn ? Attackers[turn].Dead : Targets[turn].Dead)
            {
                UpdateTurn();
            }

            // Are they being mind-controlled?
            if (turner.Controller > 0)
            {
                bool match = false;
                foreach (IEntity ent in Entities)
                {
                    if (turner.Controller == ent.ID && !ent.Dead && ent.Controller <= 0) 
                    {
                        log = log.AddLine($"{Syntax.ToCodeLine(turner.Name)}, controlled by {Syntax.ToCodeLine(ent.Name)}, is taking their turn!");
                        CurrentTurner = ent;
                        match = true;
                        break;
                    }
                }
                // Their controller is either dead or being mind-controlled themselves.
                if (!match)
                {
                    log = log.AddLine($"{Syntax.ToCodeLine(turner.Name)} is no longer controlled!");
                    turner.Controller = 0;
                }

                AddLog();
                
                // AI is taking a turn, do that until a human is found.
                if (CurrentTurner is NPC)
                {
                    TakeAITurn();
                    UpdateStrife(out Player ent, true);
                    CurrentTurner = ent;

                    AddLog();
                }
            }
            // Not mind-controlled.
            else
            {
                AddLog();

                log = log.AddLine($"{Syntax.ToCodeLine(turner.Name)} is taking their turn!");
                // AI is taking a turn, do that until a human is found.
                if (turner is NPC)
                {
                    TakeAITurn();
                    UpdateStrife(out Player ent, true);
                    CurrentTurner = ent;
                }

                AddLog();
            }

            ntty = (Player)CurrentTurner;

            return returnEmp ? string.Empty : GetLogs();
        }

        private void UpdateTurn()
        {
            bool notValidTurner = false;
            do
            {
                // Rotate turn.
                ++turn;
                if (attackTurn)
                {
                    if (turn >= Attackers.Count)
                    {
                        // Reached the end of the attackers list.
                        attackTurn = false;
                        turn = 0;
                        AddLog();

                        log = log.AddLine("Targets are now taking their turns.");
                        notValidTurner = Targets[turn].Dead;
                    }
                    else
                    {
                        notValidTurner = Attackers[turn].Dead;
                    }
                }
                else
                {
                    if (turn >= Targets.Count)
                    {
                        // Reached the end of the attackers list.
                        attackTurn = true;
                        turn = 0;
                        AddLog();

                        log = log.AddLine("Attackers are now taking their turns.");
                        notValidTurner = Attackers[turn].Dead;
                    }
                    else
                    {
                        notValidTurner = Targets[turn].Dead;
                    }
                }
            }
            // If the current index lands on a user that is no longer in the strife continue incrementing.
            while (notValidTurner);

            AddLog();
        }

        /// <summary>
        /// Validates a strife turn against a series of arbitrary parameters based on the action and state of the current turner.
        /// </summary>
        /// <param name="action">The action that is being performed this turn.</param>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <param name="reason">The reason for a turn being rejected if done so.</param>
        /// <returns>Whether or not the action is valid.</returns>
        public bool ValidateTurn(StrifeAction action, int targetNum, bool targetingAttackers, Player plyr, out string reason)
        {
            IEntity attacker = CurrentEntity;
            IEntity target = GetTarget(targetNum, targetingAttackers);

            NPC npcAtk = attacker as NPC;
            NPC npcTar = target as NPC;

            // Is it their turn?
            if (CurrentTurner.ID != plyr.ID)
            {
                reason = $"It is {Syntax.ToCodeLine(CurrentTurner.Name.ToApostrophe())} turn.";
                return false;
            }

            // Are they targeting someone who exist?
            if (target == null)
            {
                reason = $"Invalid target entity. Use {Syntax.ToCodeLine(Constants.BotPrefix + "strife check")} to see the indexes of each strifer.";
                return false;
            }

            // Are they targeting someone who is dead?
            if (target.Dead)
            {
                reason = "Invalid target. The targeted user is no longer in the strife.";
                return false;
            }

            // Are they targeting themselves?
            if (attacker.ID == target.ID)
            {
                reason = "Invalid target. You cannot target yourself.";
                return false;
            }

            // Are they trying to mind-control while mind-controlling?
            if (attacker.Controller > 0 && action == StrifeAction.MindControl)
            {
                reason = "Invalid attack. Cannot mind-control while controlling someone else.";
                return false;
            }

            // Are they trying to use a lusus to perform psionic attacks?
            if (npcAtk != null && npcAtk.Type == NPCType.Lusus
                && !(action == StrifeAction.PhysicalAttack || action == StrifeAction.Guard)
                )
            {
                reason = "Invalid attack. A lusus cannot perform psionic or speech attacks.";
                return false;
            }

            // Are they trying to intimidate a lusus?
            if (npcTar != null && npcTar.Type == NPCType.Lusus && action == StrifeAction.SpeechAttack)
            {
                reason = "Invalid attack. Cannot indimidate lusus.";
                return false;
            }

            reason = "Strife action valid.";
            return true;
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
            IEntity target = GetTarget(targetNum, targetingAttackers);
            // If it's an NPC then don't update the logs. Unless they're mind-controlled.
            bool returnEmp = CurrentTurner is NPC ? true : false;

            switch (action)
            {
                case StrifeAction.PhysicalAttack:
                    PhysicalAttack(attacker, target);
                    break;

                case StrifeAction.MindControl:
                    MindControl(attacker, target);
                    break;

                case StrifeAction.OpticBlast:
                    OpticBlast(attacker, target);
                    break;

                case StrifeAction.SpeechAttack:
                    SpeechAttack(attacker, target);
                    break;

                case StrifeAction.Guard:
                    Guard(attacker);
                    break;
            }

            attacker.UpdateTempMods();

            if (attacker.Health < 1 && !attacker.Dead)
            {
                LeaveStrife(attacker);
            }
            if (target.Health < 1 && !target.Dead)
            {
                LeaveStrife(target);
            }

            // Update lists.
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
            }

            UpdateTurn();
            return returnEmp ? string.Empty : GetLogs();
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
            int targetID = 0;

            IEntity target = null;

            while (target == null || target.Dead)
            {
                targetID = attackTurn
                    ? Toolbox.RandInt(Targets.Count)
                    : Toolbox.RandInt(Attackers.Count);

                target = GetTarget(targetID, !attackTurn);
            }

            // TODO: More AI-y stuff.
            if (ai is NPC npc)
            {
                switch (npc.Type)
                {
                    case NPCType.Lusus:
                    case NPCType.Normal:
                        TakeTurn(StrifeAction.PhysicalAttack, targetID, !attackTurn);
                        break;

                    case NPCType.Psionic:
                        int rng = Toolbox.RandInt(4);
                        // 25% chance to mind-control.
                        if (rng == 3 && target.Controller != ai.ID)
                        {
                            TakeTurn(StrifeAction.MindControl, targetID, !attackTurn);
                        }
                        // 25% chance to attack.
                        else if (rng == 2)
                        {
                            TakeTurn(StrifeAction.PhysicalAttack, targetID, !attackTurn);
                        }
                        // 25% chance to optic blast.
                        else
                        {
                            TakeTurn(StrifeAction.OpticBlast, targetID, !attackTurn);
                        }
                        break;
                    
                    case NPCType.Talker:
                        if (Toolbox.TrueOrFalse(4))
                        {
                            TakeTurn(StrifeAction.SpeechAttack, targetID, !attackTurn);
                        }
                        else
                        {
                            TakeTurn(StrifeAction.Guard, targetID, !attackTurn);
                        }
                        break;
                }
            }
        }

        private void LeaveStrife(IEntity ent)
        {
            ent.Dead = true;
            if (ent.Health < 0)
            {
                ent.Health = 0;
            }
            log = log.AddLine(Syntax.ToCodeLine(ent.Name) + " is no longer participating in the strife.");
        }

        public string Forfeit(ulong id)
        {
            IEntity ent = Entities.FirstOrDefault(x => x.ID == id);
            if (ent == null) { return null; }

            bool attacker = Attackers.Contains(ent);
            int index = attacker
                ? Attackers.IndexOf(ent)
                : Targets.IndexOf(ent);

            // Kill 'em.
            LeaveStrife(ent);
            AddLog();
            return GetLogs();
        }

        private void EndStrife()
        {
            Active = false;
            AddLog();
            log = log.AddLine("The strife is now over. XP will be awarded at the discretion of the GM.");
            AddLog();
            log = Display();
            AddLog();

            foreach (IEntity ent in Entities)
            {
                if (!(ent is NPC npc && npc.Type == NPCType.Lusus))
                {
                    ent.Controller = 0;
                }

                // If an entity is dead reset their HP to 1. 
                if (ent.Dead)
                {
                    ent.Dead = false;
                    if (ent.Health < 1) { ent.Health = 1; }
                }

                if (ent is Player plyr)
                {
                    plyr.StrifeID = 0;
                }
            }
        }

        // Physical: XDSTR --> XDCON.
        // If XDSTR rolls higher than difference between both is the damage on the target.
        // Then the target has a chance to counter attack if they roll higher.
        // XDSTR <-- XDPER: Debuff of the difference between both rolls is applied to the attacker's strength.
        private void PhysicalAttack(IEntity attacker, IEntity target)
        {
            log = log.AddLine(Toolbox.GetMessage("phyStart", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)));
            log = log.AddLine("");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.TotalAbilities.Strength;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.TotalAbilities.Constitution;

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

                log = log.AddLine("");
                log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} took {dmg} hitpoint(s) of damage.");
            }
            else if (atk < tar)
            {
                log = log.AddLine("");
                log = log.AddLine("Attack missed.");
            }
            // Equal rolls, do nothing.
            else
            {
                log = log.AddLine("");
                log = log.AddLine("Nothing happened.");
            }

            // Counter attack.
            // If the strength modifier of the attacker is already debuffed below 1 then don't debuff.
            if (attacker.TotalAbilities.Strength < 1)
            {
                log = log.AddLine("");
                log = log.AddLine(Toolbox.GetMessage("phyCounterMax", Syntax.ToCodeLine(attacker.Name)));
                return;
            }
            else
            {
                tarY = target.TotalAbilities.Persuasion;

                // 25% chance to counter. Increased by an addition 25 if the target's persuasion
                // is higher than the attacker's strength.
                if (Toolbox.TrueOrFalse(4 - (2 * Convert.ToInt32(tarY > atkY))))
                {
                    log = log.AddLine("");
                    log = log.AddLine(Toolbox.GetMessage("phyCounterStart", Syntax.ToCodeLine(target.Name), Syntax.ToCodeLine(attacker.Name)));

                    atk = Toolbox.DiceRoll(atkX, atkY);
                    tar = Toolbox.DiceRoll(tarX, tarY);
                    log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
                    log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");

                    // Counter failed.
                    if (atk >= tar)
                    {
                        log = log.AddLine("");
                        log = log.AddLine(Toolbox.GetMessage("phyCounterBlock"));
                    }
                    // Counter suceeded, debuff strength.
                    else
                    {
                        int nonBuff = attacker.TempMods.ContainsKey(1)
                            ? attacker.TempMods[1].Strength
                            : 0;
                        int debuff = Math.Max(nonBuff - (tar - atk), -tarY);
                        attacker.TempMods.Remove(1);
                        ApplyTempMod(attacker, "strength", debuff, 1);
                    }
                }
            }
        }

        // Mental: XDPSI --> XDFOR 3 times in a row.
        // TODO: Mind control can break somehow?
        private void MindControl(IEntity attacker, IEntity target)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} attempts to mind control {Syntax.ToCodeLine(target.Name)}.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.TotalAbilities.Psion;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.TotalAbilities.Fortitude;

            // Dice rolls.
            int[] atk = new int[3];
            int[] tar = new int[3];

            bool success = true;
            for (int i = 0; i < 3; i++)
            {
                atk[i] = Toolbox.DiceRoll(atkX, atkY);
                tar[i] = Toolbox.DiceRoll(tarX, tarY);
                if (tar[i] >= atk[i]) { success = false; }
            }

            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk[0]}, {atk[1]}, {atk[2]}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar[0]}, {tar[1]}, {tar[2]}!");

            int atkTotal = atk[0] + atk[1] + atk[2];
            int tarTotal = tar[0] + tar[1] + tar[2];
            // Mind control wooo.
            if (success)
            {
                target.Controller = attacker.ID;
                log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} has successfully mind controlled {Syntax.ToCodeLine(target.Name)}.");
            }
            else
            {
                log = log.AddLine("Mind control failed.");
            }
        }
        private void OpticBlast(IEntity attacker, IEntity target)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} is preparing an Optic Blast.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.TotalAbilities.Psion;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.TotalAbilities.Fortitude;

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
        private void SpeechAttack(IEntity attacker, IEntity target)
        {
            log = log.AddLine(Toolbox.GetMessage("speStart", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)) + "\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.TotalAbilities.Intimidation + attacker.TotalAbilities.Strength;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.TotalAbilities.Persuasion + target.TotalAbilities.Fortitude;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            log = log.AddLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {atk}!");
            log = log.AddLine($"{Syntax.ToCodeLine(target.Name)} rolls {tar}!");
            
            // Attack rolls higher, random chance begins.
            if (atk > tar)
            {
                int rng = Toolbox.RandInt(2, true);
                int debuff = 0;
                string stat = "";
                switch (rng)
                {
                    // Roll 1DINT to debuff STR for 3 turns.
                    case 0:
                        {
                            int y = attacker.TotalAbilities.Intimidation;
                            debuff = Toolbox.DiceRoll(1, y);
                            debuff = Math.Min(target.Abilities.Strength / 3, debuff);
                            stat = "strength";
                        } break;

                    // Roll 1DPER to debuff FOR for 1 turn.
                    case 1:
                        {
                            int y = attacker.TotalAbilities.Persuasion;
                            debuff = Toolbox.DiceRoll(1, y);
                            debuff = Math.Min(target.Abilities.Fortitude / 3, debuff);
                            stat = "fortitude";
                        } break;
                    
                    // Roll 1DINT to debuff INT for 1 turn.
                    case 2:
                        {
                            int y = attacker.TotalAbilities.Intimidation;
                            debuff = Toolbox.DiceRoll(1, y);
                            stat = "intimidation";
                        } break;
                }

                ApplyTempMod(target, stat, -debuff, -1);

                // If STR or FOR reach 0 they leave the strife.
                if ((target.TotalAbilities.Strength < 1 && rng == 0) || (target.TotalAbilities.Fortitude < 1 && rng == 1))
                {
                    log = log.AddLine($"{Syntax.ToCodeLine(target.Name.ToApostrophe())} {stat} has fallen below 1.");

                    // 50% chance for them to leave the strife.
                    if (Toolbox.TrueOrFalse())
                    {
                        log = log.AddLine(Toolbox.GetMessage("speKill", Syntax.ToCodeLine(target.Name), Syntax.ToCodeLine(attacker.Name)));

                        LeaveStrife(target);
                    }
                    // Otherwise their debuffs are removed.
                    else
                    {
                        log = log.AddLine(Toolbox.GetMessage("speKillFail", Syntax.ToCodeLine(target.Name), Syntax.ToCodeLine(attacker.Name)));
                        log = log.AddLine(Syntax.ToCodeLine(target.Name.ToApostrophe()) + " debuffs were removed.");
                        target.Modifiers = new AbilitySet();
                    }
                }
            }
            else
            {
                log = log.AddLine("");
                log = log.AddLine(Toolbox.GetMessage("speFail", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)));
            }
        }

        // Guard CON += XDCON
        private void Guard(IEntity plyr)
        {
            log = log.AddLine($"{Syntax.ToCodeLine(plyr.Name)} is guarding.");

            AbilitySet mod = new AbilitySet();
            ApplyTempMod(plyr, "constitution", Toolbox.DiceRoll(1, plyr.Abilities.Constitution), 0);
        }

        private void AddLog()
        {
            if (string.IsNullOrWhiteSpace(log)) { return; }

            log = log.AddLine("\n------------");
            string str = log;

            Logs.Add(log);
            log = "";
        }

        /// <summary>
        /// Returns the logs which have not been posted yet. Updates the counter to indicate that they are now posted.
        /// </summary>
        private string GetLogs()
        {
            string wha = "";

            for (int i = postedLogs; i < Logs.Count; i++)
            {
                wha = wha.AddLine(Logs[i]);
            }
            postedLogs = Logs.Count;

            return wha;
        }

        /// <summary>
        /// Writes every log into a file, the path of which is "strifes/{id}.txt".
        /// </summary>
        /// <returns>The path to the log file.</returns>
        public string LogLogs()
        {
            string txt = string.Join("\n", Logs);
            string path = Path.Combine(Dirs.Config, $"{Dirs.StrifeLogs}{ID}.txt");
            File.WriteAllText(path, txt);

            return path;
        }

        /// <summary>
        /// Writes every log into a file, the path of which is "strifes/{id}.txt".
        /// Clears all of the current logs at the same time.
        /// </summary>
        /// <returns>The path to the log file.</returns>
        public string ClearAndLogLogs()
        {
            string path = LogLogs();

            Logs.Clear();
            postedLogs = 0;

            return path;
        }

        private void ApplyTempMod(IEntity ent, string stat, int value, int turns)
        {
            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (prop.Name.Contains(stat, StringComparison.OrdinalIgnoreCase))
                {
                    prop.SetValue(set, value);
                    // Temporary modifier.
                    if (turns >= 0)
                    {
                        ent.AddTempMod(set, turns);

                        string plural = turns == 0
                            ? "1 turn"
                            : (turns + 1).ToString() + " turns";
                        log = log.AddLine($"\n{Syntax.ToCodeLine(ent.Name)} was inflicted with {Syntax.ToCodeLine(value.ToString("+0;-#"))} {prop.Name} for {Syntax.ToCodeLine(plural)}.");
                    }
                    // Permanent modifier.
                    else
                    {
                        ent.Modifiers += set;
                        log = log.AddLine($"\n{Syntax.ToCodeLine(ent.Name)} was inflicted with {Syntax.ToCodeLine(value.ToString("+0;-#"))} {prop.Name} for the remainder of the strife.");
                    }

                    return;
                }
            }
        }

        public static bool TryCreateStrife(int id, out Strife strife)
        {
            Strife strf = new Strife();

            if (id < 1)
            {
                // Generate ID.
                do
                {
                    strf.ID = Toolbox.RandInt(Int16.MaxValue);
                }
                while (File.Exists(Path.Combine(Dirs.Strifes, strf.ID + ".xml")));
            }
            else
            {
                if (File.Exists(Path.Combine(Dirs.Strifes, id + ".xml")))
                {
                    strife = null;
                    return false;
                }

                strf.ID = id;
            }

            strife = strf;
            return true;
        }
    }
}
