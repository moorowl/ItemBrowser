using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Utilities.DataStructures {
	public class CyclingObjectData {
		public const float DefaultCycleSpeed = 2f;
			
		private readonly List<ObjectDataCD> _objectsToDisplay;
		private readonly float _cycleSpeed;
		private float _lastCycledTime;
			
		public ObjectDataCD CurrentObjectData => _objectsToDisplay.Count > 0 ? _objectsToDisplay[CurrentObjectDataIndex] : default;
		public int CurrentObjectDataIndex { get; private set; }

		public CyclingObjectData(IEnumerable<ObjectDataCD> objectsToDisplay, float cycleSpeed = DefaultCycleSpeed) {
			_objectsToDisplay = objectsToDisplay.ToList();
			_cycleSpeed = cycleSpeed;
		}
			
		public CyclingObjectData(float cycleSpeed = DefaultCycleSpeed) {
			_objectsToDisplay = new List<ObjectDataCD>();
			_cycleSpeed = cycleSpeed;
		}

		public void Add(ObjectDataCD objectData) {
			_objectsToDisplay.Add(objectData);
		}
			
		public void Update(SlotUIBase slot) {
			if (_lastCycledTime == 0f)
				_lastCycledTime = Time.time;
				
			if (Time.time >= _lastCycledTime + _cycleSpeed) {
				CurrentObjectDataIndex++;
				if (CurrentObjectDataIndex >= _objectsToDisplay.Count)
					CurrentObjectDataIndex = 0;

				_lastCycledTime = Time.time;
				if (slot is ItemBrowserSlot basicItemSlot)
					basicItemSlot.UpdateVisuals();
			}
		}
	}
}