using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace RTHooks
{
    public static class ScreenHelpers
    {
        #region Cooperative Full Screen Mode

        public static void CooperativeFullScreenStep1(Object terrariaMain)
        {
            var terrariaAssembly = Assembly.GetAssembly(terrariaMain.GetType());
            Type gameType = terrariaAssembly.GetType("Terraria.Main");


            FieldInfo screenHeight = gameType.GetField("screenHeight");
            FieldInfo screenWidth = gameType.GetField("screenWidth");
            FieldInfo toggleFullscreen = gameType.GetField("toggleFullscreen");

            screenHeight.SetValue(terrariaMain, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            screenWidth.SetValue(terrariaMain, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            toggleFullscreen.SetValue(terrariaMain, false);
        }

        public static void CooperativeFullScreenStep2(Object terrariaMain)
        {
            Game g = (Game)terrariaMain;
            System.Windows.Forms.Form mainForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(g.Window.Handle);

            mainForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            mainForm.Location = new System.Drawing.Point(0, 0);
            mainForm.TopLevel = true;
        }

        public static void LaunchDebugger(Object exception)
        {
            System.Diagnostics.Debugger.Break();
        }

        #endregion

        #region Replacement Functions

        private static Assembly m_terrariaAssembly = null;
        private static Game m_terrariaMain = null;
        private static StreamWriter m_logger = null;

        public static void Main_Initialize(object terrariaMain)
        {
            var terrariaAssembly = Assembly.GetAssembly(terrariaMain.GetType());
            m_terrariaAssembly = terrariaAssembly;
            Type gameType = terrariaAssembly.GetType("Terraria.Main");
            Game game = (Game)terrariaMain;
            m_terrariaMain = game;
            MethodInfo releaseTargets = gameType.GetMethod("ReleaseTargets");

            var savepathfield = gameType.GetField("SavePath");
            string savepath = savepathfield.GetValue(game) as string;
            m_logger = new StreamWriter(Path.Combine(savepath, "RomTerrariaDebug.txt"), true);
            m_logger.AutoFlush = true;
            m_logger.WriteLine("=====");

            game.Content.RootDirectory = Path.Combine(StartupHelpers.GetTerrariaFolder(), "Content");
            //game.Content = new LoggingContentManager(game.Content);

            game.GraphicsDevice.DeviceLost += new EventHandler<EventArgs>((o, e) =>
            {
                //ClearAllContent(terrariaMain);
                //releaseTargets.Invoke(terrariaMain, null);
            });
            game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>((o, e) =>
            {
                //ClearAllContent(terrariaMain);
                Main_InitTargets(terrariaMain);
            });
            game.Window.ClientSizeChanged += new EventHandler<EventArgs>((o, e) =>
            {
                ClearAllContent(terrariaMain);
                Main_InitTargets(terrariaMain);
            });
        }

        private static void ClearBooleanArray(object terrariaMain, Type gameType, string fieldName)
        {
            FieldInfo array = gameType.GetField(fieldName);
            var boolArray = array.GetValue(terrariaMain) as bool[];
            if (boolArray == null)
            {
                Type t = terrariaMain.GetType();
                throw new ArgumentException(
                    String.Format("Can't find field {0} in {1}", fieldName, t.AssemblyQualifiedName),
                    "fieldName");
            }
            for (int i = 0; i <= boolArray.GetUpperBound(0); i++)
            {
                boolArray[i] = false;
            }
            array.SetValue(terrariaMain, boolArray);
        }

        private static void ClearAllContent(object terrariaMain)
        {
            var terrariaAssembly = Assembly.GetAssembly(terrariaMain.GetType());
            Type gameType = terrariaAssembly.GetType("Terraria.Main");

            ClearBooleanArray(terrariaMain, gameType, "hairLoaded");
            ClearBooleanArray(terrariaMain, gameType, "wingsLoaded");
            ClearBooleanArray(terrariaMain, gameType, "goreLoaded");
            ClearBooleanArray(terrariaMain, gameType, "projectileLoaded");
            ClearBooleanArray(terrariaMain, gameType, "itemFlameLoaded");
            ClearBooleanArray(terrariaMain, gameType, "backgroundLoaded");
            ClearBooleanArray(terrariaMain, gameType, "tileSetsLoaded");
            ClearBooleanArray(terrariaMain, gameType, "wallLoaded");
            ClearBooleanArray(terrariaMain, gameType, "NPCLoaded");
            ClearBooleanArray(terrariaMain, gameType, "armorHeadLoaded");
            ClearBooleanArray(terrariaMain, gameType, "armorBodyLoaded");
            ClearBooleanArray(terrariaMain, gameType, "armorLegsLoaded");

            // 1.3
            ClearBooleanArray(terrariaMain, gameType, "accBackLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accFaceLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accNeckLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accFrontLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accShoesLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accWaistLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accShieldLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accballoonLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accHandsOnLoaded");
            ClearBooleanArray(terrariaMain, gameType, "accHandsOffLoaded");

            FieldInfo tilesLoaded = gameType.GetField("tilesLoaded");
            tilesLoaded.SetValue(terrariaMain, false);
        }

        private static int MaxTextureSize = 8192;

        public static Texture2D TextureManager_Load(string name)
        {
            Type textureManager = m_terrariaAssembly.GetType("Terraria.Graphics.TextureManager");
            FieldInfo textures = textureManager.GetField("_textures", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo blankTexture = textureManager.GetField("BlankTexture", BindingFlags.Public | BindingFlags.Static);
            Texture2D blank = blankTexture.GetValue(textureManager) as Texture2D;
            ConcurrentDictionary<string, Texture2D> _textures = textures.GetValue(textureManager) as ConcurrentDictionary<string, Texture2D>;
            if (_textures != null)
            {
                if (_textures.ContainsKey(name))
                {
                    return _textures[name];
                }

                if (!String.IsNullOrWhiteSpace(name))
                {
                    try
                    {
                        blank = m_terrariaMain.Content.Load<Texture2D>(name);
                        _textures[name] = blank;
                        return blank;
                    }
                    catch (Exception ex)
                    {
                        m_logger.WriteLine("Exception loading Texture2D " + name + ": " + ex.GetType().ToString() + ": " + ex.Message);
                        return blank;
                    }
                }
                else
                {
                    return blank;
                }
            }
            return blank;
        }

        public static void Main_InitTargets(object terrariaMain)
        {
            Game game = (Game)terrariaMain;

            // Let's see if we can't up the maximum texture size a bit
            Assembly xna = Assembly.GetAssembly(typeof(GraphicsProfile));
            Type profileCapabilities = xna.GetType("Microsoft.Xna.Framework.Graphics.ProfileCapabilities", true);
            if (profileCapabilities != null)
            {
                FieldInfo maxTextureSize = profileCapabilities.GetField("MaxTextureSize", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo hidefProfile = profileCapabilities.GetField("HiDef", BindingFlags.Static | BindingFlags.NonPublic);
                if (maxTextureSize != null && hidefProfile != null)
                {
                    System.Diagnostics.Debug.WriteLine("Upping MaxTextureSize in GraphicsProfile");
                    object profile = hidefProfile.GetValue(null);
                    maxTextureSize.SetValue(hidefProfile.GetValue(null), MaxTextureSize);
                }
            }

            var terrariaAssembly = Assembly.GetAssembly(terrariaMain.GetType());
            Type gameType = terrariaAssembly.GetType("Terraria.Main");

            FieldInfo dedServ = gameType.GetField("dedServ");
            FieldInfo offScreenRange = gameType.GetField("offScreenRange"); // Made public in 1.2.3.1
            FieldInfo targetSet = gameType.GetField("targetSet");
            FieldInfo waterTarget = gameType.GetField("waterTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo backWaterTarget = gameType.GetField("backWaterTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo blackTarget = gameType.GetField("blackTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo tileTarget = gameType.GetField("tileTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo tile2Target = gameType.GetField("tile2Target", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo wallTarget = gameType.GetField("wallTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo backgroundTarget = gameType.GetField("backgroundTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo screenTarget = gameType.GetField("screenTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo spriteBatch = gameType.GetField("spriteBatch"); // Made public static in 1.2.3.1 
            FieldInfo drawToScreen = gameType.GetField("drawToScreen"); // New field in 1.3.x

            Type lightingType = terrariaAssembly.GetType("Terraria.Lighting");

            FieldInfo lightMode = lightingType.GetField("lightMode");

            // Force a reload of all content
            ClearAllContent(terrariaMain);

            int _offScreenRange = 192;

            try
            {
                if (!((bool)dedServ.GetValue(terrariaMain)))
                {
                    int width = game.GraphicsDevice.PresentationParameters.BackBufferWidth + (_offScreenRange * 2);
                    int height = game.GraphicsDevice.PresentationParameters.BackBufferHeight + (_offScreenRange * 2);

                    if (width > MaxTextureSize)
                    {
                        width = MaxTextureSize;
                        _offScreenRange = Math.Min(_offScreenRange, width - game.GraphicsDevice.PresentationParameters.BackBufferWidth);
                    }

                    if (height > MaxTextureSize)
                    {
                        height = MaxTextureSize;
                        _offScreenRange = Math.Min(_offScreenRange, height - game.GraphicsDevice.PresentationParameters.BackBufferWidth);
                    }

                    SetRenderTarget(terrariaMain, waterTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, backWaterTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, blackTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, tileTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, tile2Target, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, wallTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, backgroundTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));
                    SetRenderTarget(terrariaMain, screenTarget, new RenderTarget2D(game.GraphicsDevice, width, height, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24));

                    targetSet.SetValue(terrariaMain, true);
                    drawToScreen.SetValue(terrariaMain, false);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions for troubleshooting
                var savepathfield = gameType.GetField("SavePath");
                string savepath = savepathfield.GetValue(game) as string;
                if (savepath != null)
                {
                    using (StreamWriter sw = new StreamWriter(Path.Combine(savepath, "CrashInInitTargets.txt")))
                    {
                        sw.WriteLine("RomTerraria - Crash In InitTargets");
                        sw.WriteLine(ex.GetType().ToString());
                        sw.WriteLine(ex.Message);
                        sw.WriteLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            sw.WriteLine("Inner Exception:");
                            sw.WriteLine(ex.InnerException.GetType().ToString());
                            sw.WriteLine(ex.Message);
                            sw.WriteLine(ex.StackTrace);
                        }
                        sw.Flush();
                    }
                }

                MethodInfo releaseTargets = gameType.GetMethod("ReleaseTargets");
                releaseTargets.Invoke(terrariaMain, null);
                lightMode.SetValue(null, 2);
                drawToScreen.SetValue(terrariaMain, true);
            }

            offScreenRange.SetValue(game, _offScreenRange);
        }

        // Safely swap render targets
        private static void SetRenderTarget(object o, FieldInfo field, RenderTarget2D target)
        {
            RenderTarget2D oldTex = field.GetValue(o) as RenderTarget2D;
            field.SetValue(o, target);
            if (oldTex != null && oldTex.IsDisposed == false)
            {
                oldTex.Dispose();
            }
        }

        public static void Main_ReleaseTargets(object terrariaMain)
        {
            ClearAllContent(terrariaMain);
        }
        #endregion
    }
}
