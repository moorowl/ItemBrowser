using HarmonyLib;
using Interaction;
using Pug.Automation;
using Pug.Conversion;
using Unity.Entities;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.UserInterface.Highlights {
	public struct HighlightContainerCD : IComponentData, IEnableableComponent {
		public bool Value;

		[HarmonyPatch]
		public class HighlightContainerPostConverter : PostConverter {
			public override void PostConvert(GameObject authoring) {
				if (IsServer)
					return;
				
				var entity = GetEntity(authoring);
				if (!EntityManager.HasComponent<InteractableCD>(entity) || !EntityManager.HasComponent<ContainedObjectsBuffer>(entity))
					return;

				if (!EntityManager.HasComponent<PugAutomationCD>(entity) || !EntityManager.GetComponentData<PugAutomationCD>(entity).type.HasFlag(AutomationType.Storage))
					return;
				
				EntityManager.AddComponentData(entity, new HighlightContainerCD());
			}

			[HarmonyPatch(typeof(ECSManager), "ConfigurePostConverters")]
			[HarmonyPostfix]
			public static void ECSManager_ConfigurePostConverters(ECSManager __instance, ConversionManager conversionManager) {
				conversionManager.AddPostConverter(new HighlightContainerPostConverter());
			}
		}
	}
}