using System.Collections.Generic;
using System.Globalization;
using System.Text;
using I2.Loc;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using PinyinNet;
using PugMod;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class SearchResults {
		private static readonly Dictionary<string, string> StrippedCharactersCache = new();
		private static readonly Dictionary<ObjectDataCD, string> ObjectTermsBlobCache = new();
		private static string _lastLanguage;
		private static bool _lastSearchByDescription;
		private static bool _lastSearchByEffect;
		private static bool _lastSearchById;
		
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
				switch (character) {
					case '\'':
					case ' ':
					case '-':
					case '.':
					case ',':
					case '\n':
						continue;
				}
				
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
					sb.Append(character);
			}

			var result = sb.ToString().Normalize(NormalizationForm.FormC);
			StrippedCharactersCache[term] = result;

			return result;
		}

		private static void CheckInvalidateObjectTermsBlobCache() {
			if (LocalizationManager.CurrentLanguage == _lastLanguage && OptionsManager.Instance.SearchByDescription == _lastSearchByDescription && OptionsManager.Instance.SearchByEffect == _lastSearchByEffect && OptionsManager.Instance.SearchById == _lastSearchById)
				return;

			ObjectTermsBlobCache.Clear();
			_lastLanguage = LocalizationManager.CurrentLanguage;
			_lastSearchByEffect = OptionsManager.Instance.SearchByEffect;
			_lastSearchByDescription = OptionsManager.Instance.SearchByDescription;
			_lastSearchById = OptionsManager.Instance.SearchById;
		}

		private static bool ShouldAddPinyinTerms() {
			var currentLanguage = LocalizationManager.CurrentLanguageCode;
			return currentLanguage == "zh-CN" || currentLanguage == "zh-TW";
		}

		private static string GetObjectTermsBlob(ObjectDataCD objectData) {
			if (ObjectTermsBlobCache.TryGetValue(objectData, out var cachedBlob))
				return cachedBlob;
			
			var terms = new List<string>();
			var shouldAddPinyinTerms = ShouldAddPinyinTerms();

			void TryAddTerm(string text, bool convertToPinyin = true) {
				if (text == null)
					return;
				
				terms.Add(StripUnimportantCharacters(text));

				if (convertToPinyin && shouldAddPinyinTerms)
					terms.Add(PinyinConvert.GetPinyinForAutoComplete(text));
			}

			TryAddTerm(ObjectUtility.GetLocalizedDisplayName(objectData));
			
			var displayNameNote = ObjectUtility.GetUnlocalizedDisplayNameNote(objectData);
			if (displayNameNote != null)
				displayNameNote = API.Localization.GetLocalizedTerm(displayNameNote);
			TryAddTerm(displayNameNote);

			if (OptionsManager.Instance.SearchByDescription)
				TryAddTerm(ObjectUtility.GetLocalizedDescription(objectData), false);
			
			if (OptionsManager.Instance.SearchById) {
				TryAddTerm(ObjectUtility.GetInternalName(objectData));
				TryAddTerm(((int) objectData.objectID).ToString());
			}

			if (OptionsManager.Instance.SearchByEffect) {
				foreach (var condition in ObjectUtility.GetAssociatedConditions(objectData)) {
					var conditionInfo = Manager.ui.conditionsIconsTable.GetConditionInfo(condition);
					var conditionForEffectDescription = (conditionInfo.useSameDescAsId != 0) ? conditionInfo.useSameDescAsId : conditionInfo.Id;

					var effectDescription = API.Localization.GetLocalizedTerm($"Conditions/{conditionForEffectDescription}");
					if (effectDescription != null)
						TryAddTerm(effectDescription.Replace("{0}", ""));
				}
			}

			ObjectTermsBlobCache[objectData] = string.Join('\0', terms);
			return ObjectTermsBlobCache[objectData];
		}

		public static SearchResults Create(string term, List<ObjectDataCD> objectsToFilter) {
			term = StripUnimportantCharacters(term);
			var isTermEmpty = string.IsNullOrEmpty(term);

			CheckInvalidateObjectTermsBlobCache();

			var matches = new HashSet<ObjectDataCD>();
			foreach (var objectData in objectsToFilter) {
				if (!isTermEmpty && !GetObjectTermsBlob(objectData).Contains(term))
					continue;

				matches.Add(objectData);
			}

			return new SearchResults(matches);
		}
	}
}