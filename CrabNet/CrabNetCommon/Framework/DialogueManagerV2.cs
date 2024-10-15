using CrabNet.Framework;
using CrabNet_REDUX.I18n;

namespace CrabNet_REDUX.Framework
{
    //
    //  Skeleton replacement of the old DialogueManager
    //
    internal static class DialogueManagerV2
    {
        public static string PerformReplacement(string message, CrabNetStats stats, ModConfig config)
        {
            int index = message.IndexOf("%%");
            while (index > -1)
            {
                int end = message.IndexOf("%%", index + 2);
                if (end > -1)
                {
                    string paramName = message.Substring(index + 2, end - index - 2);
                    if (stats.GetFields().ContainsKey(paramName))
                    {
                        message = message.Replace($"%%{paramName}%%", stats.GetFields()[paramName].ToString());
                        index = message.IndexOf("%%");
                    }
                    else
                    {
                        index = message.IndexOf("%%", end + 2);
                    }
                }
                else
                {
                    index = -1;
                }
            }
            return message;
        }
        public static Dictionary<int, string> GetDialogue(string prefix)
        {
            Dictionary<int, string> dialogue = new();
            Dictionary<string, int> prefixCounts = new Dictionary<string, int>
            {
                {"Xdialog",6 },
                {"greeting",6 },
                {"unfinishedmoney",8 },
                {"freebies",8 },
                {"unfinishedinventory",3 },
                {"smalltalk",14 },
                {"Shane",2 },
                {"Haley",2 },
                {"Willy",1 },
                {"Leah",1 }
            };
            if (prefixCounts.TryGetValue(prefix, out int count))
            {
                for (int index = 0; index < count; index++)
                {
                    dialogue.Add(index, i18n.GetByKey($"{prefix}_{index + 1}"));
                }
            }

            return dialogue;
        }
    }
}
