﻿using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages;

internal class QuantumShuffleMessage : QSBWorldObjectMessage<QSBQuantumShuffleObject, int[]>
{
	public QuantumShuffleMessage(int[] indexArray) : base(indexArray) { }

	public override void OnReceiveRemote() => WorldObject.ShuffleObjects(Data);
}