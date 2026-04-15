using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class SearchResults {
		private static readonly List<char> CharactersToStrip = new() {
			'\'', ' ', '-', '.', '\n'
		};
		
		private readonly HashSet<ObjectDataCD> _matches;

		private SearchResults(HashSet<ObjectDataCD> matches) {
			_matches = matches;
		}

		public bool Matches(ObjectDataCD objectData) {
			return _matches.Contains(objectData);
		}
		
		private static string StripUnimportantCharacters(string term) {
			var sb = new StringBuilder();
			var normalized = term.ToLowerInvariant().Normalize(NormalizationForm.FormD);

			foreach (var character in normalized) {
				if (CharactersToStrip.Contains(character))
					continue;
				
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
					sb.Append(character);
			}

			return sb.ToString().Normalize(NormalizationForm.FormC);
		}

		public static SearchResults Create(string term) {
			term = StripUnimportantCharacters(term);
			
			var matches = new HashSet<ObjectDataCD>();
			var termsToMatch = new List<string>(5);

			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				var displayName = ObjectUtility.GetLocalizedDisplayName(objectData);
				var displayNameNote = ObjectUtility.GetUnlocalizedDisplayNameNote(objectData);
				if (displayNameNote != null)
					displayNameNote = API.Localization.GetLocalizedTerm(displayNameNote);
				var description = ObjectUtility.GetLocalizedDescription(objectData);
				
				termsToMatch.Clear();
				if (displayName != null)
					termsToMatch.Add(displayName);
				if (displayNameNote != null)
					termsToMatch.Add(displayNameNote);
				if (description != null)
					termsToMatch.Add(description);
				termsToMatch.Add(ObjectUtility.GetInternalName(objectData));
				termsToMatch.Add(((int) objectData.objectID).ToString());

				foreach (var termToMatch in termsToMatch) {
					if (StripUnimportantCharacters(termToMatch).Contains(term)) {
						matches.Add(objectData);
						break;
					}
				}
			}

			return new SearchResults(matches);
		}
	}
}