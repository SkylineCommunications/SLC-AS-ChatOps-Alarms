using System;

using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.DcpChatIntegrationHelper.Common;
using Skyline.DataMiner.DcpChatIntegrationHelper.Teams;

public class Script
{
	public void Run(IEngine engine)
	{
		var chatIntegrationHelper = new ChatIntegrationHelperBuilder().Build();
		try
		{
			var teamIdParam = engine.GetScriptParam("Team Id");
			if (String.IsNullOrWhiteSpace(teamIdParam?.Value))
			{
				engine.ExitFail("'Team Id' parameter is required.");
				return;
			}

			var channelIdParam = engine.GetScriptParam("Channel Id");
			if (String.IsNullOrWhiteSpace(channelIdParam?.Value))
			{
				engine.ExitFail("'Channel Id' parameter is required.");
				return;
			}

			var dms = engine.GetDms();

			engine.GenerateInformation("Info: " + engine.GetScriptParam(65006).Value);
			string[] alarmInfo = engine.GetScriptParam(65006).Value.Split('|');

			string alarmId = alarmInfo[0];
			int dmaId = Convert.ToInt32(alarmInfo[1]);
			int elementId = Convert.ToInt32(alarmInfo[2]);
			string paramId = alarmInfo[3];
			//string paramIdx = alarmInfo[4];
			AlarmLevel severity = (AlarmLevel)Convert.ToInt32(alarmInfo[7]);
			string status = alarmInfo[9];
			string value = alarmInfo[10];

			// string elementName = engine.FindElement(dmaId, elementId).ElementName;
			var element = dms.GetElement(new DmsElementId(dmaId, elementId));
			string elementName = element.Name;
			// string parameterDescr = tbd

			engine.GenerateInformation("severity: " + severity);
			engine.GenerateInformation("elementName: " + elementName);

			try
			{
				// chatIntegrationHelper.Teams.TrySendChannelNotification(teamIdParam.Value, channelIdParam.Value, engine.GetScriptParam(65006).Value);

				chatIntegrationHelper.Teams.TrySendChannelNotification(teamIdParam.Value, channelIdParam.Value,
					$"New Alarm - Alarm ID: {alarmId}; Severity: {severity}; Element: {elementName}; Parameter: {paramId}; Value: {value}; Status: {status};");

				// chatIntegrationHelper.Teams.TrySendChannelNotification(teamIdParam.Value, channelIdParam.Value, $" *** New Alarm *** Alarm ID: {alarmId} Element: {elementName} Severity: {severity}");

				// chatIntegrationHelper.Teams.TrySendChannelNotification(teamIdParam.Value, channelIdParam.Value, notificationParam.Value);
			}
			catch (TeamsChatIntegrationException e)
			{
				engine.ExitFail($"Couldn't send the notification to the channel with id {channelIdParam.Value} with error {e.Message}.");
				return;
			}

			engine.ExitSuccess($"The notification was sent to the channel with id {channelIdParam.Value}!");
		}
		catch (ScriptAbortException)
		{
			// Also ExitSuccess is a ScriptAbortException
			throw;
		}
		catch (Exception e)
		{
			engine.ExitFail($"An exception occurred with the following message: {e.Message}");
		}
		finally
		{
			chatIntegrationHelper?.Dispose();
		}
	}
}