﻿using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Component;

internal class ComponentRepairTickMessage : QSBWorldObjectMessage<QSBShipComponent, float>
{
	public ComponentRepairTickMessage(float repairFraction) : base(repairFraction) { }

	public override void OnReceiveRemote() => WorldObject.RepairTick(Data);
}