﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using StardewLib;
using Log = StardewLib.Log;
using Object = StardewValley.Object;
using System.Reflection;
using SFarmer = StardewValley.Farmer;

namespace ExtremePetting
{
    public class AnimalSitter : Mod
    {

        private static Keys petKey;

        // Whether to use dark magic to age the animals to maturity when visiting the animals.
        private bool growUpEnabled = true;

        // Whether to pet the animal until their maximum happiness level is reached.
        private bool maxHappinessEnabled = true;

        // Whether to feed the animals to their max fullness when visiting.
        private bool maxFullnessEnabled = true;

        // Whether to harvest animal drops while visiting.
        private bool harvestEnabled = true;

        // Whether to pet animals as they are visited.
        private bool pettingEnabled = true;

        // Whether to max the animal's friendship toward the farmer while visiting, even though the farmer is completely ignoring them.
        private bool maxFriendshipEnabled = true;

        // Whether to display the in game dialogue messages.
        private bool messagesEnabled = true;

        // Who does the checking.
        private string checker = "spouse";

        // How much to charge per animal.
        private int costPerAnimal = 0;

        // Whether to display debugging log messages.
        private bool loggingEnabled = false;

        // Whether to snatch hidden truffles from the snout of the pig.
        private bool takeTrufflesFromPigs = true;

        // Coordinates of the default chest.
        private Vector2 chestCoords = new Vector2(73f, 14f);

        // Whether to bypass the inventory, and first attempt to deposit the harvest into the chest.  Inventory is then used as fallback.
        private bool bypassInventory = false;

        // A string defining the locations of specific chests.
        private String chestDefs = "";
        
        // Whether both inventory and chests are full.
        private bool inventoryAndChestFull = false;

        // How many days the farmer has not been able to afford to pay the laborer.
        private int shortDays = 0;


        public static AnimalSitterConfig config;

        public AnimalSitter() : base()
        {
        }

        public override void Entry(params object[] objects)
        {
            PlayerEvents.LoadedGame += onLoaded;
            ControlEvents.KeyReleased += onKeyReleased;
        }


        private void onLoaded(object sender, EventArgs e)
        {
            AnimalSitter.config = (AnimalSitterConfig)ConfigExtensions.InitializeConfig<AnimalSitterConfig>(new AnimalSitterConfig(), this.BaseConfigPath);

            importConfiguration();

            //parseChestLocations();
            ChestManager.parseChests(this.chestDefs);
            ChestManager.setDefault(this.chestCoords);

            // Read in dialogue
            DialogueManager.initialize(Game1.content.ServiceProvider, this.PathOnDisk);
            DialogueManager.config = config;
            DialogueManager.readInMessages();

            Log.INFO((object)("[Animal-Sitter] chestCoords:" + this.chestCoords.X + "," + this.chestCoords.Y));
        }


        private void importConfiguration()
        {
            Log.enabled = config.verboseLogging;

            if (!Enum.TryParse<Keys>(config.keybind, true, out AnimalSitter.petKey))
            {
                AnimalSitter.petKey = Keys.O;
                Log.force_INFO((object)"[Animal-Sitter] Error parsing key binding. Defaulted to O");
            }

            this.pettingEnabled = config.pettingEnabled;
            this.growUpEnabled = config.growUpEnabled;
            this.maxHappinessEnabled = config.maxHappinessEnabled;
            this.maxFriendshipEnabled = config.maxFriendshipEnabled;
            this.maxFullnessEnabled = config.maxFullnessEnabled;
            this.harvestEnabled = config.harvestEnabled;
            this.loggingEnabled = config.verboseLogging;
            this.checker = config.whoChecks;
            this.messagesEnabled = config.enableMessages;
            this.takeTrufflesFromPigs = config.takeTrufflesFromPigs;
            this.chestCoords = config.chestCoords;

            this.bypassInventory = config.bypassInventory;
            this.chestDefs = config.chestDefs;

            if (config.costPerAction < 0)
            {
                Log.INFO((object)"[Animal-Sitter] I'll do it for free, but I'm not paying YOU to take care of YOUR stinking animals!");
                Log.force_INFO((object)"[Animal-Sitter] Setting costPerAction to 0.");
                this.costPerAnimal = 0;
            }
            else
            {
                this.costPerAnimal = config.costPerAction;
            }
        }

