using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace CrabNet.Framework
{
    internal class ModConfig
    {
        // The hot key that performs this action.
        public SButton KeyBind { get; set; } = SButton.H;

        // Whether or not logging is enabled.  If set to true, then debugging log entries will be output to the SMAPI console.
        public bool EnableLogging { get; set; }

        // This overrides all cost settings.  If this is set to true, there will be no costs associated with using the command.
        public bool Free { get; set; }

        // The cost associated with checking (visiting) a crab pot.  This will be assessed regardless of whether the pot was emptied or baited.
        public int CostPerCheck { get; set; } = 1;

        // The cost associated with emptying the pot.  This is only assessed if there is something to remove.
        public int CostPerEmpty { get; set; } = 10;

        // Whether the user wants to be charged for bait.
        public bool ChargeForBait { get; set; } = true;

        public int BaitCost
        {
            get
            {
                if (string.IsNullOrEmpty(PreferredBait))
                {
                    return -1;
                }
                else
                {
                    if (Game1.objectData !=null && Game1.objectData.TryGetValue(PreferredBait, out var bait))
                    {
                        return bait.Price;
                    }
                    return -1;
                }
            }
            set { }
        }


        // The ID of the users preferred bait (regular, or wild)
        public string PreferredBait { get; set; } = "685";

        // The name of the person who is performing the checks.  'spouse' and character names wil result in interaction.  Setting it to anything else will display that sting in all messages.
        public string WhoChecks { get; set; } = "spouse";

        // Whether to display HUD messages and dialog.  Not to be confused with the logging setting.
        public bool EnableMessages { get; set; } = true;

        // The X, Y coordinates of a chest, into which surplus items can be deposited.  The farmers inventory will be tried first.
        public Vector2 ChestCoords { get; set; } = new Vector2(73, 14);

        // Whether to bypass the user's inventory and try depositing to the chest first.  Will fall back to the inventory if no chest is present.
        public bool BypassInventory { get; set; }

        // Whether the mod will be lenient about not having enough cash to complete the transaction.  If set to false, the worker will not do work the farmer cannot afford.
        public bool AllowFreebies { get; set; } = true;
    }
}
