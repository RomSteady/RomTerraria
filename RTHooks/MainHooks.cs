using System;
using System.Reflection;

namespace RTHooks
{
    public static class MainHooks
    {
        public static void StartHook(object terrariaMain, bool alwaysDaytime)
        {
            var terrariaAssembly = Assembly.GetAssembly(terrariaMain.GetType());
            Type gameType = terrariaAssembly.GetType("Terraria.Main");

            FieldInfo netMode = gameType.GetField("netMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo gameMenu = gameType.GetField("gameMenu", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (!Convert.ToBoolean(gameMenu.GetValue(null))) // Don't run in menu
            {
                if (Convert.ToInt32(netMode.GetValue(null)) == 0) // Only run hooks in single player
                {
                    if (alwaysDaytime)
                    {
                        FieldInfo time = gameType.GetField("time", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        time.SetValue(null, (double)32401);
                        FieldInfo dayTime = gameType.GetField("dayTime", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        dayTime.SetValue(null, true);
                    }
                }
            }
        }
    }
}
