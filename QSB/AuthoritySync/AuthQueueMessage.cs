﻿using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.AuthoritySync;

/// <summary>
/// always sent to host
/// </summary>
public class AuthQueueMessage : QSBMessage<(uint NetId, AuthQueueAction Action)>
{
	public AuthQueueMessage(uint netId, AuthQueueAction action) : base((netId, action)) =>
		To = 0;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => NetworkServer.spawned[Data.NetId].ServerUpdateAuthQueue(From, Data.Action);
}

public enum AuthQueueAction
{
	/// <summary>
	/// add player to the queue
	/// </summary>
	Add,
	/// <summary>
	/// remove player from the queue
	/// </summary>
	Remove,
	/// <summary>
	/// add player to the queue and force them to the front
	/// </summary>
	Force
}