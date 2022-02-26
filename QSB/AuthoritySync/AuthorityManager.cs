﻿using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System.Collections.Generic;

namespace QSB.AuthoritySync;

public static class AuthorityManager
{
	#region host only

	/// <summary>
	/// whoever is first gets authority
	/// </summary>
	private static readonly Dictionary<NetworkIdentity, (List<uint> AuthQueue, bool ServerIsDefaultOwner)> _authQueue = new();

	public static void RegisterAuthQueue(this NetworkIdentity identity, bool serverIsDefaultOwner) => _authQueue.Add(identity, (new List<uint>(), serverIsDefaultOwner));
	public static void UnregisterAuthQueue(this NetworkIdentity identity) => _authQueue.Remove(identity);

	public static void ServerUpdateAuthQueue(this NetworkIdentity identity, uint id, AuthQueueAction action)
	{
		var (authQueue, serverIsDefaultOwner) = _authQueue[identity];
		var defaultOwner = serverIsDefaultOwner ? QSBPlayerManager.LocalPlayerId : uint.MaxValue;
		var oldOwner = authQueue.Count != 0 ? authQueue[0] : defaultOwner;

		switch (action)
		{
			case AuthQueueAction.Add:
				authQueue.SafeAdd(id);
				break;

			case AuthQueueAction.Remove:
				authQueue.Remove(id);
				break;

			case AuthQueueAction.Force:
				authQueue.Remove(id);
				authQueue.Insert(0, id);
				break;
		}

		var newOwner = authQueue.Count != 0 ? authQueue[0] : defaultOwner;
		if (oldOwner != newOwner)
		{
			SetAuthority(identity, newOwner);
		}
	}

	/// <summary>
	/// transfer authority to a different client
	/// </summary>
	public static void OnDisconnect(NetworkConnectionToClient conn)
	{
		var id = conn.GetPlayerId();
		foreach (var identity in _authQueue.Keys)
		{
			identity.ServerUpdateAuthQueue(id, AuthQueueAction.Remove);
		}
	}

	public static void SetAuthority(this NetworkIdentity identity, uint id)
	{
		var oldConn = identity.connectionToClient;
		var newConn = id != uint.MaxValue ? id.GetNetworkConnection() : null;

		if (oldConn == newConn)
		{
			return;
		}

		identity.RemoveClientAuthority();

		if (newConn != null)
		{
			identity.AssignClientAuthority(newConn);
		}

		// DebugLog.DebugWrite($"{identity.NetId}:{identity.gameObject.name} - "
		// + $"set authority to {id}");
	}

	#endregion

	#region any client

	public static void UpdateAuthQueue(this NetworkIdentity identity, AuthQueueAction action) =>
		new AuthQueueMessage(identity.netId, action).Send();

	#endregion
}