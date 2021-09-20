using System.Threading.Tasks;
using BotNet.Services.OpenGraph.Models;
using Orleans;

namespace BotNet.GrainInterfaces {
	public interface IOpenGraphGrain : IGrainWithStringKey {
		Task<OpenGraphMetadata> GetMetadataAsync();
	}
}
