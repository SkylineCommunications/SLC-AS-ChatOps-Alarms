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
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Filters;
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
			// format (ISO-8601): yyyy-MM-ddTHH:mm:ssZ
			var serviceName = engine.GetScriptParam("Service Name")?.Value;

			if (string.IsNullOrWhiteSpace(serviceName))
			{
				engine.ExitFail("A 'Service Name' is required.");
				return;
			}

			var service = engine.FindService(serviceName);

			if (service == default(Service))
			{
				engine.ExitFail($"'{serviceName}' element not found!");
				return;
			}

			var fromdatetime = engine.GetScriptParam("Start Date")?.Value;
			if (string.IsNullOrWhiteSpace(fromdatetime))
			{
				engine.ExitFail("A 'Start Date' is required.");
				return;
			}

			var todatetime = engine.GetScriptParam("End Date")?.Value;
			if (string.IsNullOrWhiteSpace(todatetime))
			{
				engine.ExitFail("An 'End Date' is required.");
				return;
			}

			DateTime startDate;
			if (!DateTime.TryParseExact(fromdatetime.Replace("S", string.Empty), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
			{
				engine.ExitFail("'Start Date' should be provided with the following format: 'yyyy-MM-ddTHH:mm:ss'.");
				return;
			}

			DateTime endDate;
			if (!DateTime.TryParseExact(todatetime.Replace("S", string.Empty), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
			{
				engine.ExitFail("'End Date' should be provided with the following format: 'yyyy-MM-ddTHH:mm:ss'.");
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

			var request = new GetAlarmDetailsFromDbMessage(
				dataMinerID: service.DmaId,
				filter: new AlarmFilter(filterItem, filterOpen, filterServiceImpact, filterSeverity),
				startTime: startDate,
				endTime: endDate,
				alarmTable: true,
				infoTable: false
			);

			var response = engine.SendSLNetMessage(request);

			CurrentAlarms output = new CurrentAlarms { Alarms = new List<Alarm>() };
			foreach (AlarmEventMessage alarm in response)
			{
				output.Alarms.Add(
					new Alarm
					{
						ParameterName = alarm.ParameterName,
						ParameterValue = alarm.Value,
						Severity = alarm.Severity,
					});
			}

			// engine.GenerateInformation(JsonConvert.SerializeObject(output));
			engine.AddSingularJsonOutput(JsonConvert.SerializeObject(output));
		}
	}

	public class CurrentAlarms
	{
		public List<Alarm> Alarms { get; set; }
	}

	public class Alarm
	{
		public string ParameterName { get; set; }

		public string ParameterValue { get; set; }

		public string Severity { get; set; }
	}
}