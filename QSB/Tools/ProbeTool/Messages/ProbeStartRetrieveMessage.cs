﻿using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeTool.Messages;

internal class ProbeStartRetrieveMessage : QSBMessage<float>
{
	public ProbeStartRetrieveMessage(float duration) => Data = duration;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		if (!player.IsReady || player.Probe == null)
		{
			return;
		}

		var probe = player.Probe;
		probe.OnStartRetrieve(Data);
	}
}