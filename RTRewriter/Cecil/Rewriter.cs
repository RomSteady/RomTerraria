using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

namespace RTRewriter.Cecil
{
    public class Rewriter : IDisposable
    {
        private AssemblyDefinition terraria;
        private AssemblyDefinition hooks;
        private string assemblyOut;

        private const int NewMaxResolution = 8192;
        private const string NewConfigFileName = "config.rt";

        private System.IO.MemoryStream hidefprofilestream = null;

        public Rewriter(string assemblyIn, string assemblyOut)
        {
            terraria = AssemblyDefinition.ReadAssembly(assemblyIn);
            hooks = AssemblyDefinition.ReadAssembly("RTHooks.dll");

            foreach (var module in terraria.MainModule.AssemblyReferences)
            {
                if (module.Name.ToLower().Contains("rthooks"))
                {
                    throw new InvalidDataException("This copy of Terraria is already patched.  Validate your game cache and try again.");
                }
            }
            this.assemblyOut = assemblyOut;
        }

        public void Save()
        {
            terraria.Write(assemblyOut);
            if (hidefprofilestream != null)
            {
                hidefprofilestream.Close();
                hidefprofilestream = null;
            }
        }

        public void CondenseSteam()
        {
            /*
            // Condense the Kill method
            CecilHelpers.TurnMethodToNoOp(terraria, "System.Void Terraria.Steam::Kill()");

            // Handle the normal Steam checks
            //var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Steam::.cctor()");
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Steam::Init()");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldc_I4_1);
            processor.InsertBefore(firstInstruction, newInstruction);
            var nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Stsfld, CecilHelpers.FindFieldInType(method.DeclaringType, "SteamInit"));
            processor.InsertAfter(newInstruction, nextInstruction);
            var finalInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ret);
            processor.InsertAfter(nextInstruction, finalInstruction);
            */

            // To kill Steam, I need to find the Nullable:SocialMode constructor in
            // Terraria.Program.InternalMain and change it to "None," but let's try this without it.
        }

        public void FixConfigFileNames()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Boolean Terraria.Main::SaveSettings()");
            CecilHelpers.ReplaceStringInMethod(method, "config.dat", NewConfigFileName);

            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::OpenSettings()");
            CecilHelpers.ReplaceStringInMethod(method, "config.dat", NewConfigFileName);
        }

        public void ForceGravityGlobeOn()
        {
            var playerCtor = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Player::.ctor()");
            var processor = playerCtor.Body.GetILProcessor();
            var firstInstruction = playerCtor.Body.Instructions[0];
            var loadThis = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, loadThis);
            var loadTrue = processor.Create(Mono.Cecil.Cil.OpCodes.Ldc_I4_1);
            processor.InsertAfter(loadThis, loadTrue);
            var saveInGravControl = processor.Create(Mono.Cecil.Cil.OpCodes.Stfld, CecilHelpers.FindFieldInType(playerCtor.DeclaringType, "gravControl2"));
            processor.InsertAfter(loadTrue, saveInGravControl);

