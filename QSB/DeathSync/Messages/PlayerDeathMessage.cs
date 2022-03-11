﻿using Mirror;
using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;
using QSB.RespawnSync;
using QSB.Utility;

namespace QSB.DeathSync.Messages;

public class PlayerDeathMessage : QSBMessage<DeathType>
{
	private int NecronomiconIndex;

	public PlayerDeathMessage(DeathType type) : base(type) =>
		NecronomiconIndex = Necronomicon.GetRandomIndex(type);

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(NecronomiconIndex);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		NecronomiconIndex = reader.Read<int>();
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		RespawnManager.Instance.OnPlayerDeath(player);
		ClientStateManager.Instance.OnDeath();
	}

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		var playerName = player.Name;
		var deathMessage = Necronomicon.GetPhrase(Data, NecronomiconIndex);
		if (deathMessage != null)
		{
			DebugLog.ToAll(string.Format(deathMessage, playerName));
		}

		RespawnManager.Instance.OnPlayerDeath(player);
	}
}