using System;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using Orleans;

namespace BotNet.Grains {
	public class BubbleWrapGrain : Grain, IBubbleWrapGrain {
		private bool[,]? _sheet;

		public Task<bool[,]?> GetSheetStateAsync() {
			return Task.FromResult(_sheet);
		}

		public Task PopAsync(bool[,] expectedSheet) {
			if (_sheet is null) {
				_sheet = expectedSheet;
			} else {
				for (int row = 0; row < 8; row++) {
					for (int col = 0; col < 8; col++) {
						_sheet[row, col] &= expectedSheet[row, col];
					}
				}
			}
			DelayDeactivation(TimeSpan.FromSeconds(5));
			return Task.CompletedTask;
		}
	}
}
