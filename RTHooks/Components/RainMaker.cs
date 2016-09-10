using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RTHooks.Components
{
    class RainMaker : GameComponent
    {
        TimeSpan elapsedTime = TimeSpan.Zero;
        Random random = new Random();
        TimeSpan tick = TimeSpan.FromMilliseconds(100);
        const int topOfWorld = 5;

        Assembly terrariaAssembly;
        Type worldGen;
        Type main;
        
        public RainMaker(Game game) : base(game)
        {
            terrariaAssembly = Assembly.GetAssembly(game.GetType());
            main = terrariaAssembly.GetType("Terraria.Main");
            worldGen = terrariaAssembly.GetType("Terraria.WorldGen");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            elapsedTime = elapsedTime.Add(gameTime.ElapsedGameTime);
            while (elapsedTime > tick)
            {
                elapsedTime = elapsedTime.Subtract(tick);
                // TODO: If we're in single player, generate a "rain" tile every tick
                // Old code:
                /*
                for (int x = (int)(Terraria.Main.screenPosition.X / 16) - 100;
                     x < (Terraria.Main.screenPosition.X / 16) + (Terraria.Main.screenWidth / 16) + 100;
                     x++)
                {
                    if (x >= 0 && x <= Terraria.Main.maxTilesX && random.Next(200) == 0)
                    {
                        if (Terraria.Main.tile[x, y] == null)
                            Terraria.Main.tile[x, y] = new Terraria.Tile();
                        Terraria.Main.tile[x, y].liquid = (byte)random.Next(8);
                        Terraria.Main.tile[x, y].lava = true;
                        Terraria.Liquid.AddWater(x, y);

                    }
                }
                */
            }
        }
    }
}
