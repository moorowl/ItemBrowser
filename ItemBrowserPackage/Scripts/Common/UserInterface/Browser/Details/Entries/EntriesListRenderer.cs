using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public abstract class EntriesListRenderer {
		public float TotalHeight { get; protected set; }

		public virtual bool SetEntries(EntriesList list, ObjectDataCD objectData, List<ObjectEntry> entries) {
			return false;
		}
		
		public virtual void RenderList() { }

		public virtual void ClearList() { }
		
		public virtual void Update() { }
		
		public static void AppendRequirements((ObjectEntry Entry, List<(ObjectEntryRequirement Requirement, bool IsFulfilled)> Requirements, bool IsAllFulfilled) entry, EntryDescriptionButton description) {
			if (entry.Requirements.Count == 0)
				return;

			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-General/AvailabilityRequirements",
				formatFields = new[] {
					entry.Requirements.Count(rule => rule.IsFulfilled).ToString(),
					entry.Requirements.Count.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.AlmostWhiteColor
			});

			foreach (var rule in entry.Requirements) {
				description.AddLine(new TextAndFormatFields {
					text = "- {0}",
					dontLocalize = true,
					formatFields = new[] {
						rule.Requirement.GetLocalizedDescription()
					},
					dontLocalizeFormatFields = true,
					color = (rule.IsFulfilled ? UserInterfaceUtility.AlmostWhiteColor : UserInterfaceUtility.DescriptionColor)
				});
			}
		}
	}
}