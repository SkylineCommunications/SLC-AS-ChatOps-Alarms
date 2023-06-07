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

namespace GetElementAlarms_1
{
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
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

			var request = new GetActiveAlarmsMessage(element.DmaId, element.ElementId);

			var response = (ActiveAlarmsResponseMessage)engine.SendSLNetSingleResponseMessage(request);
			AlarmEventMessage[] alarms = response.ActiveAlarms;

			CurrentAlarms output = new CurrentAlarms { Alarms = new List<Alarm>() };
			alarms.ForEach(a => output.Alarms.Add(new Alarm { ParameterName = a.ParameterName, ParameterValue = a.Value, Severity = a.Severity }));

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