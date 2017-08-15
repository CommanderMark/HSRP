using System;
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

                throw new NullReferenceException($"Current entity is null. (TURN: {turn}, ATTACKTURN: {attackTurn.ToString()})");
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

        public Strife()
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
                new XAttribute("currentTurn", CurrentTurner.ID)
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

            turn = 0;
            attackTurn = true;
            CurrentTurner = CurrentEntity;

            // Add list of users to log but skip it when posting the logs as that's posted separately.
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

            // Are they being mind-controlled?
            if (turner.Controller > 0)
            {
                bool match = false;
                foreach (IEntity ent in Entities)
                {
                    if (turner.Controller == ent.ID)
                    {
                        log = log.AddLine($"{Syntax.ToCodeLine(turner.Name)}, controlled by {Syntax.ToCodeLine(ent.Name)}, is taking their turn!");
                        CurrentTurner = ent;
                        match = true;
                        break;
                    }
                }
                // Their controller isn't in the strife anymore (hopefully).
                if (!match)
                {
                    log = log.AddLine($"{Syntax.ToCodeLine(turner.Name)} is no longer mind-controlled!");
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

        /// <summary>
        /// Validates a strife turn against a series of arbitrary parameters based on the action and state of the current turner.
        /// </summary>
        /// <param name="action">The action that is being performed this turn.</param>
        /// <param name="targetNum">The index of the user being targeted.</param>
        /// <param name="targetingAttackers">Whether the attacker is targeting someone on the attacking team.</param>
        /// <param name="reason">The reason for a turn being rejected if done so.</param>
        /// <returns>Whether or not the action is valid.</returns>
        public bool ValidateTurn(StrifeAction action, int targetNum, bool targetingAttackers, out string reason)
        {
            IEntity attacker = CurrentEntity;
            IEntity target = GetTarget(targetNum, targetingAttackers);

            // Is it their turn?
            if (CurrentTurner.ID != attacker.ID)
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
                reason = $"Invalid target. The targeted user is no longer in the strife.";
                return false;
            }

            // Are they targeting themselves?
            if (attacker.ID == target.ID)
            {
                reason = $"Invalid target. You cannot target yourself.";
                return false;
            }

            // Are they trying to mind-control while mind-controlling?
            if (attacker.Controller > 0 && action == StrifeAction.MindControl)
            {
                reason = $"Invalid attack. Cannot mind-control while controlling someone else.";
                return false;
            }

            // Are they trying to use a lusus to perform psionic attacks?
            if (attacker is NPC npc && npc.Type == NPCType.Lusus
                && (action == StrifeAction.MindControl || action == StrifeAction.OpticBlast)
                )
            {
                reason = $"Invalid attack. A lusus cannot perform psionic attacks.";
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
            bool returnEmp = (attacker is NPC && attacker.Controller <= 0) || (attacker is Player && attacker.Controller > 0) ? true : false;

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
                
                // TODO: Whoops.
                case StrifeAction.Abscond:
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
                        AddLog();

                        log = log.AddLine("Targets are now taking their turns.");
                    }
                } while (Attackers[turn].Dead && attackTurn);
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
                    if (turn >= Targets.Count)
                    {
                        // Reached the end of the attackers list.
                        attackTurn = true;
                        turn = 0;
                        AddLog();

                        log = log.AddLine("Attackers are now taking their turns.");
                    }
                } while (Targets[turn].Dead && !attackTurn);
            }

            AddLog();
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

                    // TODO: Add case for mind-controlled players if that's ever a thing.
                    case NPCType.Psionic:
                        TakeTurn(StrifeAction.OpticBlast, targetID, !attackTurn);
                        break;
                }
            }
        }

        private void LeaveStrife(IEntity ent)
        {
            ent.Dead = true;
            log = log.AddLine(Syntax.ToCodeLine(ent.Name) + " is no longer participating in the strife.");
        }

        private void EndStrife()
        {

        }

        // Physical: XDSTR --> XDCON.
        // If XDSTR rolls higher than difference between both is the damage on the target.
        // Assuming they're not equal, then the target has a chance to counter attack if they rolls higher.
        // XDSTR <-- XDPER: Debuff of the difference between both roles is applied to the attacker's strength.
        private void PhysicalAttack(IEntity attacker, IEntity target)
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
                        ApplyTempMod(attacker, "strength", debuff, 1);
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
        private void MindControl(IEntity attacker, IEntity target)
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
        private void OpticBlast(IEntity attacker, IEntity target)
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
        private void SpeechAttack(IEntity attacker, IEntity target)
        {
            log = log.AddLine(Toolbox.GetRandomMessage("speechAttackStart", Syntax.ToCodeLine(attacker.Name), Syntax.ToCodeLine(target.Name)) + "\n");

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
                            ApplyTempMod(target, "strength", debuff, 2);
                        } break;

                    // Roll 1DPER to debuff FOR for 1 turn.
                    case 1:
                        {
                            int y = attacker.Abilities.Persuasion;
                            int debuff = -Toolbox.DiceRoll(1, y);
                            ApplyTempMod(target, "fortitude", debuff, 0);
                        } break;
                    
                    // Roll 1DINT to debuff INT for 1 turn.
                    case 2:
                        {
                            int y = attacker.Abilities.Intimidation;
                            int debuff = -Toolbox.DiceRoll(1, y);
                            ApplyTempMod(target, "intimidation", debuff, 0);
                        } break;
                }

                // If STR or FOR reach 0 they leave the strife.
                if (target.Abilities.Strength < 1 && rng == 0)
                {
                    log = log.AddLine($"{target.Name.ToApostrophe()} strength has fallen below 1.");
                    log = log.AddLine(Toolbox.GetRandomMessage("speechKill", target.Name));

                    LeaveStrife(target);
                }
                else if (target.Abilities.Fortitude < 1 && rng == 1)
                {
                    log = log.AddLine($"{target.Name.ToApostrophe()} fortitude has fallen below 1.");
                    log = log.AddLine(Toolbox.GetRandomMessage("speechKill", target.Name));

                    LeaveStrife(target);
                }
            }
            else
            {

            }
        }

        // Guard CON += XDCON
        private void Guard(IEntity plyr)
        {
            log = log.AddLine($"{plyr.Name} is guarding.");

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

        public string LogLogs()
        {
            string txt = string.Join("\n", Logs);
            string path = Path.Combine(Dirs.Config, $"{Dirs.StrifeLogs}{ID}.txt");
            File.WriteAllText(path, txt);

            return path;
        }

        public string ClearLogs()
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
                    ent.AddTempMod(set, turns);

                    string plural = turns == 0
                        ? "1 turn"
                        : (turns + 1).ToString() + " turns";
                    log = log.AddLine($"\n{Syntax.ToCodeLine(ent.Name)} was inflicted with {Syntax.ToCodeLine(value.ToString())} {prop.Name} for {Syntax.ToCodeLine(plural)}.");

                    return;
                }
            }
        }
    }
}
