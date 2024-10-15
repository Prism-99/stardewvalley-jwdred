using CrabNet.Framework;
using CustomPictureFrames;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using CrabNet_REDUX.I18n;

namespace CrabNet_REDUX.GMCM
{
    internal class GMCMIntegration
    {
        public static void SetupMenu(IModHelper Helper, IManifest ModManifest, ModConfig Config)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddKeybind(
               mod: ModManifest,
               name: () => i18n.Actionkey(),
               tooltip: () => i18n.Actionkey_TT(),
               getValue: () => Config.KeyBind,
               setValue: value => Config.KeyBind = value
           );

            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => i18n.Free(),
               tooltip: () => i18n.Free_TT(),
               getValue: () => Config.Free,
               setValue: value => Config.Free = value
           );

            configMenu.AddNumberOption(
               mod: ModManifest,
               name: () => i18n.CostPerCheck(),
               tooltip: () => i18n.CostPerCheck_TT(),
               getValue: () => Config.CostPerCheck,
               setValue: value => Config.CostPerCheck = Math.Max(0, value)
           );
            configMenu.AddNumberOption(
               mod: ModManifest,
               name: () => i18n.CostPerEmpty(),
               tooltip: () => i18n.CostPerEmpty_TT(),
               getValue: () => Config.CostPerEmpty,
               setValue: value => Config.CostPerEmpty = Math.Max(0, value)
         );
            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => i18n.ChargeForBait(),
               tooltip: () =>i18n.ChargeForBait_TT(),
               getValue: () => Config.ChargeForBait,
               setValue: value => Config.ChargeForBait = value
        );
            configMenu.AddTextOption(
               mod: ModManifest,
               name: () => i18n.PreferredBait(),
               tooltip: () => i18n.PreferredBait_TT(),
               getValue: () => Config.PreferredBait,
               setValue: value => Config.PreferredBait = value
          );
            configMenu.AddTextOption(
               mod: ModManifest,
               name: () => i18n.WhoChecks(),
               tooltip: () => i18n.WhoChecks_TT(),
               getValue: () => Config.WhoChecks,
               setValue: value => Config.WhoChecks = value
          );
            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => i18n.EnableMessage(),
               tooltip: () => i18n.EnableMessage_TT(),
               getValue: () => Config.EnableMessages,
               setValue: value => Config.EnableMessages = value
        );
            configMenu.AddNumberOption(
              mod: ModManifest,
              name: () => i18n.ChestX(),
              tooltip: () => i18n.ChestX_TT(),
              getValue: () => (int)Config.ChestCoords.X,
              setValue: value => Config.ChestCoords = new Vector2(  value, Config.ChestCoords.Y)
        );
            configMenu.AddNumberOption(
              mod: ModManifest,
              name: () => i18n.ChestY(),
              tooltip: () => i18n.ChestY_TT(),
              getValue: () => (int)Config.ChestCoords.Y,
              setValue: value => Config.ChestCoords = new Vector2(Config.ChestCoords.X,value)
        );
            configMenu.AddBoolOption(
              mod: ModManifest,
              name: () => i18n.BypassInventory(),
              tooltip: () => i18n.BypassInventory_TT(),
              getValue: () => Config.BypassInventory,
              setValue: value => Config.BypassInventory = value
       );
            configMenu.AddBoolOption(
              mod: ModManifest,
              name: () => i18n.AllowFreebies(),
              tooltip: () => i18n.AllowFreebies_TT(),
              getValue: () => Config.AllowFreebies,
              setValue: value => Config.AllowFreebies = value
       );
            configMenu.AddBoolOption(
              mod: ModManifest,
              name: () => i18n.EnableLogging(),
              tooltip: () => i18n.EnableLogging_TT(),
              getValue: () => Config.EnableLogging,
              setValue: value => Config.EnableLogging = value
       );
        }
    }
}
