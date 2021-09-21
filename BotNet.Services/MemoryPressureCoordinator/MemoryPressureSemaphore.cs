using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.MemoryPressureCoordinator {
	public class MemoryPressureSemaphore {
		private readonly HashSet<IPressurable> _pressurables = new();
		private readonly SemaphoreSlim _semaphore = new(1, 1);

		public void Register(IPressurable caller) {
			_pressurables.Add(caller);
		}

		public async Task WaitAsync(IPressurable caller) {
			await _semaphore.WaitAsync();
			foreach (IPressurable pressurable in _pressurables) {
				if (pressurable != caller) {
					await pressurable.ApplyPressureAsync();
				}
			}
			GC.Collect();
		}

		public void Release(IPressurable caller) {
			foreach (IPressurable pressurable in _pressurables) {
				if (pressurable != caller) {
					pressurable.ReleasePressure();
				}
			}
			_semaphore.Release();
			GC.Collect();
		}
	}
}
