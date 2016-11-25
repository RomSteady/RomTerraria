using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RTRewriter.Cecil
{
    public class Rewriter : IDisposable
    {
        private AssemblyDefinition terraria;
        private AssemblyDefinition hooks;
        private AssemblyDefinition liquid;
        private string assemblyOut;

        private const int NewMaxResolution = 8192;
        private const string NewConfigFileName = "config.rt";

        private System.IO.MemoryStream hidefprofilestream = null;

        public Rewriter(string assemblyIn, string assemblyOut)
        {
            terraria = AssemblyDefinition.ReadAssembly(assemblyIn);
            hooks = AssemblyDefinition.ReadAssembly("RTHooks.dll");
            liquid = AssemblyDefinition.ReadAssembly("ReplacementLiquidRenderer.dll");

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

        public void AddRainComponent()
        {
            //throw new NotImplementedException();
        }

        public void DoubleUISize()
        {
            var mainType = CecilHelpers.FindTypeInAssembly(terraria, "Terraria.Main");
            var inventoryScaleField = CecilHelpers.FindFieldInType(mainType, "inventoryScale");
            var reforgeScaleField = CecilHelpers.FindFieldInType(mainType, "reforgeScale");
            var inventoryMethod = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::DrawInventory()");

            var processor = inventoryMethod.Body.GetILProcessor();
            // There are multiple locations inside DrawInventory where it sets the scale of the inventory draw.  Let's just override them.

            var addTimesTwoAfter = new List<Mono.Cecil.Cil.Instruction>();
            var replaceOperations = new Dictionary<Mono.Cecil.Cil.Instruction, int>();
           

            foreach (var instruction in inventoryMethod.Body.Instructions)
            {
                if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Stsfld)
                {
                    if (instruction.Operand == inventoryScaleField ||
                        instruction.Operand == reforgeScaleField)
                    {
                        // Calculated scale...let's fix that later
                        addTimesTwoAfter.Add(instruction.Previous);
                    }
                }
          
                else if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4 || 
                         instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_R4 ||
                         instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_S)
                {
                    // Layout constants - sometimes, they're floats, other times, they're ints.  Treat them the same.
                  
                    int value = Convert.ToInt32(instruction.Operand);

                    switch(value)
                    {

                        // Coins/ammo
                        case 496:
                        case 497:
                        case 498:
                            value = 1150;
                            break;
                        case 532:
                        case 533:
                        case 534:
                            value = 1275;
                            break;
                        
                        // Sort/deposit icons
                        case 244:
                            value = 580;
                            break;

                        // Trash
                        case 448:
                            value = 1028;
                            break;
                        case 258:
                            value = 580;
                            break;

                        // Equipment
                        case -47:
                            value = -100;
                            break;
                        case 47:
                            value = 100;
                            break;

                        // Crafting menu
                        case 42:
                            value = 100;
                            break;

                        // Below here, generically double
                        case 168:
                            //case 85:
                            //case 54:
                            // Equipment
                            //case 174:


                            /*
                        case 92:
                        case 32:
                        case 440:
                        */
                            /*
                            // case 58: // Don't double 58 because it slides too many things off screen
                            // case 56: // Don't double 56 because it screws up all other layouts

                            case 64:
                            // Buffs:
                            case 46:
                            case 260:

                            // Crafting / Recipes
                            case 118:
                            case 73:
                            case 331:
                            case 26:
                            case 50:
                            case 42:
                            case 150:
                            case 94:
                            case 450:
                            case 340:
                            case 310:
                            case 280:
                            //case 20:
                            //case 40:
                            //case 80:
                            case 380:
                            // UI Elements
                            case 302:
                            //case 30:
                            */
                            value *= 2;
                            break;
                        default:
                            break; // We don't recognize the value, so we won't double it.
                    }


                    if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4)
                    {
                        instruction.Operand = value;
                    }
                    else if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_S)
                    {
                        if (value >= -128 && value <= 127)
                        {
                            instruction.Operand = (sbyte)value;
                        }
                        else
                        {
                            //replaceOperations.Add(instruction, value);
                        }
                    }
                    else
                    {
                        instruction.Operand = (float)value;
                    }
                }
               
            }

            foreach (var instruction in replaceOperations.Keys)
            {
                var replacementInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldc_I4, replaceOperations[instruction]);
                processor.Replace(instruction, replacementInstruction);
            }

            foreach (var instruction in addTimesTwoAfter)
            {
                var insertTwoInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ldc_R4, 2.0f);
                processor.InsertAfter(instruction, insertTwoInstruction);
                var multiplyInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Mul);
                processor.InsertAfter(insertTwoInstruction, multiplyInstruction);

            }
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
            CecilHelpers.ChangeDefaultInt32Value(method, "System.Int32 Terraria.Main::_renderTargetMaxSize", NewMaxResolution);

            //DEBUG
            CecilHelpers.ChangeDefaultBooleanValue(method, "System.Boolean Terraria.Main::SkipAssemblyLoad", true);

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

        private Dictionary<string, MethodDefinition> GetMethodMap(TypeDefinition typ)
        {
            var ret = new Dictionary<string, MethodDefinition>();
            foreach(var method in typ.Methods)
            {
                var splitName = method.FullName.Split(new string[] { "::" }, StringSplitOptions.None);
                if (splitName.Length == 2)
                {
                    ret[splitName[1]] = method;
                }
            }
            return ret;
        }

        private void ReplaceLiquidRendererInMethod(MethodDefinition method)
        {
            var oldType = CecilHelpers.FindTypeInAssembly(terraria, "Terraria.GameContent.Liquid.LiquidRenderer");
            var oldField = CecilHelpers.FindFieldInType(oldType, "Instance");
            var newType = CecilHelpers.FindTypeInAssembly(liquid, "Terraria.GameContent.Liquid.ReplacementLiquidRenderer");
            var newField = CecilHelpers.FindFieldInType(newType, "Instance");
            var newMap = GetMethodMap(newType);

            var processor = method.Body.GetILProcessor();

            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                var inst = method.Body.Instructions[i];
                if (inst.OpCode == Mono.Cecil.Cil.OpCodes.Call ||
                    inst.OpCode == Mono.Cecil.Cil.OpCodes.Calli ||
                    inst.OpCode == Mono.Cecil.Cil.OpCodes.Callvirt)
                {
                    var methodCall = inst.Operand as MethodDefinition;
                    if (methodCall != null && methodCall.FullName.Contains("Terraria.GameContent.Liquid.LiquidRenderer"))
                    {
                        var splitName = methodCall.FullName.Split(new string[] { "::" }, StringSplitOptions.None);
                        if (splitName.Length == 2 && newMap.ContainsKey(splitName[1]))
                        {
                            var newMethod = newMap[splitName[1]];
                            inst.Operand = method.Module.Import(newMethod);
                            method.Body.Instructions[i] = inst;
                        }
                    }
                } else if (inst.OpCode == Mono.Cecil.Cil.OpCodes.Ldsfld)
                {
                    if (inst.Operand == oldField)
                    {
                        inst.Operand = method.Module.Import(newField);
                        method.Body.Instructions[i] = inst;
                    }
                }
            }
        }

        // Required for v1.3.4.3 only so far
        public void ReplaceLiquidRenderer()
        {
            var method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::DoUpdate(Microsoft.Xna.Framework.GameTime)");
            ReplaceLiquidRendererInMethod(method);
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::DrawCapture(Microsoft.Xna.Framework.Rectangle,Terraria.Graphics.Capture.CaptureSettings)");
            ReplaceLiquidRendererInMethod(method);
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::drawWaters(System.Boolean,System.Int32,System.Boolean)");
            ReplaceLiquidRendererInMethod(method);
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::DrawWater(System.Boolean,System.Int32,System.Single)");
            ReplaceLiquidRendererInMethod(method);
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.Main::DrawBlack(System.Boolean)");
            ReplaceLiquidRendererInMethod(method);
            method = CecilHelpers.FindMethodInAssembly(terraria, "System.Void Terraria.GameContent.Shaders.WaterShaderData::StepLiquids()");
            ReplaceLiquidRendererInMethod(method);
        }
    }
}