        private void onKeyReleased(object sender, EventArgsKeyPressed e)
        {
            if (Game1.currentLocation == null
                || (Game1.player == null
                || Game1.hasLoadedGame == false)
                || ((Game1.player).UsingTool
                || !(Game1.player).CanMove
                || (Game1.activeClickableMenu != null
                || Game1.CurrentEvent != null))
                || Game1.gameMode != 3) {

                return;
            }

            if (e.KeyPressed == AnimalSitter.petKey)
            {
                try
                {
                    iterateOverAnimals();
                }
                catch (Exception ex)
                {
                    if (loggingEnabled)
                    {
                        Log.force_ERROR((object)("[Animal-Sitter] Exception onKeyReleased: " + ex.ToString()));
                    }
                }

            }
        }


        private void iterateOverAnimals()
        {
            SFarmer farmer = Game1.player;
            Farm farm = Game1.getFarm();
            AnimalTasks stats = new AnimalTasks();

            foreach (FarmAnimal animal in getAllFarmAnimals())
            {
                try
                {
                    if (!animal.wasPet && this.pettingEnabled)
                    {
                        animal.pet(Game1.player);
                        stats.animalsPet++;

                        Log.INFO((object)("[Animal-Sitter] Petting animal: " + animal.name));
                    }


                    if (this.growUpEnabled && animal.isBaby())
                    {
                        Log.INFO((object)("[Animal-Sitter] Aging animal to mature+1 days: " + animal.name));

                        animal.age = animal.ageWhenMature + 1;
                        animal.reload();
                        stats.aged++;
                    }

                    if (this.maxFullnessEnabled && animal.fullness < byte.MaxValue)
                    {
                        Log.INFO((object)("[Animal-Sitter] Feeding animal: " + animal.name));

                        animal.fullness = byte.MaxValue;
                        stats.fed++;
                    }

                    if (this.maxHappinessEnabled && animal.happiness < byte.MaxValue)
                    {
                        Log.INFO((object)("[Animal-Sitter] Maxing Happiness of animal " + animal.name));

                        animal.happiness = byte.MaxValue;
                        stats.maxHappiness++;
                    }

                    if (this.maxFriendshipEnabled && animal.friendshipTowardFarmer < 1000)
                    {
                        Log.INFO((object)("[Animal-Sitter] Maxing Friendship of animal " + animal.name));

                        animal.friendshipTowardFarmer = 1000;
                        stats.maxFriendship++;
                    }

                    if (animal.currentProduce > 0 && this.harvestEnabled)
                    {
                        Log.INFO((object)("[Animal-Sitter] Has produce: " + animal.name + " " + animal.currentProduce));

                        if (animal.type.Equals("Pig"))
                        {
                            if (takeTrufflesFromPigs)
                            {
                                //Game1.player.addItemToInventoryBool((Item)new StardewValley.Object(animal.currentProduce, 1, false, -1, animal.produceQuality), false);
                                StardewValley.Object toAdd = new StardewValley.Object(animal.currentProduce, 1, false, -1, animal.produceQuality);
                                addItemToInventory(toAdd, farmer, farm, stats);

                                animal.currentProduce = 0;
                                stats.trufflesHarvested++;
                            }
                        }
                        else
                        {
                            StardewValley.Object toAdd = new StardewValley.Object(animal.currentProduce, 1, false, -1, animal.produceQuality);
                            addItemToInventory(toAdd, farmer, farm, stats);

                            animal.currentProduce = 0;
                            stats.productsHarvested++;
                        }


                    }
                }
                catch (Exception ex)
                {
                    if (loggingEnabled)
                    {
                        Log.force_ERROR((object)("[Animal-Sitter] Exception onKeyReleased: " + ex.ToString()));
                    }
                }
            }

            harvestTruffles(stats);
            harvestCoops(stats);

            bool doesPlayerHaveEnoughCash = true;
            int actions = stats.getTaskCount();
            bool gatheringOnly = stats.justGathering();

            if (actions > 0 && this.costPerAnimal > 0)
            {
                int totalCost = actions * this.costPerAnimal;
                doesPlayerHaveEnoughCash = Game1.player.Money >= totalCost;
                Game1.player.Money = Math.Max(0, Game1.player.Money - totalCost);

                if (messagesEnabled)
                    showMessage(actions, totalCost, doesPlayerHaveEnoughCash, gatheringOnly, stats);

                Log.INFO((object)("[Animal-Sitter] Animal sitter performed " + actions + " actions. Total cost: " + totalCost + "g"));

            }
            else if (actions == 0 && this.costPerAnimal > 0)
            {
                if (messagesEnabled)
                {
                    HUDMessage msg = new HUDMessage("There's nothing to do for the animals right now.");
                    Game1.addHUDMessage(msg);
                }

                Log.INFO((object)("[Animal-Sitter] There's nothing to do for the animals right now."));
            }
        }


