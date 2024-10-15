using System;
using System.Collections.Generic;
using CrabNet.Framework;
using CrabNet_REDUX.I18n;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using CrabNet_REDUX.Framework;
using CrabNet_REDUX.GMCM;

namespace CrabNet
{
    public class CrabNet : Mod
    {
        /*********
        ** Properties
        *********/
        // Local variable for config setting "keybind"
        private SButton ActionKey;

        // Local variable for config setting "loggingEnabled"
        //private bool LoggingEnabled;

        // Local variable for config setting "preferredBait"
        //private string BaitChoice = "685";

        // Local variable for config setting "free"
        //private bool Free;

        // Local variable for config setting "chargeForBait"
        //private bool ChargeForBait = true;

        // Local variable for config setting "costPerCheck"
        //private int CostPerCheck = 1;

        // Local variable for config setting "costPerEmpty"
        //private int CostPerEmpty = 10;

        // Local variable for config setting "whoChecks"
        //private string Checker = "CrabNet";

        // Local variable for config setting "enableMessages"
        //private bool EnableMessages = true;

        // Local variable for config setting "chestCoords"
        //private Vector2 ChestCoords = new Vector2(73f, 14f);

        // Local variable for config setting "bypassInventory"
        //private bool BypassInventory;

        // Local variable for config setting "allowFreebies"
        //private bool AllowFreebies = true;

        // The cost per 1 bait, as determined from the user's bait preference
        //private int BaitCost;

        // An indexed list of all messages from the dialog.xna file
        //private Dictionary<string, string> AllMessages;

        // An indexed list of key dialog elements, these need to be indexed in the order in the file ie. cannot be randomized.
        private Dictionary<int, string> Dialog;

        // An indexed list of greetings.
        private Dictionary<int, string> Greetings;

        // An indexed list of all dialog entries relating to "unfinished"
        private Dictionary<int, string> UnfinishedMessages;

        // An indexed list of all dialog entries related to "freebies"
        private Dictionary<int, string> FreebieMessages;

        // An indexed list of all dialog entries related to "inventory full"
        private Dictionary<int, string> InventoryMessages;

        // An indexed list of all dialog entries related to "smalltalk".  This list is merged with a list of dialogs that are specific to your "checker"
        private Dictionary<int, string> Smalltalk;

        // Random number generator, used primarily for selecting dialog messages.
        private readonly Random Random = new Random();

        // A flag for when an item could not be deposited into either the inventory or the chest.
        private bool InventoryAndChestFull;

        // The configuration object.  Not used per-se, only to populate the local variables.
        private ModConfig Config;
        private IModHelper helper;
        //private DialogueManager DialogueManager;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            Config = Helper.ReadConfig<ModConfig>();
            //this.DialogueManager = new DialogueManager(this.Config, helper.Content, this.Monitor);
            i18n.Init(helper.Translation);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GMCMIntegration.SetupMenu(helper, ModManifest, Config);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            //if (!Enum.TryParse(Config.KeyBind, true, out this.ActionKey))
            //{
            //    ActionKey = SButton.H;
            //    Monitor.Log($"Error parsing key binding; defaulted to {ActionKey}.");
            //}

            //LoggingEnabled = Config.EnableLogging;

            // 685, or 774
            //if (Config.PreferredBait == "685" || Config.PreferredBait == "774")
            //    BaitChoice = Config.PreferredBait;
            //else
            //    BaitChoice = "685";

            //BaitCost = new SObject(BaitChoice, 1).Price;
            //ChargeForBait = Config.ChargeForBait;
            //CostPerCheck = Math.Max(0, Config.CostPerCheck);
            //CostPerEmpty = Math.Max(0, Config.CostPerEmpty);
            //Free = Config.Free;
            //Checker = Config.WhoChecks;
            //EnableMessages = Config.EnableMessages;
            //ChestCoords = Config.ChestCoords;
            //BypassInventory = Config.BypassInventory;
            //AllowFreebies = Config.AllowFreebies;

