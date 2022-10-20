using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Stability {
	public class StabilityClient : IDisposable {
		private readonly GrpcChannel _grpcChannel;
		private readonly GenerationService.GenerationServiceClient _generationServiceClient;
		private readonly string _apiKey;
		private bool _disposedValue;

		public StabilityClient(
			IOptions<StabilityOptions> optionsAccessor
		) {
			_grpcChannel = GrpcChannel.ForAddress("https://grpc.stability.ai/");
			_generationServiceClient = new GenerationService.GenerationServiceClient(_grpcChannel);
			_apiKey = optionsAccessor.Value.ApiKey!;
		}

		public async Task<byte[]> GenerateImageAsync(string promptText, CancellationToken cancellationToken) {
			if (_grpcChannel.State is ConnectivityState.Idle or ConnectivityState.Shutdown) {
				await _grpcChannel.ConnectAsync(cancellationToken);
			}

			AsyncServerStreamingCall<Answer> streamingCall = _generationServiceClient.Generate(
				request: new Request {
					EngineId = "stable-diffusion-v1",
					RequestId = Guid.NewGuid().ToString(),
					Prompt = {
						new Prompt {
							Text = promptText
						}
					},
					Image = new ImageParameters {
						Width = 512,
						Height = 512,
						Steps = 10,
						Samples = 1,
						Transform = new TransformType {
							Diffusion = DiffusionSampler.SamplerKLms
						},
						Parameters = {
							new StepParameter {
								ScaledStep = 0,
								Sampler = new SamplerParameters {
									CfgScale = 7
								}
							}
						},
						Seed = {
							(uint)Random.Shared.Next()
						}
					}
				},
				headers: new Metadata {
					{ "Authorization", $"Bearer {_apiKey}" }
				},
				cancellationToken: cancellationToken
			);

			await foreach (Answer answer in streamingCall.ResponseStream.ReadAllAsync(cancellationToken)) {
				foreach (Artifact artifact in answer.Artifacts) {
					if (artifact.Type == ArtifactType.ArtifactImage) {
						return artifact.Binary.ToByteArray();
					}
				}
			}

			throw new InvalidOperationException("Unable to generate image");
		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					// dispose managed state (managed objects)
					_grpcChannel.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
