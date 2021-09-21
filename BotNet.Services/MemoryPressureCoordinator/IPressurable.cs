using System.Threading.Tasks;

namespace BotNet.Services.MemoryPressureCoordinator {
	public interface IPressurable {
		Task ApplyPressureAsync();
		void ReleasePressure();
	}
}