            var playerReset = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Player::ResetEffects()");
            CecilHelpers.ChangeDefaultBooleanValue(playerReset, "System.Boolean Terraria.Player::gravControl2", true);
        }

        public void UpMaxResolution()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::.cctor()");
            CecilHelpers.ChangeDefaultInt32Value(method, "System.Int32 Terraria.Main::maxScreenW", NewMaxResolution);
            CecilHelpers.ChangeDefaultInt32Value(method, "System.Int32 Terraria.Main::maxScreenH", NewMaxResolution);

            // Replace InitTargets()
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::InitTargets()");
            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.ScreenHelpers::Main_InitTargets(System.Object)");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, newInstruction);
            var nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(newInstruction, nextInstruction);
            var finalInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ret);
            processor.InsertAfter(nextInstruction, finalInstruction);

            // Inject Lighting Update At End Of ReleaseTargets()
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::ReleaseTargets()");
            hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.ScreenHelpers::Main_ReleaseTargets(System.Object)");
            processor = method.Body.GetILProcessor();
            firstInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];
            newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, newInstruction);
            nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(newInstruction, nextInstruction);

            // Inject ClientSizeChanged event into Initialize()
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::Initialize()");
            hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.ScreenHelpers::Main_Initialize(System.Object)");
            processor = method.Body.GetILProcessor();
            firstInstruction = method.Body.Instructions[0];
            newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, newInstruction);
            nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(newInstruction, nextInstruction);
        }

        public void CooperativeFullscreen()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::Initialize()");

            // We need to insert two hooks.  The first has to happen at the start of Initialize...
            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.ScreenHelpers::CooperativeFullScreenStep1(System.Object)");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, newInstruction);
            var nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(newInstruction, nextInstruction);

            // The first has to happen at the end of Initialize...
            var lastInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];
            hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.ScreenHelpers::CooperativeFullScreenStep2(System.Object)");
            newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(lastInstruction, newInstruction);
            nextInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(newInstruction, nextInstruction);
        }

        public void ChangeWorkingDirectory()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Program::LaunchGame(System.String[])");

            // We need to insert one hook
            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.StartupHelpers::ChangeWorkingDirectory()");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertBefore(firstInstruction, newInstruction);
        }

        public void LogReplacementTextureLoad()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "Microsoft.Xna.Framework.Graphics.Texture2D Terraria.Graphics.TextureManager::Load(System.String)");

            // We need to insert one hook
            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "Microsoft.Xna.Framework.Graphics.Texture2D RTHooks.ScreenHelpers::TextureManager_Load(System.String)");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];

            // Push the texture name object
            var pushTextureName = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, pushTextureName);

            // Call the hook
            var callInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(pushTextureName, callInstruction);

            // Return
            var finalInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ret);
            processor.InsertAfter(callInstruction, finalInstruction);
        }

        public void HookMain(bool alwaysDaylight)
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::Update(Microsoft.Xna.Framework.GameTime)");

            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.MainHooks::StartHook(System.Object,System.Boolean)");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];

            // Push the Main object
            var pushMain = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, pushMain);
            // Conditionally spawn NPCs
            var alwaysDaylightInstruction = processor.Create(alwaysDaylight ? Mono.Cecil.Cil.OpCodes.Ldc_I4_1 : Mono.Cecil.Cil.OpCodes.Ldc_I4_0);
            processor.InsertAfter(pushMain, alwaysDaylightInstruction);

            // Call the hook
            var callInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(alwaysDaylightInstruction, callInstruction);
        }

        public void EnableHiDefProfile()
        {
            EmbeddedResource hidefprofile = null;

            hidefprofilestream = new System.IO.MemoryStream();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(hidefprofilestream);
            sw.Write("Windows.v4.0.HiDef");
            sw.Flush();
            hidefprofilestream.Seek(0, System.IO.SeekOrigin.Begin);
            hidefprofile = new EmbeddedResource("Microsoft.Xna.Framework.RuntimeProfile",
                ManifestResourceAttributes.Private, hidefprofilestream);



            for (int i = 0; i < terraria.MainModule.Resources.Count; i++)
            {
                if (terraria.MainModule.Resources[i].Name == "Microsoft.Xna.Framework.RuntimeProfile")
                {
                    terraria.MainModule.Resources[i] = hidefprofile;
                }
            }
        }

        public void HookWorldGen(bool spawnNpcs)
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.WorldGen::UpdateWorld()");


            var hookMethod = CecilHelpers.FindMethodInAssembly(hooks, "System.Void RTHooks.WorldGenHooks::StartHook(System.Object,System.Boolean)");
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            // Push the worldgen object
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            processor.InsertBefore(firstInstruction, newInstruction);
            // Conditionally spawn NPCs
            var spawnNpcsInstruction = processor.Create(spawnNpcs ? Mono.Cecil.Cil.OpCodes.Ldc_I4_1 : Mono.Cecil.Cil.OpCodes.Ldc_I4_0);
            processor.InsertAfter(newInstruction, spawnNpcsInstruction);
            // Call the hook
            var callInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(hookMethod));
            processor.InsertAfter(spawnNpcsInstruction, callInstruction);
        }

        private void SwapLeftRightInMethod(MethodDefinition method)
        {
            var xnaAssembly = Assembly.Load(new AssemblyName("Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553"));
            var xna = AssemblyDefinition.ReadAssembly(xnaAssembly.Location);
            var leftButton = CecilHelpers.FindMethodInAssembly(xna, "Microsoft.Xna.Framework.Input.ButtonState Microsoft.Xna.Framework.Input.MouseState::get_LeftButton()");
            var rightButton = CecilHelpers.FindMethodInAssembly(xna, "Microsoft.Xna.Framework.Input.ButtonState Microsoft.Xna.Framework.Input.MouseState::get_RightButton()");
            var processor = method.Body.GetILProcessor();
            foreach (var inst in method.Body.Instructions)
            {
                if (inst.OpCode == Mono.Cecil.Cil.OpCodes.Call)
                {
                    if (inst.Operand.ToString() == leftButton.ToString())
                    {
                        inst.Operand = method.Module.Import(rightButton);
                    }
                    else if (inst.Operand.ToString() == rightButton.ToString())
                    {
                        inst.Operand = method.Module.Import(leftButton);
                    }
                }
            }
        }

        public void SwapLeftRightMouseButtons()
        {
            SwapLeftRightInMethod(CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.UI.UserInterface::ResetState()"));
            SwapLeftRightInMethod(CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.UI.UserInterface::Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.GameTime)"));
            SwapLeftRightInMethod(CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::Update(Microsoft.Xna.Framework.GameTime)"));
        }

        protected virtual void Dispose(Boolean disposeNative)
        {
            try
            {
                if (hidefprofilestream != null)
                {
                    hidefprofilestream.Close();
                }
            }
            catch (IOException)
            {
                // Ignoring.  Means everything is already closed.
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void DisableAchievements()
        {
            CecilHelpers.TurnMethodToNoOp(terraria, "System.Void Terraria.Achievements.Achievement::OnConditionComplete(Terraria.Achievements.AchievementCondition)");
        }
    }
}
