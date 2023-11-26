using System;
using System.Text;

namespace BotNet.Views.DecimalClock {
	public static class DecimalClockSvgBuilder {
		public static string GenerateSvg() {
			TimeSpan timeOfDay = DateTime.UtcNow.AddHours(7).TimeOfDay;
			double fractionOfDay = timeOfDay / TimeSpan.FromDays(1);

			StringBuilder digitsBuilder = new();
			for (int i = 0; i < 10; i++) {
				double x = 400 - Math.Sin(Math.PI * i / 5) * 320;
				double y = 400 + Math.Cos(Math.PI * i / 5) * 320;
				digitsBuilder.Append($$"""
				        <text x="{{x}}" y="{{y}}">{{i}}</text>

				""");
			}

			StringBuilder ticksBuilder = new();
			for (int i = 0; i < 100; i++) {
				ticksBuilder.Append($$"""
				<rect x="398.8" y="0" width="2.4" class="{{(i % 10 == 0 ? "large tick" : "tick")}}" transform="rotate({{i * 3.6}})" transform-origin="400 400" fill="#888"></rect>

				""");
			}

			return $$"""
			<svg viewBox="0 0 1000 1000" xmlns="http://www.w3.org/2000/svg">
			    <style>
			        text {
			            font: 80px sans-serif;
			            text-anchor: middle;
			            alignment-baseline: middle;
			        }
			        .logo {
			            font: 20px sans-serif;
			            font-weight: 500;
			            alignment-baseline: top;
			            letter-spacing: 2.4px;
			        }
			        .tick {
			            height: 16px;
			        }
			        .large.tick {
			            height: 32px;
			        }
			    </style>
			    <rect x="88.75" y="88.75" width="822.5" height="822.5" rx="422.5" stroke="#888" stroke-width="22.5" fill="#fff"></rect>
			    <g transform="translate(100 100)">
			        <text x="400" y="228" class="logo" fill="#bbb">TEKNUM</text>
					{{digitsBuilder.ToString().Trim()}}
					{{ticksBuilder.ToString().Trim()}}
			        <g transform="rotate({{fractionOfDay * 360 + 180}})" transform-origin="400 400">
			            <rect x="396" y="216" width="8" height="184" fill="#000" transform-origin="400 400">
			                <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0" to="360" dur="24h" repeatCount="indefinite" />
			            </rect>
			        </g>
			        <g transform="rotate({{fractionOfDay * 10 % 1 * 360 + 180}})" transform-origin="400 400">
			            <rect x="396" y="160" width="8" height="240" fill="#000" transform-origin="400 400">
			                <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0" to="360" dur="2.4h" repeatCount="indefinite" />
			            </rect>
			        </g>
			        <g transform="rotate({{fractionOfDay * 1000 % 1 * 360 + 180}})" transform-origin="400 400">
			            <rect x="398.8" y="144" width="2.4" height="256" fill="#000" transform-origin="400 400">
			                <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0" to="360" dur="0.024h" repeatCount="indefinite" />
			            </rect>
			        </g>
			        <rect x="384" y="384" width="32" height="32" rx="16" fill="#000"></rect>
			    </g>
			</svg>

			""";
		}
	}
}