            ReadInMessages();
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == Config.KeyBind)
            {
                try
                {
                    IterateOverCrabPots();
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Exception onKeyReleased: {ex}", LogLevel.Error);
                }
            }
        }

        private void IterateOverCrabPots()
        {
            // reset this each time invoked, it is a flag to determine if uncompleted work is due to inventory or money.
            InventoryAndChestFull = false;
            CrabNetStats stats = new CrabNetStats(Config.WhoChecks);

            foreach (GameLocation location in Game1.locations)
            {
                if (location.IsOutdoors)
                {
                    foreach (SObject obj in location.Objects.Values)
                    {
                        if (obj.Name == "Crab Pot")
                        {
                            stats.numTotal++;

                            if (!Config.Free && !CanAfford(Game1.player, Config.CostPerCheck, stats) && !Config.AllowFreebies)
                            {
                                Monitor.Log("Couldn't afford to check.", LogLevel.Trace);
                                stats.notChecked++;
                                continue;
                            }

                            stats.numChecked++;
                            stats.runningTotal += Config.CostPerCheck;

                            CrabPot pot = (CrabPot)obj;

                            if (pot.heldObject.Value != null && pot.heldObject.Value.Category != -21)
                            {
                                if (!Config.Free && !CanAfford(Game1.player, Config.CostPerEmpty, stats) && !Config.AllowFreebies)
                                {
                                    Monitor.Log("Couldn't afford to empty.", LogLevel.Trace);
                                    stats.notEmptied++;
                                    continue;
                                }

                                if (CheckForAction(Game1.player, pot, stats))
                                {
                                    stats.numEmptied++;
                                    stats.runningTotal += Config.CostPerEmpty;
                                }
                            }
                            else
                            {
                                stats.nothingToRetrieve++;
                            }

                            if (pot.bait.Value == null && pot.heldObject.Value == null && !Game1.player.professions.Contains(11))
                            {
                                SObject b = new SObject(Config.PreferredBait, 1);

                                if (!Config.Free && !CanAfford(Game1.player, Config.BaitCost, stats) && !Config.AllowFreebies && Config.ChargeForBait)
                                {
                                    Monitor.Log("Couldn't afford to bait.", LogLevel.Trace);
                                    stats.notBaited++;
                                    continue;
                                }

                                if (PerformObjectDropInAction(b, Game1.player, pot))
                                {
                                    stats.numBaited++;
                                    if (Config.ChargeForBait)
                                        stats.runningTotal += Config.BaitCost;
                                }
                            }
                            else
                            {
                                stats.nothingToBait++;
                            }

                        }
                    }
                }
            }

            int totalCost = (stats.numChecked * Config.CostPerCheck);
            totalCost += (stats.numEmptied * Config.CostPerEmpty);
            if (Config.ChargeForBait)
            {
                totalCost += (Config.BaitCost * stats.numBaited);
            }
            if (Config.Free)
            {
                totalCost = 0;
            }

            if (Config.EnableLogging)
            {
                Monitor.Log($"CrabNet checked {stats.numChecked} pots. You used {stats.numBaited} bait to reset.", LogLevel.Trace);
                if (!Config.Free)
                    Monitor.Log($"Total cost was {totalCost}g. Checks: {stats.numChecked * Config.CostPerCheck}, Emptied: {stats.numEmptied * Config.CostPerEmpty}, Bait: {stats.numBaited * Config.BaitCost}", LogLevel.Trace);
            }

            if (!Config.Free)
                Game1.player.Money = Math.Max(0, Game1.player.Money + (-1 * totalCost));

            if (Config.EnableMessages)
            {
                ShowMessage(stats, totalCost);
            }
        }

        private bool CheckForAction(Farmer farmer, CrabPot pot, CrabNetStats stats)
        {
            if (!CanAfford(farmer, Config.CostPerCheck, stats))
                return false;

            if (pot.tileIndexToShow == 714)
            {
                if (farmer.IsMainPlayer && !AddItemToInventory(pot.heldObject.Value, farmer, Game1.getFarm()))
                {
                    Game1.addHUDMessage(new HUDMessage("Inventory Full", 3500f));
                    return false;
                }
                Dictionary<string, string> dictionary = Helper.GameContent.Load<Dictionary<string, string>>("Data\\Fish");
                if (GetFishSize(pot.heldObject.Value.ParentSheetIndex, out int minSize, out int maxSize))
                    farmer.caughtFish(pot.heldObject.Value.ItemId, Game1.random.Next(minSize, maxSize + 1));
                pot.readyForHarvest.Value = false;
                pot.heldObject.Value = null;
                pot.tileIndexToShow = 710;
                pot.bait.Value = null;
                farmer.gainExperience(1, 5);

                return true;
            }
            return false;
        }

        /// <summary>Get the minimum and maximum size for a fish.</summary>
        /// <param name="parentSheetIndex">The parent sheet index for the fish.</param>
        /// <param name="minSize">The minimum fish size.</param>
        /// <param name="maxSize">The maximum fish size.</param>
        private bool GetFishSize(int parentSheetIndex, out int minSize, out int maxSize)
        {
            minSize = -1;
            maxSize = -1;

            // get data
            Dictionary<string, string> data = Helper.GameContent.Load<Dictionary<string, string>>("Data\\Fish");
            if (!data.TryGetValue(parentSheetIndex.ToString(), out string rawFields) || rawFields == null || !rawFields.Contains("/"))
                return false;

            // get field indexes
            string[] fields = rawFields.Split('/');
            int minSizeIndex = fields[1] == "trap"
                ? 5
                : 3;
            int maxSizeIndex = minSizeIndex + 1;
            if (fields.Length <= maxSizeIndex)
                return false;

            // parse fields
            if (!int.TryParse(fields[minSizeIndex], out minSize))
                minSize = 1;
            if (!int.TryParse(fields[maxSizeIndex], out maxSize))
                maxSize = 10;
            return true;
        }

        private bool PerformObjectDropInAction(SObject dropIn, Farmer farmer, CrabPot pot)
        {
            if (pot.bait.Value != null || farmer.professions.Contains(11))
                return false;

            pot.bait.Value = dropIn;

            return true;
        }

        private bool AddItemToInventory(SObject obj, Farmer farmer, Farm farm)
        {
            bool wasAdded = false;

            if (farmer.couldInventoryAcceptThisItem(obj) && !Config.BypassInventory)
            {
                farmer.addItemToInventory(obj);
                wasAdded = true;

                if (Config.EnableLogging)
                    Monitor.Log("Was able to add item to inventory.", LogLevel.Trace);
            }
            else
            {
                farm.objects.TryGetValue(Config.ChestCoords, out SObject chestObj);

                if (chestObj is Chest chest)
                {
                    if (Config.EnableLogging)
                        Monitor.Log($"Found a chest at {(int)Config.ChestCoords.X},{(int)Config.ChestCoords.Y}", LogLevel.Trace);

                    Item i = chest.addItem(obj);
                    if (i == null)
                    {
                        wasAdded = true;

                        if (Config.EnableLogging)
                            Monitor.Log("Was able to add items to chest.", LogLevel.Trace);
                    }
                    else
                    {
                        this.InventoryAndChestFull = true;

                        if (Config.EnableLogging)
                            Monitor.Log("Was NOT able to add items to chest.", LogLevel.Trace);
                    }

                }
                else
                {
                    if (Config.EnableLogging)
                        Monitor.Log($"Did not find a chest at {(int)Config.ChestCoords.X},{(int)Config.ChestCoords.Y}", LogLevel.Trace);

                    // If bypassInventory is set to true, but there's no chest: try adding to the farmer's inventory.
                    if (Config.BypassInventory)
                    {
                        if (Config.EnableLogging)
                            Monitor.Log($"No chest at {(int)Config.ChestCoords.X},{(int)Config.ChestCoords.Y}, you should place a chest there, or set bypassInventory to \'false\'.", LogLevel.Trace);

                        if (farmer.couldInventoryAcceptThisItem(obj))
                        {
                            farmer.addItemToInventory(obj);
                            wasAdded = true;

                            if (Config.EnableLogging)
                                Monitor.Log("Was able to add item to inventory. (No chest found, bypassInventory set to 'true')", LogLevel.Trace);
                        }
                        else
                        {
                            InventoryAndChestFull = true;

                            if (Config.EnableLogging)
                                Monitor.Log("Was NOT able to add item to inventory or a chest.  (No chest found, bypassInventory set to 'true')", LogLevel.Trace);
                        }
                    }
                    else
                    {
                        InventoryAndChestFull = true;

                        if (Config.EnableLogging)
                            Monitor.Log("Was NOT able to add item to inventory or a chest.  (No chest found, bypassInventory set to 'false')", LogLevel.Trace);
                    }
                }
            }

            return wasAdded;
        }

        private bool CanAfford(Farmer farmer, int amount, CrabNetStats stats)
        {
            // Calculate the running cost (need config passed for that) and determine if additional puts you over.
            return (amount + stats.runningTotal) <= farmer.Money;
        }


        private void ShowMessage(CrabNetStats stats, int totalCost)
        {
            string message = "";

            if (Config.WhoChecks.ToLower() == "spouse")
            {
                if (Game1.player.GetDaysMarried() > 0)
                    message += DialogueManagerV2.PerformReplacement(Dialog[1], stats, Config);
                else
                    message += DialogueManagerV2.PerformReplacement(Dialog[2], stats, Config);

                if (totalCost > 0 && !Config.Free)
                    message += DialogueManagerV2.PerformReplacement(Dialog[3], stats, Config);

                HUDMessage msg = new HUDMessage(message);
                Game1.addHUDMessage(msg);
            }
            else
            {
                NPC character = Game1.getCharacterFromName(Config.WhoChecks);
                if (character != null)
                {
                    message += DialogueManagerV2.PerformReplacement(GetRandomMessage(Greetings), stats, Config);
                    message += " " + DialogueManagerV2.PerformReplacement(Dialog[4], stats, Config);

                    if (!Config.Free)
                    {
                        DialogueManagerV2.PerformReplacement(Dialog[5], stats, Config);

                        if (stats.HasUnfinishedBusiness())
                        {
                            if (InventoryAndChestFull)
                            {
                                message += DialogueManagerV2.PerformReplacement(GetRandomMessage(InventoryMessages), stats, Config);
                            }
                            else
                            {
                                if (Config.AllowFreebies)
                                {
                                    message += DialogueManagerV2.PerformReplacement(GetRandomMessage(FreebieMessages), stats, Config);
                                }
                                else
                                {
                                    message += " " + DialogueManagerV2.PerformReplacement(GetRandomMessage(UnfinishedMessages), stats, Config);
                                }
                            }
                        }

                        message += DialogueManagerV2.PerformReplacement(GetRandomMessage(Smalltalk), stats, Config);
                        message += "#$e#";
                    }
                    else
                    {
                        message += DialogueManagerV2.PerformReplacement(GetRandomMessage(Smalltalk), stats, Config);
                        message += "#$e#";
                    }

                    character.CurrentDialogue.Push(new Dialogue(character, "", message));
                    Game1.drawDialogue(character);
                }
                else
                {
                    message += DialogueManagerV2.PerformReplacement(Dialog[6], stats, Config);
                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
            }
        }

        private string GetRandomMessage(Dictionary<int, string> messageStore)
        {
            int rand = Random.Next(1, messageStore.Count + 1);
            string value = "";
            if (messageStore.TryGetValue(rand, out string? message))
                value = message;

            if (Config.EnableLogging)
                Monitor.Log($"condition met to return random message, returning:{value}", LogLevel.Trace);

            return value;
        }


        private void ReadInMessages()
        {
            try
            {
                Dialog = DialogueManagerV2.GetDialogue("Xdialog");
                Greetings = DialogueManagerV2.GetDialogue("greeting");
                UnfinishedMessages = DialogueManagerV2.GetDialogue("unfinishedmoney");
                FreebieMessages = DialogueManagerV2.GetDialogue("freebies");
                InventoryMessages = DialogueManagerV2.GetDialogue("unfinishedinventory");
                Smalltalk = DialogueManagerV2.GetDialogue("smalltalk");

                Dictionary<int, string> characterDialog = DialogueManagerV2.GetDialogue(Config.WhoChecks);
                AddToSmallTalk(characterDialog);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Exception loading content:{ex}", LogLevel.Error);
            }
        }
        private void AddToSmallTalk(Dictionary<int, string> moresmalltalk)
        {
            if (moresmalltalk.Count > 0)
            {
                int index = Smalltalk.Count + 1;
                foreach (KeyValuePair<int, string> d in moresmalltalk)
                {
                    Smalltalk.Add(index, d.Value);
                    index++;
                }
            }
        }
    }
}