        private void harvestTruffles(AnimalTasks stats)
        {
            Farm farm = Game1.getFarm();
            SFarmer farmer = Game1.player;

            List<Vector2> itemsToRemove = new List<Vector2>();

            // Iterate over the objects, and add them to inventory.
            foreach (KeyValuePair<Vector2, StardewValley.Object> keyvalue in farm.Objects) {
                StardewValley.Object obj = keyvalue.Value;

                if (obj.Name == "Truffle")
                {
                    bool doubleHarvest = false;

                    if (Game1.player.professions.Contains(16))
                        obj.quality = 4;

                    double randomNum = Game1.random.NextDouble();
                    bool doubleChance = (checker.Equals("pet")) ? (randomNum < 0.4) : (randomNum < 0.2);

                    if (Game1.player.professions.Contains(13) && doubleChance)
                    {
                        obj.Stack = 2;
                        doubleHarvest = true;
                    }
                        
                    if (addItemToInventory(obj, farmer, farm, stats))
                    {
                        itemsToRemove.Add(keyvalue.Key);
                        farmer.gainExperience(2, 7);
                        stats.trufflesHarvested++;

                        if (doubleHarvest)
                        {
                            stats.trufflesHarvested++;
                            farmer.gainExperience(2, 7);
                        }
                            
                    }
                    else
                    {
                        Log.INFO((object)("[Animal-Sitter] Inventory full, could not add animal product."));
                    }
                }

            }

            // Now remove the items
            foreach (Vector2 itemLocation in itemsToRemove)
            {
                farm.removeObject(itemLocation, false);
            }

        }

        private void harvestCoops(AnimalTasks stats)
        {
            Farm farm = Game1.getFarm();
            SFarmer farmer = Game1.player;

            foreach (Building building in farm.buildings)
            {
                if (building is Coop)
                {
                    List<Vector2> itemsToRemove = new List<Vector2>();

                    foreach (KeyValuePair<Vector2, StardewValley.Object> keyvalue in building.indoors.Objects)
                    {
                        StardewValley.Object obj = keyvalue.Value;

                        Log.INFO((object)("[Animal-Sitter] Found coop object: " + obj.Name + " / " + obj.Category + "/" + obj.isAnimalProduct()));

                        if (obj.isAnimalProduct() || obj.parentSheetIndex == 107)
                        {
                            if (addItemToInventory(obj, farmer, farm, stats))
                            {
                                itemsToRemove.Add(keyvalue.Key);
                                stats.productsHarvested++;
                                farmer.gainExperience(0, 5);
                            }
                            else
                            {
                                Log.INFO((object)("[Animal-Sitter] Inventory full, could not add animal product."));
                            }
                        }
                    }

                    // Remove the object that were picked up.
                    foreach (Vector2 itemLocation in itemsToRemove)
                    {
                        building.indoors.removeObject(itemLocation, false);
                    }
                }
            }

        }


        private bool addItemToInventory(Object obj, SFarmer farmer, Farm farm, AnimalTasks stats)
        {
            bool wasAdded = false;

            if (!bypassInventory)
            {
                if (farmer.couldInventoryAcceptThisItem(obj))
                {
                    farmer.addItemToInventory(obj);
                    return true;
                }
            }

            // Get the preferred chest (could be default)
            Object chest = ChestManager.getChest(obj.parentSheetIndex);

            if (chest != null && (chest is Chest))
            {
                Item i = ((Chest)chest).addItem(obj);
                if (i == null)
                    return true;
            }

            // We haven't returned, get the default chest.
            chest = ChestManager.getDefaultChest();

            if (chest != null && (chest is Chest))
            {
                Item i = ((Chest)chest).addItem(obj);
                if (i == null)
                    return true;
            }

            // Haven't been able to add to a chest, try inventory one last time.
            if (farmer.couldInventoryAcceptThisItem(obj))
            {
                farmer.addItemToInventory(obj);
                return true;
            }

            inventoryAndChestFull = true;
            return wasAdded;
        }


