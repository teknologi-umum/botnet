using System;
using System.Collections.Generic;
using System.Text;

namespace BotNet.Services.Brainfuck {
	public class BrainfuckInterpreter {
		public static string RunBrainfuck(string code) {
			byte[] program = Encoding.UTF8.GetBytes(code);
			int programPointer = 0;
			byte[] memory = new byte[1024];
			int pointer = 0;
			Stack<int> loopPointers = new();
			Dictionary<int, int> loopCache = new();

			StringBuilder stdout = new();

			// Run
			while (programPointer < program.Length) {
				switch (program[programPointer]) {
					case 0x3E: // >
						++pointer;
						break;

					case 0x3C: // <
						--pointer;
						break;

					case 0x2B: // +
						++memory[pointer];
						break;

					case 0x2D: // -
						--memory[pointer];
						break;

					case 0x2E: // .
						stdout.Append(Convert.ToChar(memory[pointer]));
						break;

					case 0x2C: // ,
						memory[pointer] = (byte)Console.Read();
						break;

					case 0x5B: // [
						if (memory[pointer] != 0x00) {
							loopPointers.Push(programPointer);
						} else {
							if (loopCache.ContainsKey(programPointer)) {
								programPointer = loopCache[programPointer];
							} else {
								programPointer++;

								// Skip the loop.
								int currentPointer = programPointer;
								int depth = 1;

								for (int p = programPointer; p < program.Length; p++) {
									switch (program[p]) {
										case 0x5B:
											depth++;
											break;
										case 0x5D:
											depth--;
											break;
									}

									if (depth == 0) {
										loopCache[currentPointer] = p;
										programPointer = p;
										break;
									}
								}
							}
						}
						break;

					case 0x5D: // ]
						int oldPointer = programPointer;

						if (loopPointers.TryPop(out programPointer)) {
							loopCache[programPointer] = oldPointer;
							programPointer--;
						}
						break;

					default:
						throw new InvalidProgramException($"Unexpected token {Convert.ToChar(program[programPointer])} at pos {programPointer}");
				}

				programPointer++;
			}

			return stdout.ToString();
		}
	}
}
