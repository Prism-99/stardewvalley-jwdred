using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrabNet_REDUX.I18n
{
    internal static  class i18n
    {
        private static ITranslationHelper Translations;
        public static void Init(ITranslationHelper translations)
        {
            Translations = translations;
        }
        public static string Actionkey()
        {
            return GetByKey("actionkey");
        }
        public static string Actionkey_TT()
        {
            return GetByKey("actionkey.tt");
        }
        public static string Free()
        {
            return GetByKey("free");
        }
        public static string Free_TT()
        {
            return GetByKey("free.tt");
        }
        public static string CostPerCheck()
        {
            return GetByKey("costpercheck");
        }
        public static string CostPerCheck_TT()
        {
            return GetByKey("costpercheck.tt");
        }
        public static string CostPerEmpty()
        {
            return GetByKey("costperempty");
        }
        public static string CostPerEmpty_TT()
        {
            return GetByKey("costperempty.tt");
        }
        public static string ChargeForBait()
        {
            return GetByKey("chargeforbait");
        }
        public static string ChargeForBait_TT()
        {
            return GetByKey("chargeforbait.tt");
        }
        public static string PreferredBait()
        {
            return GetByKey("preferredbait");
        }
        public static string PreferredBait_TT()
        {
            return GetByKey("preferredbait.tt");
        }
        public static string WhoChecks()
        {
            return GetByKey("whochecks");
        }
        public static string WhoChecks_TT()
        {
            return GetByKey("whochecks.tt");
        }
        public static string EnableMessage()
        {
            return GetByKey("enablemessages");
        }
        public static string EnableMessage_TT()
        {
            return GetByKey("enablemessages.tt");
        }
        public static string ChestX()
        {
            return GetByKey("chestx");
        }
        public static string ChestX_TT()
        {
            return GetByKey("chestx.tt");
        }
        public static string ChestY()
        {
            return GetByKey("chesty");
        }
        public static string ChestY_TT()
        {
            return GetByKey("chesty.tt");
        }
        public static string BypassInventory()
        {
            return GetByKey("bypassinventory");
        }
        public static string BypassInventory_TT()
        {
            return GetByKey("bypassinventory.tt");
        }
        public static string AllowFreebies()
        {
            return GetByKey("allowfreebies");
        }
        public static string AllowFreebies_TT()
        {
            return GetByKey("allowfreebies.tt");
        }
        public static string EnableLogging()
        {
            return GetByKey("enablelogging");
        }
        public static string EnableLogging_TT()
        {
            return GetByKey("enablelogging.tt");
        }


        public static Translation GetByKey(string key, object tokens = null)
        {
            if (Translations == null)
                throw new InvalidOperationException($"You must call {nameof(i18n)}.{nameof(Init)} from the mod's entry method before reading translations.");
            return Translations.Get(key, tokens);
        }
    }
}
