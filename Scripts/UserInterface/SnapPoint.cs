using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Pug.UnityExtensions;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ItemBrowser.UserInterface {
	public class SnapPoint : MonoBehaviour {
		private static readonly List<SnapPoint> ActiveSnapPoints = new();
		
		public List<string> tags;
		public bool useCustomSnapRules;
		[ShowIf("useCustomSnapRules")]
		public float angleWeight = 0.5f;
		[ShowIf("useCustomSnapRules")]
		public float distanceWeight = 0.5f;
		[ShowIf("useCustomSnapRules")]
		public float coneThreshold = 0.2f;

		public UIelement AttachedElement { get; private set; }
		public BoxCollider Collider { get; private set; }
		
		private void Awake() {
			AttachedElement = GetComponent<UIelement>();
			Collider = GetComponentInChildren<BoxCollider>(true);
		}

		private void OnEnable() {
			ActiveSnapPoints.Add(this);
		}

		private void OnDisable() {
			ActiveSnapPoints.Remove(this);
		}

		public bool CanSnapTo(SnapPoint other) {
			return other.AttachedElement.isShowing
			       && (other.AttachedElement.isVisibleOnScreen || ElementsSharesScrollWindow(AttachedElement, other.AttachedElement))
				   && other.AttachedElement.gameObject.activeSelf;
		}
		
		private static bool ElementsSharesScrollWindow(UIelement a, UIelement b) {
			if (a != null && b != null && a.uiScrollWindow != null)
				return a.uiScrollWindow == b.uiScrollWindow;

			return false;
		}

		public SnapPoint TryFindNextSnapPoint(Direction.Id direction) {
			var currentBounds = Collider.bounds;
			var currentCenter = currentBounds.center;

			List<(SnapPoint Point, float Score, float AngleDot, float Distance)> candidates = new();

			foreach (var snapPoint in ActiveSnapPoints) {
				if (snapPoint == this || !CanSnapTo(snapPoint))
					continue;

				var bounds = snapPoint.Collider.bounds;
				
				var closest = bounds.ClosestPoint(currentCenter);
				var raw = closest - currentCenter;
				var dist = raw.magnitude;
				
				if (dist <= Mathf.Epsilon) {
					const float angleDotOverlap = 1f;
					var scoreOverlap = ComputeScore(angleDotOverlap, 0f);
					candidates.Add((snapPoint, scoreOverlap, angleDotOverlap, 0f));
					continue;
				}

				var dir2 = new Vector2(raw.x, raw.y).normalized;
				var dot = Vector2.Dot(dir2, ((Direction) direction).vec2);
				
				if (dot < coneThreshold)
					continue;
				
				var score = ComputeScore(dot, dist);
				candidates.Add((snapPoint, score, dot, dist));
			}

			if (candidates.Count == 0)
				return null;
			
			var best = candidates
				.OrderByDescending(c => c.Score)
				.ThenByDescending(c => c.AngleDot)
				.ThenBy(c => c.Distance)
				.First();

			return best.Point;

			float ComputeScore(float angleDot, float distance) {
				var distanceScore = 1f / (1f + distance);
				var score = angleWeight * Mathf.Clamp01(angleDot) + distanceWeight * Mathf.Clamp01(distanceScore);
				return score;
			}
		}

		public static SnapPoint TryFindNextSnapPoint(UIelement element, Direction.Id direction) {
			var snapPoint = element.GetComponent<SnapPoint>();
			if (snapPoint == null)
				return null;

			return snapPoint.TryFindNextSnapPoint(direction);
		}

		public static bool HasSnapPoint(UIelement element) {
			return element.GetComponent<SnapPoint>() != null;
		}
	}
}