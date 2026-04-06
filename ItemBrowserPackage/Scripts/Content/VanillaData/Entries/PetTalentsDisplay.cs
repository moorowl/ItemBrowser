using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;
using PugMod;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class PetTalentsDisplay : ObjectEntryDisplay<PetTalents> {
		public ItemBrowserButton talentButton;
		public SpriteRenderer talentIcon;
		public PugText talentNameText;

		public override IEnumerable<PetTalents> OnSort(IEnumerable<PetTalents> entries) {
			return entries.OrderBy(entry => {
				if (!PugDatabase.TryGetComponent<PetCD>(entry.Pet, out var petCD))
					return "";

				return API.Localization.GetLocalizedTerm($"PetTalents/{entry.Talent}{petCD.petType}") ?? entry.Talent.ToString();
			});
		}

		protected override void OnRender(PetTalents entry) {
			if (!PugDatabase.TryGetComponent<PetCD>(entry.Pet, out var petCD))
				return;

			var talentInfo = Manager.ui.petInfosTable.GetTalent(entry.Talent);

			var displayNameTerm = $"PetTalents/{entry.Talent}{petCD.petType}";
			talentNameText.Render(displayNameTerm);
			talentButton.Title = new TextAndFormatFields {
				text = displayNameTerm
			};
			talentButton.Description = GetTalentEffects(entry.Pet, petCD.buffsOwner, talentInfo);
			talentIcon.sprite = petCD.petType switch {
				PetType.Melee => talentInfo.meleeIcon,
				PetType.Range => talentInfo.rangeIcon,
				_ => talentInfo.buffIcon
			};
		}

		protected override void OnRenderDescription(PetTalents entry, EntryDescriptionButton description) { }
		
		private static List<TextAndFormatFields> GetTalentEffects(ObjectID pet, bool buffsOwner, PetInfosTable.PetTalentInfo talentInfo) {
			var lines = new List<TextAndFormatFields>();

			var valueMultiplier = 1f;
			foreach (var multiplierOverride in talentInfo.multiplierOverrides) {
				if (multiplierOverride.petId != pet)
					continue;

				valueMultiplier = multiplierOverride.multiplier;
				break;
			}
			
			var valueBase = buffsOwner ? talentInfo.buffValue : talentInfo.value;
			var text = ConditionUI.GetConditionTextAndFormatFields(default, new ConditionData {
				conditionID = talentInfo.conditionID,
				value = (int) math.round(valueBase * valueMultiplier)
			}, false, false, false, buffsOwner);
			text.color = UserInterfaceUtility.DescriptionColor;
			text.dontLocalizeFormatFields = true;
			lines.Add(text);

			return lines;
		}
	}
}