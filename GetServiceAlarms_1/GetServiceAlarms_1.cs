/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2023	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace GetElementHistoryAlarms_1
{
	using System.Collections.Generic;
	using System.Linq;
	using AdaptiveCards;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Filters;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			var serviceName = engine.GetScriptParam("Service Name")?.Value;

			if (string.IsNullOrWhiteSpace(serviceName))
			{
				engine.ExitFail("A 'Service Name' is required.");
				return;
			}

			var service = engine.FindService(serviceName);

			if (service == default(Service))
			{
				engine.ExitFail($"'{serviceName}' service not found!");
				return;
			}

			AlarmFilterItem filterItem = new AlarmFilterItemString(
				AlarmFilterField.ServiceName,
				AlarmFilterCompareType.WildcardEquality,
				new[] { serviceName });

			AlarmFilterItem filterOpen = new AlarmFilterItemInt(
				AlarmFilterField.StatusID,
				AlarmFilterCompareType.Equality,
				new[] { 12 /*open*/ });

			AlarmFilterItem filterServiceImpact = new AlarmFilterItemInt(
				AlarmFilterField.ServiceImpact,
				AlarmFilterCompareType.GreaterThan,
				new[] { 0 });

			AlarmFilterItem filterSeverity = new AlarmFilterItemInt(
				AlarmFilterField.SeverityID,
				AlarmFilterCompareType.WildcardNonEquality,
				new[] { 5 /*normal*/ });

			var request = new GetActiveAlarmsMessage(service.DmaId);
			request.Filter = new AlarmFilter(filterItem, filterOpen, filterServiceImpact, filterSeverity);

			var response = (ActiveAlarmsResponseMessage)engine.SendSLNetSingleResponseMessage(request);
			AlarmEventMessage[] alarms = response.ActiveAlarms;

			var adaptiveCardBody = new List<AdaptiveElement>();
			adaptiveCardBody.Add(new AdaptiveTextBlock
			{
				Type = "TextBlock",
				Text = $"Current Alarms for {serviceName}.",
				Weight = AdaptiveTextWeight.Bolder,
				Size = AdaptiveTextSize.Large,
			});

			alarms.OrderBy(x => x.Severity)
				.ThenByDescending(x => x.RootTime)
				.Take(10)
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
						Style = SeverityToContainerStyle(a.Severity),
						Items = new List<AdaptiveElement> { infoFacts },
					};

					adaptiveCardBody.Add(bpaResultsContainer);
				});

			engine.AddScriptOutput("AdaptiveCard", JsonConvert.SerializeObject(adaptiveCardBody));
		}

		private AdaptiveContainerStyle SeverityToContainerStyle(string severity)
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
	}
}