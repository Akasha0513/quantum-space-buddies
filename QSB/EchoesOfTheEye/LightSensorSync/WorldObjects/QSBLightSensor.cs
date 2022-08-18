﻿using Cysharp.Threading.Tasks;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

/// <summary>
/// BUG: this breaks in zone2.
/// the sector it's enabled in is bigger than the sector the zone2 walls are enabled in :(
/// maybe this can be fixed by making the collision group use the same sector.
/// </summary>
internal class QSBLightSensor : AuthWorldObject<SingleLightSensor>
{
	internal bool _locallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;


	public override bool CanOwn => AttachedObject.enabled;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new SetIlluminatedMessage(AttachedObject._illuminated) { To = to });
		if (AttachedObject._illuminatingDreamLanternList != null)
		{
			this.SendMessage(new IlluminatingLanternsMessage(AttachedObject._illuminatingDreamLanternList) { To = to });
		}
	}

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		// do this stuff here instead of Start, since world objects won't be ready by that point
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._sector != null)
			{
				if (AttachedObject._startIlluminated)
				{
					_locallyIlluminated = true;
					OnDetectLocalLight?.Invoke();
				}
			}
		});
	}
}
