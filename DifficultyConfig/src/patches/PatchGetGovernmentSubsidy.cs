using Game.Simulation;
using HarmonyLib;
using Unity.Mathematics;

namespace DifficultyConfig
{
	[HarmonyPatch(typeof(CityServiceBudgetSystem), "GetGovernmentSubsidy")]
	public class PatchGetGovernmentSubsidy
	{
		[HarmonyPostfix]
		public static void Postfix(ref int __result)
		{
			switch (Mod.INSTANCE.settings().subsidyType)
			{
				case DifficultySettings.SubsidyType.NEGATIVE:
					__result = -1000000000;
					break;
				case DifficultySettings.SubsidyType.NONE:
					__result = math.min(0, __result);
					break;
				case DifficultySettings.SubsidyType.DEFAULT:
					break;
				case DifficultySettings.SubsidyType.HIGH:
					__result = 10000000;
					break;
			}
		}
	}
}
