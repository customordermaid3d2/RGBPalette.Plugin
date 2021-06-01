using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using HarmonyLib;

namespace RGBPalette.Plugin
{
	class Patcher
	{

		/// <summary>
		/// 원본
		/// </summary>
		/// <param name="ass"></param>
		public static void Patch(AssemblyDefinition ass)
		{
			TypeDefinition type = AssemblyDefinition.ReadAssembly(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "./UnityInjector/RGBPalette.Plugin.dll")).MainModule.GetType("RGBPalette.Plugin.RGBPalette");
			MethodDefinition method = type.Methods.First((MethodDefinition m) => m.Name == "OnPaletteOpen");
			MethodDefinition method2 = type.Methods.First((MethodDefinition m) => m.Name == "OnPaletteClose");
			TypeDefinition type2 = ass.MainModule.GetType("ColorPaletteManager");
			MethodDefinition methodDefinition = type2.Methods.First((MethodDefinition m) => m.Name == "Call");
			MethodDefinition methodDefinition2 = type2.Methods.First((MethodDefinition m) => m.Name == "Close");
			ILProcessor ilprocessor = methodDefinition.Body.GetILProcessor();
			Instruction target = methodDefinition.Body.Instructions.First<Instruction>();
			ilprocessor.InsertBefore(target, ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.InsertBefore(target, ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.InsertBefore(target, ilprocessor.Create(OpCodes.Ldfld, type2.Fields.First((FieldDefinition f) => f.Name == "uiManager")));
			ilprocessor.InsertBefore(target, ilprocessor.Create(OpCodes.Call, ass.MainModule.ImportReference(method)));
			ilprocessor = methodDefinition2.Body.GetILProcessor();
			target = methodDefinition2.Body.Instructions.First<Instruction>();
			ilprocessor.InsertBefore(target, ilprocessor.Create(OpCodes.Call, ass.MainModule.ImportReference(method2)));
		}


		[HarmonyPatch(typeof(ColorPaletteManager), "Call")]
		[HarmonyPostfix]
		//public void Call(int maidNo, MaidParts.PARTS_COLOR colorType)
		public static void Call(ColorPaletteManager __instance, int maidNo, MaidParts.PARTS_COLOR colorType, ColorPaletteUIManager ___uiManager)
		{
			RGBPalette.OnPaletteOpen(__instance, ___uiManager);
		}


		
		[HarmonyPatch(typeof(ColorPaletteManager), "Close")]
		[HarmonyPostfix]
		public static void Start(ColorPaletteManager __instance)
		{
			RGBPalette.OnPaletteClose();
		}




	}
}
