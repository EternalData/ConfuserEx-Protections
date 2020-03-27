
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Confuser.Protections
{
	public class RandomOutLineMethods : Protection
	{
		public override ProtectionPreset Preset => ProtectionPreset.Minimum;

		public override string Name => "Random OutLine Methods ";

		public override string Description => "Adds random outlines";

		public override string Id => "Ki.OutLine";

		public override string FullId => "Ki.Outline";

		protected override void Initialize(ConfuserContext context) { }

		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPreStage(PipelineStage.WriteModule, new RandomOutLineMethodsPhase(this));
		}

		private class RandomOutLineMethodsPhase : ProtectionPhase
		{
			public RandomOutLineMethodsPhase(RandomOutLineMethods parent) : base(parent) { }

			public override ProtectionTargets Targets => ProtectionTargets.Modules;

			public override string Name => "Anti De4Dot";

			protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
			{
				foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
				
					foreach (var type in module.Types)
				{
					foreach (var method in type.Methods.ToArray())
					{
						MethodDef strings = CreateReturnMethodDef(RandomString(), method);
						MethodDef ints = CreateReturnMethodDef(RandomInt(), method);
						type.Methods.Add(strings);
						type.Methods.Add(ints);
					}
				}
			}
			public static string RandomString()
			{
				const string chars = "abcdefghijklmnopqrstuv!@#$%^&*()/[]";
				return new string(Enumerable.Repeat(chars, 10)
					.Select(s => s[new Random(Guid.NewGuid().GetHashCode()).Next(s.Length)]).ToArray());
			}
			public static int RandomInt()
			{
				var ints = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
				return new Random(ints).Next(0, 99999999);
			}
			private static MethodDef CreateReturnMethodDef(object value, MethodDef source_method)
			{
				CorLibTypeSig corlib = null;

				if (value is int)
					corlib = source_method.Module.CorLibTypes.Int32;
				else if (value is float)
					corlib = source_method.Module.CorLibTypes.Single;
				else if (value is string)
					corlib = source_method.Module.CorLibTypes.String;
				MethodDef newMethod = new MethodDefUser(RandomString(),
						MethodSig.CreateStatic(corlib),
						MethodImplAttributes.IL | MethodImplAttributes.Managed,
						MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
				{
					Body = new CilBody()
				};
				if (value is int)
					newMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, (int)value));
				else if (value is float)
					newMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (double)value));
				else if (value is string)
					newMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, (string)value));
				newMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
				return newMethod;
			}
		}
	}
	}
