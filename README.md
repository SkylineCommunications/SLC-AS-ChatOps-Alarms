# SLC-AS-ChatOps-Alarms

This repository contains automation scripts that can help you to interact with the Alarms from your DataMiner system using the DataMiner Teams bot.

This contains the following automation scripts:

- [SLC-AS-ChatOps-Alarms](#slc-as-chatops-alarms)
- [Pre-requisites](#pre-requisites)
  - [Send Channel Notification - Alarms from Correlation](#send-channel-notification---alarms-from-correlation)
    - [Installation](#installation)
  - [Alarm Info](#alarm-info)

# Pre-requisites

Kindly ensure that your DataMiner system and your Microsoft Teams adhere to the pre-requisites described in [DM Docs](https://docs.dataminer.services/user-guide/Cloud_Platform/TeamsBot/Microsoft_Teams_Chat_Integration.html#server-side-prerequisites).

## Send Channel Notification - Alarms from Correlation
Automation script that implements the MS Teams bot integration to receive alarm notifications in your MS Teams Channel.

### Installation

1. Deploy the automation script from this repo to your DMS.
   This can be done by cloning the repo and using DIS to publish in your DMS or going to the Catalog and deploy from there or use the DataMiner CICD Automation GitHub Action.

2. Create memory files in the automation scripts to easily save the Team ID and Channel ID (this can be done with PowerShell - [details here](https://learn.microsoft.com/en-us/powershell/module/teams/?view=teams-ps))

3. Create a correlation rule that triggers this automation script when the appropriated alarms are detected **or** use the examples provided in the Documentation folder of this repo.

## Alarm Info
To be added.