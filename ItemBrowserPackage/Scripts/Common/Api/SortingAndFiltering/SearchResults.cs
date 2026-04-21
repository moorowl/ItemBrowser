using System.Collections.Generic;
using System.Globalization;
using System.Text;
using I2.Loc;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class SearchResults {
		private static readonly List<char> CharactersToStrip = new() {
			'\'', ' ', '-', '.', ',', '\n'
		};
		private static readonly Dictionary<string, string> StrippedCharactersCache = new();
		private static readonly Dictionary<ObjectDataCD, List<string>> ObjectTermsCache = new();
		private static string _lastLanguage;
		
		private readonly HashSet<ObjectDataCD> _matches;

		private SearchResults(HashSet<ObjectDataCD> matches) {
			_matches = matches;
		}

		public bool Matches(ObjectDataCD objectData) {
			return _matches.Contains(objectData);
		}
		
		private static string StripUnimportantCharacters(string term) {
			if (StrippedCharactersCache.TryGetValue(term, out var cachedResult))
				return cachedResult;
			
			var sb = new StringBuilder();
			var normalized = term.ToLowerInvariant().Normalize(NormalizationForm.FormD);

			foreach (var character in normalized) {
				if (CharactersToStrip.Contains(character))
					continue;
				
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
					sb.Append(character);
			}

			var result = sb.ToString().Normalize(NormalizationForm.FormC);
			StrippedCharactersCache[term] = result;

			return result;
		}

		private static void CheckInvalidateObjectTermsCache() {
			if (LocalizationManager.CurrentLanguage == _lastLanguage)
				return;

			ObjectTermsCache.Clear();
			_lastLanguage = LocalizationManager.CurrentLanguage;
		}

		private static List<string> GetObjectTerms(ObjectDataCD objectData) {
			if (ObjectTermsCache.TryGetValue(objectData, out var cachedTerms))
				return cachedTerms;
			
			var terms = new List<string>();
			
			var displayName = ObjectUtility.GetLocalizedDisplayName(objectData);
			var displayNameNote = ObjectUtility.GetUnlocalizedDisplayNameNote(objectData);
			if (displayNameNote != null)
				displayNameNote = API.Localization.GetLocalizedTerm(displayNameNote);
			var description = ObjectUtility.GetLocalizedDescription(objectData);

			if (displayName != null)
				terms.Add(StripUnimportantCharacters(displayName));
			if (displayNameNote != null)
				terms.Add(StripUnimportantCharacters(displayNameNote));
			if (description != null)
				terms.Add(StripUnimportantCharacters(description));
			terms.Add(StripUnimportantCharacters(ObjectUtility.GetInternalName(objectData)));
			terms.Add(((int) objectData.objectID).ToString());
			
			ObjectTermsCache[objectData] = terms;
			return terms;
		}

		public static SearchResults Create(string term) {
			term = StripUnimportantCharacters(term);

			CheckInvalidateObjectTermsCache();

			var matches = new HashSet<ObjectDataCD>();
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				foreach (var termToMatch in GetObjectTerms(objectData)) {
					if (termToMatch.Contains(term)) {
						matches.Add(objectData);
						break;
					}
				}
			}

			return new SearchResults(matches);
		}
	}
}