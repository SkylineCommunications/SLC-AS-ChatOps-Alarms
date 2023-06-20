namespace ShowAlarmsLibrary
{
	using System.Collections.Generic;
	using System.Linq;
	using AdaptiveCards;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;

	public static class AlarmsUtils
	{
		public static AdaptiveContainerStyle SeverityToContainerStyle(this string severity)
		{
			switch (severity)
			{
				case "Warning":
				case "Minor":
					return AdaptiveContainerStyle.Warning;

				case "Major":
				case "Critical":
					return AdaptiveContainerStyle.Attention;

				case "Normal":
					return AdaptiveContainerStyle.Good;

				default:
					return AdaptiveContainerStyle.Emphasis;
			}
		}

		public static IEnumerable<AdaptiveElement> CreateAdaptiveCard(string message, IEnumerable<AlarmEventMessage> alarms, int maxAlarmCount = 10)
		{
			var adaptiveCardBody = new List<AdaptiveElement>();
			adaptiveCardBody.Add(new AdaptiveTextBlock
			{
				Type = "TextBlock",
				Text = message,
				Weight = AdaptiveTextWeight.Bolder,
				Size = AdaptiveTextSize.Large,
			});

			alarms.OrderBy(x => x.Severity)
				.ThenByDescending(x => x.RootTime)
				.Take(maxAlarmCount)
				.ForEach(a =>
				{
					var infoFacts = new AdaptiveFactSet
					{
						Type = "FactSet",
						Facts = new List<AdaptiveFact>
						{
							new AdaptiveFact("Parameter:", a.ParameterName),
							new AdaptiveFact("Value:", a.Value),
							new AdaptiveFact("Severity:", a.Severity),
						},
					};

					var bpaResultsContainer = new AdaptiveContainer
					{
						Type = "Container",
						Style = a.Severity.SeverityToContainerStyle(),
						Items = new List<AdaptiveElement> { infoFacts },
					};

					adaptiveCardBody.Add(bpaResultsContainer);
				});

			return adaptiveCardBody;
		}
	}
}