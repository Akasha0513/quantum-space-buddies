﻿using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public abstract class QSBRaftCarrier<T> : WorldObject<T>, IQSBRaftCarrier where T : RaftCarrier
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public async UniTask Undock(QSBRaft qsbRaft, CancellationToken ct)
	{
		DebugLog.DebugWrite($"TODO: {this} undock {qsbRaft}");
		await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: ct);
	}

	public async UniTask Dock(QSBRaft qsbRaft, CancellationToken ct)
	{
		DebugLog.DebugWrite($"TODO: {this} undock {qsbRaft}");
		await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: ct);
	}
}

public interface IQSBRaftCarrier : IWorldObject
{
	UniTask Undock(QSBRaft qsbRaft, CancellationToken ct);
	UniTask Dock(QSBRaft qsbRaft, CancellationToken ct);
}
