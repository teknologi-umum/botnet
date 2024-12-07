using System;
using System.Text;

namespace BotNet.Services.Brainfuck {
	public class BrainfuckTranspiler {
		private readonly string[][] _map;
		private readonly string[] _minusMap;
		private readonly string[] _plusMap;
		private int _repeat;

		public BrainfuckTranspiler() {
			_map = new string[256][];
			_plusMap = new string[256];
			_plusMap[0] = "";
			_minusMap = new string[256];
			_minusMap[0] = "";
			_repeat = 2;
			for (int i = 1; 256 > i; i++) {
				_plusMap[i] = _plusMap[i - 1] + "+";
				_minusMap[i] = _minusMap[i - 1] + "-";
			}
			for (int x = 0; 256 > x; x++) {
				_map[x] = new string[256];
				for (int y = 0; 256 > y; y++) {
					int delta = y - x;
					if (128 < delta) delta -= 256;
					if (-128 > delta) delta += 256;
					_map[x][y] = 0 <= delta
						? _plusMap[delta]
						: _minusMap[-delta];
				}
			}
			Next();
		}

		public string TranspileBrainfuck(string message) {
			return Generate(message);
		}

		private static int Gcd(
			int c,
			int a
		) {
			while (true) {
				if (a == 0) return c;
				int c1 = c;
				c = a;
				a = c1 % a;
			}
		}

		private static int InverseMod(int c, int a) {
			int f = 1, d = 0, b;
			while (a != 0) {
				b = f;
				f = d;
				d = b - d * ((c / a) | 0);
				b = c;
				c = a;
				a = b % a;
			}
			return f;
		}

		private static int ShortestStr(string[] c) {
			int a = 0;
			for (int f = 1; f < c.Length; f++) {
				if (c[f].Length < c[a].Length) {
					a = f;
				}
			}
			return a;
		}

		private void Next() {
			for (int c = 0; 256 > c; c++) {
				for (int a = 1; 40 > a; a++) {
					for (int f = InverseMod(a, 256) & 255, d = 1; 40 > d; d++) {
						if (1 == Gcd(a, d)) {
							int b;
							int e;
							if ((a & 1) != 0) {
								b = 0;
								e = (c * f) & 255;
							} else {
								for (b = c, e = 0; 256 > e && b != 0; e++) {
									b = (b - a) & 255;
								}
							}
							if (0 == b) {
								b = (d * e) & 255;
								if (a + d + 5 < _map[c][b].Length) {
									_map[c][b] = "[" + _minusMap[a] + ">" + _plusMap[d] + "<]>";
								}
							}
							if ((a & 1) != 0) {
								b = 0;
								e = (-c * f) & 255;
							} else {
								for (b = c, e = 0; 256 > e && b != 0; e++) {
									b = (b + a) & 255;
								}
							}
							if (0 == b) {
								b = (-d * e) & 255;
								if (a + d + 5 < _map[c][b].Length) {
									_map[c][b] = "[" + _plusMap[a] + ">" + _minusMap[d] + "<]>";
								}
							}
						}
					}
				}
			}
			for (int c = 0; 256 > c; c++) {
				string[] a = _map[c];
				for (int e = 0; 256 > e; e++) {
					string[] f = _map[e];
					string d = a[e];
					for (int b = 0; 256 > b; b++) {
						if (d.Length + f[b].Length < a[b].Length) {
							a[b] = d + f[b];
						}
					}
				}
			}
			if (--_repeat != 0) {
				Next();
			}
		}

		private string Generate(string s) {
			Span<byte> c = stackalloc byte[Encoding.UTF8.GetByteCount(s)];
			Encoding.UTF8.GetBytes(s, c);
			StringBuilder d = new();
			for (int a = 0, f = c.Length, b = 0; b < f; b++) {
				int e = c[b] & 255;
				string[] l = [$">{_map[0][e]}", _map[a][e]];
				int g = ShortestStr(l);
				d.Append(l[g]).Append('.');
				a = e;
			}
			return d.ToString();
		}
	}
}
