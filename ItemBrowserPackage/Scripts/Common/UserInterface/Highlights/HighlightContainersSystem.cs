using ItemBrowser.Common.Api;
using Outlines.Components;
using Outlines.Systems;
using Pug.ECS.Hybrid;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace ItemBrowser.Common.UserInterface.Highlights {
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	[UpdateBefore(typeof(VisualOutlineDisplaySystem))]
	[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
	public partial struct HighlightContainersSystem : ISystem {
		private NativeList<ObjectID> _objectsToHiglight;
		private EntityQuery _entitiesToHighlightQuery;

		public void OnCreate(ref SystemState state) {
			_objectsToHiglight = new NativeList<ObjectID>(64, Allocator.Persistent);
			_entitiesToHighlightQuery = SystemAPI.QueryBuilder().WithAll<HighlightContainerCD>().WithAll<VisualOutlineCD>().WithAll<ContainedObjectsBuffer>().WithAll<EntityMonoBehaviourCD>().Build();
			
			state.RequireForUpdate<NetworkTime>();
		}

		public void OnDestroy(ref SystemState state) {
			_objectsToHiglight.Dispose();
		}

		public void OnUpdate(ref SystemState state) {
			if (ItemBrowserAPI.ItemBrowserUI == null)
				return;
			
			var networkTime = SystemAPI.GetSingleton<NetworkTime>();
			if (ClientWorldStateSystem.ShouldUpdate(ref state, networkTime, 15, 0.5f)) {
				_objectsToHiglight.Length = 2;
				_objectsToHiglight.Clear();
				foreach (var id in ItemBrowserAPI.ItemBrowserUI.ObjectsToHighlightInInventory)
					_objectsToHiglight.Add(id);
				
				state.Dependency = new UpdateHighlightedEntitiesJob {
					ObjectsToHighlight = _objectsToHiglight,
				}.Schedule(_entitiesToHighlightQuery, state.Dependency);	
			}
			
			foreach (var (visualOutlineCD, highlightContainerCD) in SystemAPI.Query<RefRW<VisualOutlineCD>, RefRO<HighlightContainerCD>>().WithAll<EntityMonoBehaviourCD, ContainedObjectsBuffer>()) {
				if (highlightContainerCD.ValueRO.Value)
					visualOutlineCD.ValueRW.outlineType = OutlineType.ClosestInteractable;
			}
		}

		public partial struct UpdateHighlightedEntitiesJob : IJobEntity {
			public NativeList<ObjectID> ObjectsToHighlight;

			public void Execute(Entity entity, ref HighlightContainerCD highlightContainerCD, DynamicBuffer<ContainedObjectsBuffer> containedObjectsBuffer) {
				foreach (var containedObject in containedObjectsBuffer) {
					foreach (var objectToHighlight in ObjectsToHighlight) {
						if (objectToHighlight == containedObject.objectID) {
							highlightContainerCD.Value = true;
							return;
						}
					}
				}
				
				highlightContainerCD.Value = false;
			}
		}
	}
}