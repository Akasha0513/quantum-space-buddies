﻿using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class IlluminatingLanternsMessage : QSBWorldObjectMessage<QSBLightSensor, int[]>
{
	public IlluminatingLanternsMessage(IEnumerable<DreamLanternController> lanterns) :
		base(lanterns.Select(x => x.GetWorldObject<QSBDreamLantern>().ObjectId).ToArray()) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._illuminatingDreamLanternList.Clear();
		WorldObject.AttachedObject._illuminatingDreamLanternList.AddRange(
			Data.Select(x => x.GetWorldObject<QSBDreamLantern>().AttachedObject));
	}
}
