﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public StringBuilder Log = new StringBuilder();
        private int postedLogs;

        /// <summary>
        /// The enitites on the team that initiated the strife.
        /// </summary>
        public List<Entity> Attackers { get; set; }
        public List<Entity> Targets { get; set; }
        public IEnumerable<Entity> Entities
        {
            get
            {
                return Attackers.Concat(Targets);
            }
        }

        /// <summary>
        /// Returns the entity who is currently ready for their turn.
        /// </summary>
        public Entity CurrentEntity
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
        private Entity CurrentTurner;

        /// <summary>
        /// Gets the entity being targeted based on the inputted values.
        /// </summary>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <returns>The target entity.</returns>
        public Entity GetTarget(int targetNum, bool targetingAttackers)
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
            Attackers = new List<Entity>();
            Targets = new List<Entity>();
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
            foreach (Entity ent in Entities)
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
            foreach (Entity ent in Attackers)
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
            foreach (Entity ent in Targets)
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
            foreach (string logStr in Logs)
            {
                logs.Add(new XElement("log", new XText(logStr)));
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
            StringBuilder txt = new StringBuilder("Team A:\n");
            for (int i = 0; i < Attackers.Count; i++)
            {
                Entity ent = Attackers[i];
                string usr = $"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}";
                if (ent.Dead)
                {
                    usr += " (DEFEATED)";
                }
                txt.AppendLine(usr);
            }

            txt.AppendLine("\nTeam T: ");
            for (int i = 0; i < Targets.Count; i++)
            {
                Entity ent = Targets[i];
                string usr = $"{i} - {ent.Name} - {ent.Health}/{ent.MaxHealth}";
                if (ent.Dead)
                {
                    usr += " (DEFEATED)";
                }
                txt.AppendLine(usr);
            }

            return txt.ToString();
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
            Log.Append(Display());


            AddLog();
            postedLogs = 1;

            // Safety net incase the strife is initialized with an empty team.
            if (Attackers.Any() && Targets.Any())
            {
                // Check for any events that should trigger when the strife starts.
                foreach (Entity ent in Attackers)
                {
                    // Will find a random target if the event needs it.
                    ent.TriggerEvent(EventType.OnStrifeStart, FindAvailableTarget(false, out int ID), true, this);
                }
                foreach (Entity ent in Targets)
                {
                    ent.TriggerEvent(EventType.OnStrifeStart, FindAvailableTarget(true, out int ID), false, this);
                }
            }

            // Notify any player involved.
            foreach (Entity ent in Entities)
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
        /// Checks who the next strifer is. If they're an AI then run through their turn. If the next one is dead then cycle through until the first non-dead strifer.
        /// </summary>
        /// <returns>A tuple containing the log of events that transpired when updating the strife and a boolean stating whether or not a human strifer is next.</returns>
        public Tuple<string, bool> UpdateStrife()
        {
            // Check who the next user is.
            Entity turner = CurrentEntity;

            // This disassociates the one doing the turn from the one who actually controls them.
            CurrentTurner = turner;

            bool humanNext = true;

            // Is the strife still going on?
            if (!Active)
            {
                return new Tuple<string, bool>(GetLogs(), true);
            }

            // Are all the members of one team dead?
            if (Attackers.All(x => x.Dead))
            {
                Log.AppendLine("Attackers have been defeated.");
                EndStrife();
                
                return new Tuple<string, bool>(GetLogs(), true);
            }
            if (Targets.All(x => x.Dead))
            {
                Log.AppendLine("Targets have been defeated.");
                EndStrife();
                
                return new Tuple<string, bool>(GetLogs(), true);
            }

            // Are they being mind-controlled?
            ulong controller = turner.GetMindController();
            if (controller > 0)
            {
                bool match = false;
                bool skipTurn = false;
                foreach (Entity ent in Entities)
                {
                    if (controller == ent.ID && !ent.Dead && ent.GetMindController() <= 0) 
                    {
                        Log.AppendLine($"{Syntax.ToCodeLine(turner.Name)}, controlled by {Syntax.ToCodeLine(ent.Name)}, is taking their turn!");

                        match = true;
                        
                        skipTurn = UpdateEntity(turner);

                        if (skipTurn)
                        {
                            UpdateTurn();
                            humanNext = false;
                        }
                        else
                        {
                            CurrentTurner = ent;
                        }
                        
                        break;
                    }
                }
                // Their controller is either dead or being mind-controlled themselves.
                if (!match)
                {
                    turner.RemoveStatusEffect(Constants.MIND_CONTROL_AIL, this, false);
                }

                AddLog();
                
                // AI is taking a turn.
                if (CurrentTurner is NPC && !skipTurn)
                {
                    TakeAITurn();
                    AddLog();
                    humanNext = false;
                }
            }
            // Not mind-controlled.
            else
            {
                AddLog();

                Log.AppendLine($"{Syntax.ToCodeLine(turner.Name)} is taking their turn!");

                bool skipTurn = UpdateEntity(turner);

                if (skipTurn)
                {
                    UpdateTurn();
                    humanNext = false;
                }
                else
                {
                    // AI is taking a turn.
                    if (turner is NPC)
                    {
                        TakeAITurn();
                        humanNext = false;
                    }
                }

                AddLog();
            }

            return new Tuple<string, bool>(GetLogs(), humanNext);
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

                        Log.AppendLine("Targets are now taking their turns.");
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

                        Log.AppendLine("Attackers are now taking their turns.");
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
        /// Updates the entity's inflicted status effects and move cooldowns.
        /// </summary>
        /// <returns>Whether or not the entity's turn is skipped.</returns>
        private bool UpdateEntity(Entity ent)
        {
            // Move cooldowns.
            foreach (KeyValuePair<string, Move> mov in ent.Moves)
            {
                if (mov.Value.Cooldown <= 0)
                {
                    continue;
                }

                --mov.Value.Cooldown;
                if (mov.Value.Cooldown <= 0)
                {
                    if (!string.IsNullOrWhiteSpace(mov.Value.RechargeMsg))
                    {
                        Log.AppendLine(Entity.GetEntityMessage(mov.Value.RechargeMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(mov.Value.Name)));
                    }
                }
            }

            // Status effects.
            bool skipTurn = false;
            for (int i = 0; i < ent.InflictedAilments.Count; i++)
            {
                StatusEffect sa = ent.InflictedAilments[i];
                // Is the status effect removed this turn?
                if (sa.Turns < 1)
                {
                    ent.RemoveStatusEffect(sa.Name, this, true);
                    --i;
                    continue;
                }

                skipTurn |= sa.Update(ent, null, attackTurn, this);
            }

            if (ent.Health <= 0 && !ent.Dead)
            {
                LeaveStrife(ent);
                skipTurn = true;
            }

            return skipTurn;
        }

        /// <summary>
        /// Validates a strife turn against a series of arbitrary parameters based on the action and state of the current turner.
        /// </summary>
        /// <param name="action">The action that is being performed this turn.</param>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <param name="reason">The reason for a turn being rejected if done so.</param>
        /// <returns>Whether or not the action is valid.</returns>
        public bool ValidateTurn(string action, int targetNum, bool targetingAttackers, Player plyr, out string reason)
        {
            Entity attacker = CurrentEntity;
            Entity target = GetTarget(targetNum, targetingAttackers);
            bool predefinedMove = Enum.TryParse(action, out StrifeAction sa);

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

            // Are they trying to use a move while it is blocked?
            if (attacker.BlockedMoves())
            {
                reason = "A status effect is blocking you from using character-specific moves.";
                return false;
            }

            // Are they trying to mind-control while mind-controlling?
            if (predefinedMove && attacker.GetMindController() > 0 && sa == StrifeAction.MindControl)
            {
                reason = "Invalid attack. Cannot mind-control while controlling someone else.";
                return false;
            }

            // Are they trying to use a lusus to perform psionic attacks?
            if (predefinedMove && npcAtk != null && npcAtk.Type == NPCType.Lusus
                && (sa == StrifeAction.MindControl || sa == StrifeAction.OpticBlast || sa == StrifeAction.SpeechAttack)
                )
            {
                reason = "Invalid attack. A lusus cannot perform psionic or speech attacks.";
                return false;
            }

            // Are they trying to intimidate a lusus?
            if (predefinedMove && npcTar != null && npcTar.Type == NPCType.Lusus && sa == StrifeAction.SpeechAttack)
            {
                reason = "Invalid attack. Cannot indimidate lusus.";
                return false;
            }

            // Does the move exist?
            if (!predefinedMove)
            {
                bool valid = false;
                foreach (KeyValuePair<string, Move> mov in attacker.Moves)
                {
                    if (mov.Key.StartsWith(action, StringComparison.OrdinalIgnoreCase))
                    {
                        // Is it on a cooldown.
                        if (mov.Value.Cooldown > 0)
                        {
                            string cool = mov.Value.Cooldown == 1
                                ? "turn"
                                : Syntax.ToCodeLine(mov.Value.Cooldown) + " turns";
                            reason = "Invalid attack. " + Syntax.ToCodeLine(mov.Value.Name) + " is on a cooldown for another " + cool + ".";
                            return false;
                        }

                        valid = true;
                        break;
                    }
                }

                if (!valid)
                {
                    reason = "Invalid attack. Could not find such that action.";
                    return false;
                }
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
        public string TakeTurn(string action, int targetNum, bool targetingAttackers)
        {
            Entity attacker = CurrentEntity;
            Entity target = GetTarget(targetNum, targetingAttackers);

            switch (action)
            {
                case Constants.PHYSICAL_ATTACK:
                    PhysicalAttack(attacker, target);
                    break;

                case Constants.MIND_CONTROL:
                    MindControl(attacker, target);
                    break;

                case Constants.OPTIC_BLAST:
                    OpticBlast(attacker, target);
                    break;

                case Constants.SPEECH_ATTACK:
                    SpeechAttack(attacker, target);
                    break;

                case Constants.GUARD:
                    Guard(attacker, target);
                    break;

                default:
                {
                    foreach (KeyValuePair<string, Move> mov in attacker.Moves)
                    {
                        if (mov.Key.StartsWith(action, StringComparison.OrdinalIgnoreCase))
                        {
                            mov.Value.Apply(attacker, target, attackTurn, this);
                            break;
                        }
                    }
                }
                break;
            }

            if (attacker.Health < 1 && !attacker.Dead)
            {
                LeaveStrife(attacker);
                attacker.TriggerEvent(EventType.OnDeath, target, attackTurn, this);
                target.TriggerEvent(EventType.OnKill, attacker, !attackTurn, this);
            }
            if (target.Health < 1 && !target.Dead)
            {
                LeaveStrife(target);
                target.TriggerEvent(EventType.OnDeath, attacker, !attackTurn, this);
                attacker.TriggerEvent(EventType.OnKill, target, attackTurn, this);
            }

            UpdateTurn();
            return CurrentTurner is NPC ? string.Empty : GetLogs();
        }

        /// <summary>
        /// Finds an entity that can be targeted (isn't dead essentially).
        /// </summary>
        /// <param name="attackTeam">Whether the target should be from the attacking team.</param>
        /// <param name="ID">The index of the list the entity is from.</param>
        private Entity FindAvailableTarget(bool attackTeam, out int ID)
        {
            ID = -1;
            Entity target = null;

            while (target == null || target.Dead)
            {
                ID = attackTeam
                    ? Toolbox.RandInt(Attackers.Count - 1)
                    : Toolbox.RandInt(Targets.Count - 1);

                target = GetTarget(ID, !attackTurn);
            }

            return target;
        }

        /// <summary>
        /// Let's an AI take their turn. Also handles whether they die this turn or not.
        /// Note that no string is returned here as that should be handled by the UpdateStrife() function.
        /// </summary>
        private void TakeAITurn()
        {
            Entity turner = CurrentEntity;
            if (turner == null)
            {
                throw new NullReferenceException($"Current entity is null. (TURN: {turn}, ATTACKTURN: {attackTurn.ToString()})");
            }
            
            if (turner is NPC npc)
            {
                // Select a target.
                Entity target = FindAvailableTarget(!attackTurn, out int targetID);

                // Use a move if the chance is high enough.
                float doT = Toolbox.RandFloat(0.0f, 1.0f);
                if (npc.Moves.Any(x => x.Value.Cooldown <= 0) && !npc.BlockedMoves() && doT <= npc.ChanceToMove)
                {
                    if (npc.MoveQueue.Count() < 1)
                    {
                        npc.GenerateMoveList();
                    }

                    while (npc.MoveQueue.Any())
                    {
                        Move mov = npc.MoveQueue.FirstOrDefault();
                        npc.MoveQueue.Remove(mov); // Remove the move even if it's on cooldown.

                        // Is the move on cooldown?
                        if (mov.Cooldown <= 0)
                        {
                            TakeTurn(mov.Name, targetID, !attackTurn);
                            break;
                        }
                    }
                }
                // Use a pre-defined move otherwise.
                else
                {
                    switch (npc.Type)
                    {
                        case NPCType.Lusus:
                        case NPCType.Normal:
                        {
                            TakeTurn(Constants.PHYSICAL_ATTACK, targetID, !attackTurn);
                        }
                        break;

                        case NPCType.Psionic:
                        {
                            int rng = Toolbox.RandInt(4);
                            // 25% chance to mind-control.
                            // Make sure the target isn't already being mind-controlled.
                            if (rng == 3 && target.GetMindController() == 0)
                            {
                                TakeTurn(Constants.MIND_CONTROL, targetID, !attackTurn);
                            }
                            // 25% chance to attack.
                            else if (rng == 2)
                            {
                                TakeTurn(Constants.PHYSICAL_ATTACK, targetID, !attackTurn);
                            }
                            // 25% chance to optic blast.
                            else
                            {
                                TakeTurn(Constants.OPTIC_BLAST, targetID, !attackTurn);
                            }
                        }
                        break;
                        
                        case NPCType.Talker:
                        {
                            if (Toolbox.TrueOrFalse(4))
                            {
                                TakeTurn(Constants.SPEECH_ATTACK, targetID, !attackTurn);
                            }
                            else
                            {
                                TakeTurn(Constants.GUARD, targetID, !attackTurn);
                            }
                        }
                        break;
                    }
                }
            }
            // Someone is being mind-controlled.
            else
            {
                // Select a target.
                Entity target = FindAvailableTarget(attackTurn, out int targetID);
                IEnumerable<Move> list = turner.Moves.Values.ToList().Where(x => x.Cooldown <= 0);
                
                int rng = Toolbox.RandInt(1, 4);
                switch (rng)
                {
                    case 1:
                    {
                        TakeTurn(Constants.PHYSICAL_ATTACK, targetID, attackTurn);
                    }
                    break;

                    case 2:
                    {
                        TakeTurn(Constants.SPEECH_ATTACK, targetID, attackTurn);
                    }
                    break;

                    case 3:
                    {
                        TakeTurn(Constants.OPTIC_BLAST, targetID, attackTurn);
                    }
                    break;

                    case 4:
                    {
                        if (list.Count() <= 0)
                        {
                            TakeTurn(Constants.GUARD, targetID, attackTurn);
                        }
                        else
                        {
                            TakeTurn(Toolbox.RandElement(list).Name, targetID, attackTurn);
                        }
                    }
                    break;
                }
            }
        }

        private void LeaveStrife(Entity ent)
        {
            ent.Dead = true;
            if (ent.Health < 0)
            {
                ent.Health = 0;
            }
            Log.AppendLine(Syntax.ToCodeLine(ent.Name) + " is no longer participating in the strife.");
        }

        public string Forfeit(ulong id)
        {
            Entity ent = Entities.FirstOrDefault(x => x.ID == id);
            if (ent == null)
            {
                Console.WriteLine("STRIFE ERROR: ID + \"" + id + "\" wasn't able to forfeit!");
                return null;
            }

            bool attacker = Attackers.Contains(ent);
            int index = attacker
                ? Attackers.IndexOf(ent)
                : Targets.IndexOf(ent);

            // Kill 'em.
            LeaveStrife(ent);

            // If it was their turn go to the next one.
            if (CurrentTurner.ID == id)
            {
                UpdateTurn();
            }
            
            AddLog();
            return GetLogs();
        }

        private void EndStrife()
        {
            Active = false;
            AddLog();
            Log.AppendLine("The strife is now over. XP will be awarded at the discretion of the GM.");
            AddLog();
            Log.Append(Display());
            AddLog();

            foreach (Entity ent in Entities)
            {
                ent.InflictedAilments.Clear();

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

                // Remove all cooldowns.
                foreach (Move mov in ent.Moves.Values)
                {
                    mov.Cooldown = 0;
                }
            }
        }

        // Physical: XDSTR --> XDCON.
        // If XDSTR rolls higher than difference between both is the damage on the target.
        // Then the target has a chance to counter attack if they roll higher.
        // XDSTR <-- XDPER: Debuff of the difference between both rolls is applied to the attacker's strength.
        private void PhysicalAttack(Entity attacker, Entity target)
        {
            Log.AppendLine(Toolbox.GetMessage("phyStart", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)));
            Log.AppendLine();

            // If target is immune to damage then ignore.
            if (target.ImmuneToDamage())
            {
                Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} is immune to damage.\n");
                return;
            }

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.GetTotalAbilities().Strength.Value;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.GetTotalAbilities().Constitution.Value;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {Syntax.ToCodeLine(atk)}!");
            Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} rolls {Syntax.ToCodeLine(tar)}!");

            // If attacker rolled higher inflict damage.
            if (atk > tar)
            {
                int dmg = atk - tar;
                target.Health -= dmg;

                Log.AppendLine();
                Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} took {Syntax.ToCodeLine(dmg.ToString())} hitpoint(s) of damage.");

                attacker.TriggerEvent(EventType.OnHit, target, attackTurn, this);
                target.TriggerEvent(EventType.OnAttacked, attacker, !attackTurn, this);

                // If the target died due to this attack end the attack right away.
                if (target.Health < 1) { return; }
            }
            else if (atk < tar)
            {
                Log.AppendLine();
                Log.AppendLine("Attack missed.");
            }
            // Equal rolls, do nothing.
            else
            {
                Log.AppendLine();
                Log.AppendLine("Attack blocked.");
            }

            // Counter attack.
            // If the strength modifier of the attacker is already debuffed below 1 then don't debuff.
            if (attacker.GetTotalAbilities().Strength.Value < 1)
            {
                Log.AppendLine();
                Log.AppendLine(Toolbox.GetMessage("phyCounterMax", Syntax.ToCodeLine(attacker.Name)));
                return;
            }
            else
            {
                tarY = target.GetTotalAbilities().Persuasion.Value;

                // 25% chance to counter. Increased by an addition 25 if the target's persuasion
                // is higher than the attacker's strength.
                if (Toolbox.TrueOrFalse(4 - (2 * Convert.ToInt32(tarY > atkY))))
                {
                    Log.AppendLine();
                    Log.AppendLine(Toolbox.GetMessage("phyCounterStart", Syntax.ToCodeLine(target.Name), Syntax.ToCodeLine(attacker.Name)));

                    atk = Toolbox.DiceRoll(atkX, atkY);
                    tar = Toolbox.DiceRoll(tarX, tarY);
                    Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {Syntax.ToCodeLine(atk)}!");
                    Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} rolls {Syntax.ToCodeLine(tar)}!");

                    // Counter failed.
                    if (atk >= tar)
                    {
                        Log.AppendLine();
                        Log.AppendLine(Toolbox.GetMessage("phyCounterBlock"));
                    }
                    // Counter suceeded, debuff strength.
                    else
                    {
                        int debuff = Math.Max(- (tar - atk), -tarY);
                        ApplyMod(attacker, Constants.PHYSICAL_COUNTER_AIL, Ability.STR, debuff, 1, target, attackTurn);
                    }
                }
            }
        }

        // Mental: XDPSI --> XDFOR 3 times in a row.
        private void MindControl(Entity attacker, Entity target)
        {
            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} is attempting to mind control {Syntax.ToCodeLine(target.Name)}.\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.GetTotalAbilities().Psion.Value;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.GetTotalAbilities().Fortitude.Value;

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

            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {Syntax.ToCodeLine(atk[0])}, {Syntax.ToCodeLine(atk[1])}, {Syntax.ToCodeLine(atk[2])}!");
            Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} rolls {Syntax.ToCodeLine(tar[0])}, {Syntax.ToCodeLine(tar[1])}, {Syntax.ToCodeLine(tar[2])}!");

            int atkTotal = atk[0] + atk[1] + atk[2];
            int tarTotal = tar[0] + tar[1] + tar[2];
            // Mind control wooo.
            if (success)
            {
                // Or sleep.
                if (Toolbox.RandFloat(0f, 1.0f) < 0.5f)
                {
                    target.ApplyStatusEffect(Constants.SLEEPING_AIL, attacker, !attackTurn, this);
                }
                else
                {
                    target.ApplyStatusEffect(Constants.MIND_CONTROL_AIL, attacker, !attackTurn, this);
                }
            }
            else
            {
                Log.AppendLine("Mind control failed.");
            }
        }

        private void OpticBlast(Entity attacker, Entity target)
        {
            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} is preparing an Optic Blast.\n");
            Log.AppendLine();

            // If target is immune to damage then ignore.
            if (target.ImmuneToDamage())
            {
                Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} is immune to damage.\n");
                return;
            }

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.GetTotalAbilities().Psion.Value;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.GetTotalAbilities().Fortitude.Value;

            // Dice rolls.
            int[] atk = new int[3];
            int[] tar = new int[3];

            for (int i = 0; i < 3; i++)
            {
                atk[i] = Toolbox.DiceRoll(atkX, atkY);
                tar[i] = Toolbox.DiceRoll(tarX, tarY);
            }

            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {Syntax.ToCodeLine(atk[0])}, {Syntax.ToCodeLine(atk[1])}, {Syntax.ToCodeLine(atk[2])}!");
            Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} rolls {Syntax.ToCodeLine(tar[0])}, {Syntax.ToCodeLine(tar[1])}, {Syntax.ToCodeLine(tar[2])}!");

            int atkTotal = atk[0] + atk[1] + atk[2];
            int tarTotal = tar[0] + tar[1] + tar[2];
            // Pew pew pew wooo.
            if (atkTotal > tarTotal)
            {
                int dmg = atkTotal - tarTotal;
                target.Health -= dmg;
                Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} struck {Syntax.ToCodeLine(target.Name)} for {Syntax.ToCodeLine(dmg.ToString())} hitpoints.");

                attacker.TriggerEvent(EventType.OnHit, target, attackTurn, this);
                target.TriggerEvent(EventType.OnAttacked, attacker, !attackTurn, this);
            }
            else
            {
                Log.AppendLine("Optic Blast missed.");
            }
        }

        // Speech: XD(INT+STR) --> XD(PER+FOR)
        // Random chance to do 3 things on success:
        // Roll 1DINT to debuff STR.
        // Roll 1DPER to debuff FOR.
        // Roll 1DINT to debuff INT.
        // If STR or FOR reach 0 the debuffs are removed but 'Enraged' is inflicted.
        private void SpeechAttack(Entity attacker, Entity target)
        {
            Log.AppendLine(Toolbox.GetMessage("speStart", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)) + "\n");

            // Attacker XDY roll.
            int atkX = attacker.DiceRolls;
            int atkY = attacker.GetTotalAbilities().Intimidation.Value + attacker.GetTotalAbilities().Strength.Value;

            // Target XDY roll.
            int tarX = target.DiceRolls;
            int tarY = target.GetTotalAbilities().Persuasion.Value + target.GetTotalAbilities().Fortitude.Value;

            // Dice rolls.
            int atk = Toolbox.DiceRoll(atkX, atkY);
            int tar = Toolbox.DiceRoll(tarX, tarY);
            Log.AppendLine($"{Syntax.ToCodeLine(attacker.Name)} rolls {Syntax.ToCodeLine(atk)}!");
            Log.AppendLine($"{Syntax.ToCodeLine(target.Name)} rolls {Syntax.ToCodeLine(tar)}!");
            
            // Attack rolls higher, random chance begins.
            if (atk > tar)
            {
                int rng = Toolbox.RandInt(0, 2);
                int debuff = 0;
                string stat = "";
                switch (rng)
                {
                    // Roll 1DINT to debuff STR.
                    case 0:
                    {
                        int y = attacker.GetTotalAbilities().Intimidation.Value;
                        debuff = Toolbox.DiceRoll(1, y);
                        debuff = Math.Min(target.BaseAbilities.Strength.Value / 3, debuff);
                        stat = Ability.STR;
                    } break;

                    // Roll 1DPER to debuff FOR.
                    case 1:
                    {
                        int y = attacker.GetTotalAbilities().Persuasion.Value;
                        debuff = Toolbox.DiceRoll(1, y);
                        debuff = Math.Min(target.BaseAbilities.Fortitude.Value / 3, debuff);
                        stat = Ability.FOR;
                    } break;
                    
                    // Roll 1DINT to debuff INT.
                    case 2:
                    {
                        int y = attacker.GetTotalAbilities().Intimidation.Value;
                        debuff = Toolbox.DiceRoll(1, y);
                        stat = Ability.INT;
                    } break;
                }

                ApplyMod(target, Constants.SPE_ATTACK_AIL, stat, -debuff, 10, attacker, !attackTurn);

                // If STR or FOR reach 0 then inflict them with the 'Enraged' status effect and remove all previous speech debuffs.
                if ((target.GetTotalAbilities().Strength.Value < 1 && rng == 0) || (target.GetTotalAbilities().Fortitude.Value < 1 && rng == 1))
                {
                    Log.AppendLine($"{Syntax.ToCodeLine(target.Name.ToApostrophe())} {stat} has fallen below 1.");

                    // HE'S ANGRY
                    Log.AppendLine(Syntax.ToCodeLine(target.Name.ToApostrophe()) + " speech debuffs were removed.");
                    target.ApplyStatusEffect(Constants.ENRAGED_AIL, attacker, !attackTurn, this);
                    target.RemoveStatusEffect(Constants.SPE_ATTACK_AIL, this, false);
                }
            }
            else
            {
                Log.AppendLine();
                Log.AppendLine(Toolbox.GetMessage("speFail", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)));
            }
        }

        // Guard CON += XDCON
        private void Guard(Entity attacker, Entity target)
        {
            int amount = Toolbox.DiceRoll(1, attacker.GetTotalAbilities().Constitution.Value);
            attacker.ApplyStatusEffect(Constants.GUARDING_AIL, target, attackTurn, this);
        }

        // TODO:
        public void Explosion(Explosion explosion, Entity ent, Entity tar, bool attackTeam, float fallOffFactor)
        {
            Entity splashZone = null;

            // Find the initial splash zone of the explosion.
            if ((explosion.Target & TargetType.Self) == TargetType.Self)
            {
                splashZone = ent;
            }
            else if ((explosion.Target & TargetType.Target) == TargetType.Target)
            {
                splashZone = tar ?? Toolbox.RandElement(Targets);
            }
            else if ((explosion.Target & TargetType.All) == TargetType.All)
            {
                splashZone = Toolbox.RandElement(Entities);
            }
            bool doubleTarget = explosion.Target == (TargetType.Self | TargetType.Target);

            Log.AppendLine();
            if (Toolbox.RandFloat(0f, 1.0f) >= 0.001)
            {
                if (doubleTarget)
                {
                    Log.AppendLine($"An explosion has occured at {Syntax.ToCodeLine(splashZone.Name)} and {Syntax.ToCodeLine(tar.Name.ToApostrophe())} positions!");
                }
                else
                {
                    Log.AppendLine($"An explosion has occured at {Syntax.ToCodeLine(splashZone.Name.ToApostrophe())} position!");
                }
            }
            else
            {
                Log.AppendLine("SHIT BLOWS THE FUCK UP!");
            }

            // Initial damage at splash zone.
            int dmg = explosion.FixedNumber
                ? (int) explosion.Damage
                : (int) (explosion.Damage * splashZone.MaxHealth);

            splashZone.Health -= dmg;
            Log.AppendLine();
            Log.AppendLine($"{Syntax.ToCodeLine(splashZone.Name)} took {dmg} damage!");
            if (doubleTarget)
            {
                tar.Health -= dmg;
                Log.AppendLine($"{Syntax.ToCodeLine(tar.Name)} took {dmg} damage!");
            }

            if (splashZone.Health <= 0)
            {
                LeaveStrife(splashZone);
            }
            if (tar.Health <= 0)
            {
                LeaveStrife(tar);
            }

            // If the explosion was only targetting one person/two people then end the function here.
            if (explosion.Target == TargetType.Self || explosion.Target == TargetType.Target || doubleTarget)
            {
                return;
            }

            switch (explosion.Target)
            {
                case TargetType.All | TargetType.Self:
                {

                }
                break;
            }
        }

        /// <summary>
        /// Generates a status effect with the specified ability modification.
        /// </summary>
        /// <param name="ent">The entity the effect is being applied to.</param>
        /// <param name="name">What to call the status effect.</param>
        /// <param name="stat">The ability being affected.</param>
        /// <param name="value">The amount to affect the ability by.</param>
        /// <param name="turns">The number of turns the status effect should last for.</param>
        /// <param name="target">The entity the user was targetting.</param>
        /// <param name="attackTeam">Whether or not the entity this status effect is applied to was on the attacking team.</param>
        private void ApplyMod(Entity ent, string name, string stat, int value, int turns, Entity tar, bool attackTeam)
        {
            if (turns <= 0) { return; }

            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (prop.Name.Contains(stat, StringComparison.OrdinalIgnoreCase))
                {
                    // For the time being this will assume the property's name
                    // is the same as the property it's representing.
                    Ability uh = new Ability(prop.Name);
                    uh.Value = value;
                    
                    prop.SetValue(set, uh);

                    StatusEffect sa = new StatusEffect();
                    sa.Name = name;
                    sa.Modifiers = set;
                    sa.Turns = turns;
                    sa.Stacks = true;
                    ent.ApplyStatusEffect(sa, tar, attackTeam, this);

                    string plural = turns == 1
                        ? "1 turn"
                        : (turns).ToString() + " turns";
                    Log.AppendLine($"\n{Syntax.ToCodeLine(ent.Name)} was inflicted with {Syntax.ToCodeLine(value.ToString("+0;-#"))} {prop.Name} for {Syntax.ToCodeLine(plural)}.");

                    return;
                }
            }
        }

        private void AddLog()
        {
            if (string.IsNullOrWhiteSpace(Log.ToString())) { return; }

            Log.AppendLine("\n------------");

            Logs.Add(Log.ToString());
            Log.Clear();
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
