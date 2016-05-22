
using System;
using System.Reflection;
namespace RTHooks
{
    public static class WorldGenHooks
    {
        public static void StartHook(object worldgen, bool keepNpcsSpawning)
        {
            var terrariaAssembly = Assembly.GetAssembly(worldgen.GetType());
            Type worldGenType = terrariaAssembly.GetType("Terraria.WorldGen");
            Type gameType = terrariaAssembly.GetType("Terraria.Main");

            FieldInfo netMode = gameType.GetField("netMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo gameMenu = gameType.GetField("gameMenu", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (!Convert.ToBoolean(gameMenu.GetValue(null))) // Don't run in menu
            {
                if (Convert.ToInt32(netMode.GetValue(null)) == 0) // Only run hooks in single player
                {
                    if (keepNpcsSpawning)
                    {
                        FieldInfo spawnDelay = worldGenType.GetField("spawnDelay", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        spawnDelay.SetValue(worldgen, Int32.MaxValue);
                    }
                }
            }
        }
    }
}
