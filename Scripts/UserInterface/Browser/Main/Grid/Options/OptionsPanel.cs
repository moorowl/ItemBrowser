using System.Linq;
using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class OptionsPanel : BlockingUIElement {
		public Transform untoggledRoot;
		public Transform toggledRoot;
		public float selectedOpacity;
		public float unselectedOpacity;
		public float unselectedOpacityButtonMultiplier;
		public float opacityLerpSpeed;
		public SpriteRenderer[] affectedSRs;
		
		private float _opacity;
		private float[] _affectedSrInitialOpacity;
		private bool[] _affectedSrIsButton;

		public bool IsToggled {
			get => toggledRoot.gameObject.activeSelf;
			set {
				untoggledRoot.gameObject.SetActive(!value);
				toggledRoot.gameObject.SetActive(value);
			}
		}

		private void Awake() {
			_affectedSrInitialOpacity = new float[affectedSRs.Length];
			_affectedSrIsButton = new bool[affectedSRs.Length];
			for (var i = 0; i < affectedSRs.Length; i++) {
				_affectedSrInitialOpacity[i] = affectedSRs[i].color.a;
				_affectedSrIsButton[i] = affectedSRs[i].transform.GetComponentInParent<ItemBrowserButton>(true) != null;
			}

			UpdateSelected();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateSelected();
		}

		private void UpdateSelected() {
			var isAnyChildSelected = childElements.Any(element => Manager.ui.currentSelectedUIElement == element);
			var targetOpacity = isAnyChildSelected ? selectedOpacity : unselectedOpacity;
			
			_opacity = Mathf.Lerp(_opacity, targetOpacity, opacityLerpSpeed * Time.deltaTime);

			for (var i = 0; i < affectedSRs.Length; i++) {
				var ratio = math.clamp(_affectedSrIsButton[i] ? _opacity * unselectedOpacityButtonMultiplier : _opacity, 0f, 1f);
				var sr = affectedSRs[i];
				sr.color = sr.color.ColorWithNewAlpha(_affectedSrInitialOpacity[i] * ratio);
			}
		}
		
		private void OnValidate() {
			childElements = GetComponentsInChildren<UIelement>(true).ToList();
			affectedSRs = GetComponentsInChildren<SpriteRenderer>(true);
		}
	}
}