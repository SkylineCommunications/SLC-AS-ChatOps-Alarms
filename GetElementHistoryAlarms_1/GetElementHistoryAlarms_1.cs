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
	using System.Linq;
	using Newtonsoft.Json;
	using ShowAlarmsLibrary;
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
			var elementName = engine.GetScriptParam("Element Name")?.Value;

			if (string.IsNullOrWhiteSpace(elementName))
			{
				engine.ExitFail("An 'Element Name' is required.");
				return;
			}

			var element = engine.FindElement(elementName);

			if (element == default(Element))
			{
				engine.ExitFail($"'{elementName}' element not found!");
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

			AlarmFilterItem filterItem = new AlarmFilterItemString(
				AlarmFilterField.ElementID,
				AlarmFilterCompareType.WildcardEquality,
				new[] { elementName });

			AlarmFilterItem filterOpen = new AlarmFilterItemInt(
				AlarmFilterField.StatusID,
				AlarmFilterCompareType.Equality,
				new[] { 12 /*open*/ });

			AlarmFilterItem filterSeverity = new AlarmFilterItemInt(
				AlarmFilterField.SeverityID,
				AlarmFilterCompareType.WildcardNonEquality,
				new[] { 5 /*normal*/ });

			var dateNow = DateTime.Now;
			var request = new GetAlarmDetailsFromDbMessage(
				dataMinerID: element.DmaId,
				filter: new AlarmFilter(filterItem, filterOpen, filterSeverity),
				startTime: dateNow.AddHours(-24),
				endTime: dateNow,
				alarmTable: true,
				infoTable: false);

			var response = engine.SendSLNetMessage(request);

			var adaptiveCardBody = AlarmsUtils.CreateAdaptiveCard(
				message: $"History Alarms for {elementName}.",
				alarms: response.Select(alarm => (AlarmEventMessage)alarm));

			engine.AddScriptOutput("AdaptiveCard", JsonConvert.SerializeObject(adaptiveCardBody));
		}
	}
}