        private String getGathererName()
        {
            if (checker.ToLower() == "spouse")
            {
                if (Game1.player.isMarried())
                    return Game1.player.getSpouse().getName();
                else
                    return "The animal sitter";
            }
            else
            {
                return checker;
            }
                
        }


        private void showMessage(int numActions, int totalCost, bool doesPlayerHaveEnoughCash, bool gatheringOnly, AnimalTasks stats)
        {
            stats.numActions = numActions;
            stats.totalCost = totalCost;

            string message = "";

            if (checker.ToLower() == "pet")
            {
                if (Game1.player.hasPet())
                {
                    if (Game1.player.catPerson)
                    {
                        message += "Meow..";
                    }
                    else
                    {
                        message += "Woof.";
                    }
                }
                else
                {
                    message += "Your imaginary pet has taken care of your animals.";
                }

                HUDMessage msg = new HUDMessage(message);
                Game1.addHUDMessage(msg);
            }
            else
            {
                if (checker.ToLower() == "spouse")
                {
                    if (Game1.player.isMarried())
                    {
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(1, "Xdialog"), stats, config);
                    }
                    else
                    {
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(2, "Xdialog"), stats, config);
                    }

                    if (totalCost > 0 && costPerAnimal > 0)
                    {
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(3, "Xdialog"), stats, config);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else if (gatheringOnly)
                {
                    message += DialogueManager.performReplacement(DialogueManager.getMessageAt(4, "Xdialog"), stats, config);

                    if (totalCost > 0 && costPerAnimal > 0)
                    {
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(3, "Xdialog"), stats, config);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else
                {
                    NPC character = Game1.getCharacterFromName(checker);
                    if (character != null)
                    {
                        //this.isCheckerCharacter = true;
                        string portrait = "";
                        if (character.name.Equals("Shane"))
                        {
                            portrait = "$8";
                        }

                        string spouseName = null;
                        if (Game1.player.isMarried())
                        {
                            spouseName = Game1.player.getSpouse().getName();
                        }

                        message += DialogueManager.performReplacement(DialogueManager.getRandomMessage("greeting"), stats, config);
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(5, "Xdialog"), stats, config);

                        if (costPerAnimal > 0)
                        {
                            if (doesPlayerHaveEnoughCash)
                            {
                                message += DialogueManager.performReplacement(DialogueManager.getMessageAt(6, "Xdialog"), stats, config);
                                shortDays = 0;
                            }
                            else
                            {
                                message += DialogueManager.performReplacement(DialogueManager.getRandomMessage("unfinishedmoney"), stats, config);
                            }
                        }
                        else
                        {

                            //message += portrait + "#$e#";
                        }

                        message += DialogueManager.performReplacement(DialogueManager.getRandomMessage("smalltalk"), stats, config);
                        message += portrait + "#$e#";

                        character.CurrentDialogue.Push(new Dialogue(message, character));
                        Game1.drawDialogue(character);
                    }
                    else
                    {
                        //message += checker + " has performed " + numActions + " for your animals.";
                        message += DialogueManager.performReplacement(DialogueManager.getMessageAt(7, "Xdialog"), stats, config);
                        HUDMessage msg = new HUDMessage(message);
                        Game1.addHUDMessage(msg);
                    }
                }
            }
            
        }

        private List<FarmAnimal> getAllFarmAnimals()
        {
            List<FarmAnimal> list = Game1.getFarm().animals.Values.ToList<FarmAnimal>();
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building.indoors != null && building.indoors.GetType() == typeof(AnimalHouse))
                    list.AddRange((IEnumerable<FarmAnimal>)((AnimalHouse)building.indoors).animals.Values.ToList<FarmAnimal>());
            }
            return list;
        }

    }
}
