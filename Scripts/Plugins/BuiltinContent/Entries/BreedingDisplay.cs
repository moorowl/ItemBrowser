using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class BreedingDisplay : ObjectEntryDisplay<Breeding> {
		[SerializeField]
		private BasicItemSlot parentSlot;
		[SerializeField]
		private BasicItemSlot childSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			parentSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.ParentType
			});
			childSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.ChildType
			});
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Breeding_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.ParentType)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Breeding_1",
				formatFields = new[] {
					Entry.MealsRequired.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}