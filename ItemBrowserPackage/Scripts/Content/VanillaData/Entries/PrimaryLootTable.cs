using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record PrimaryLootTable : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/PrimaryLootTable", ObjectID.LegendarySwordParchment, VanillaPriorities.PrimaryLootTable);
		public override Type Renderer => typeof(LootTableEntriesListRenderer);

		public List<Pool> Pools { get; set; } = new();
		
		public Pool CreateAndAddPool(IPoolHeader header = null) {
			var pool = new Pool(header);
			Pools.Add(pool);

			return pool;
		}
		
		public class Pool {
			public readonly IPoolHeader Header;

			public List<Entry> Entries { get; set; } = new();

			public bool IsAffectedByPlayerCount => Entries.Any(entry => entry.IsAffectedByPlayerCount);
			public bool IsAffectedByWorldMode => Entries.Any(entry => entry.IsAffectedByWorldMode);

			public Pool(IPoolHeader header = null) {
				Header = header ?? new EmptyPoolHeader();
			}
			
			public void AddEntry(Entry entry) {
				Entries.Add(entry);
			}
		}

		public interface IPoolHeader {
			string GetLocalizedDescription(Pool pool);
		}

		public class EmptyPoolHeader : IPoolHeader {
			public string GetLocalizedDescription(Pool pool) {
				return null;
			}
		}
		
		public class SeasonPoolHeader : IPoolHeader {
			public readonly Season Season;

			public SeasonPoolHeader(Season season) {
				Season = season;
			}

			public string GetLocalizedDescription(Pool pool) {
				return string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-PoolHeaders/Season"),
					API.Localization.GetLocalizedTerm($"Seasons/{Season}") ?? Season.ToString()
				);
			}
		}
		
		public class RollsPoolHeader : IPoolHeader {
			public readonly ValueBasedOnWorldState<(int Min, int Max)> Rolls;

			public RollsPoolHeader(ValueBasedOnWorldState<(int Min, int Max)> rolls) {
				Rolls = rolls;
			}

			public string GetLocalizedDescription(Pool pool) {
				var rolls = Rolls.Get();

				return string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-PoolHeaders/Rolls" + (rolls.Max != 1 ? "_Plural" : "")),
					UserInterfaceUtils.FormatRange(rolls)
				);
			}
		}
		
		public abstract record Entry : ObjectEntry {
			public (ObjectID Id, int Variation) Result { get; set; }
			public float Chance { get; set; }
			public ValueBasedOnWorldState<float> ChanceForOne { get; set; }
			public ValueBasedOnWorldState<(int Min, int Max)> Amount { get; set; }
			public Biome OnlyDropsInBiome { get; set; }
			public bool IsAffectedByPlayerCount { get; set; }
			public bool IsAffectedByWorldMode { get; set; }
		}
	}
}