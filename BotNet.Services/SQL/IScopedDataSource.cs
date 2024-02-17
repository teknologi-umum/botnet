using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.SQL {
	public interface IScopedDataSource {
		Task LoadTableAsync(CancellationToken cancellationToken);
	}
}
