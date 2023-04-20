# SLC-AS-ChatOps-NewAlarmsNotifications
Automation script that implements the MS Teams bot integration to receive alarm notifications in your MS Teams Channel.

# Pre-requisites

Kindly ensure that your DataMiner system adheres to the pre-requisites described [here](https://docs.dataminer.services/user-guide/Cloud_Platform/TeamsBot/Microsoft_Teams_Chat_Integration.html#server-side-prerequisites)

# Installation

1. Deploy the automation script from this repo to your DMS.
2. Create memory files in the automation scripts to easily save the Team ID and Channel ID (this can be done with PowerShell - (details here)(https://learn.microsoft.com/en-us/powershell/module/teams/?view=teams-ps))
3. Create a correlation rule that triggers this automation script when the appropriated alarms are detected **or** use the examples provided in the Documentation folder of this repo.
