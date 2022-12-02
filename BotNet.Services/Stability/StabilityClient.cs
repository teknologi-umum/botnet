using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using SkiaSharp;

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

		public async Task<byte[]> ModifyImageAsync(byte[] imagePrompt, string textPrompt, CancellationToken cancellationToken) {
			if (_grpcChannel.State is ConnectivityState.Idle or ConnectivityState.Shutdown) {
				await _grpcChannel.ConnectAsync(cancellationToken);
			}

			// Convert to png
			using SKBitmap bitmap = SKBitmap.Decode(imagePrompt);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;
			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width, bitmap.Height),
				dest: SKRect.Create(bitmap.Width, bitmap.Height));
			canvas.Flush();
			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 80);
			using MemoryStream convertedImageStream = new();
			data.SaveTo(convertedImageStream);
			imagePrompt = convertedImageStream.ToArray();

			AsyncServerStreamingCall<Answer> streamingCall = _generationServiceClient.Generate(
				request: new Request {
					EngineId = "stable-diffusion-v1",
					RequestId = Guid.NewGuid().ToString(),
					Prompt = {
					new Prompt {
						Text = textPrompt
					},
					new Prompt {
						Artifact = new Artifact {
							Type = ArtifactType.ArtifactImage,
							Binary = ByteString.CopyFrom(imagePrompt),
							Mime = "image/png"
						},
						Parameters = new PromptParameters {
							Init = true
						}
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
							},
							Schedule = new ScheduleParameters {
								Start = 1.0f,
								End = 0.01f
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
