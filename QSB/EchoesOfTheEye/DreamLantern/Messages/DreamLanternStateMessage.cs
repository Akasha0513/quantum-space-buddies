﻿using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class DreamLanternStateMessage : QSBMessage<(DreamLanternActionType Type, bool BoolValue, float FloatValue)>
{
	public DreamLanternStateMessage(DreamLanternActionType actionType, bool state = false, float floatValue = 0f)
	{
		Data.Type = actionType;
		Data.BoolValue = state;
		Data.FloatValue = floatValue;
	}

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($"{From} Action:{Data.Type} BoolValue:{Data.BoolValue} FloatValue:{Data.FloatValue}");

		var heldItem = QSBPlayerManager.GetPlayer(From).HeldItem;

		if (heldItem is not QSBDreamLanternItem lantern)
		{
			DebugLog.ToConsole($"Error - Got DreamLanternStateMessage from player {From}, but they are not holding a QSBDreamLanternItem!");
			return;
		}

		var controller = lantern.AttachedObject._lanternController;

		switch (Data.Type)
		{
			case DreamLanternActionType.CONCEAL:
				controller.SetConcealed(Data.BoolValue);
				break;
			case DreamLanternActionType.FOCUS:
				controller.SetFocus(Data.FloatValue);
				break;
		}
	}
}
