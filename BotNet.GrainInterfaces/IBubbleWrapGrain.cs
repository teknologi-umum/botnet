using System.Threading.Tasks;
using Orleans;

namespace BotNet.GrainInterfaces {
	public interface IBubbleWrapGrain : IGrainWithStringKey {
		Task<bool[,]?> GetSheetStateAsync();
		Task PopAsync(bool[,] expectedSheet);
	}
}
