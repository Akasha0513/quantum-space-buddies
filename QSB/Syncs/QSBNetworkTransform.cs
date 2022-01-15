﻿using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs
{
	public class QSBNetworkTransform : QSBNetworkBehaviour
	{
		protected override float SendInterval => 0.05f;

		protected Vector3 _prevPosition;
		protected Quaternion _prevRotation;

		protected override void UpdatePrevData()
		{
			_prevPosition = transform.position;
			_prevRotation = transform.rotation;
		}

		protected override bool HasChanged() =>
			Vector3.Distance(transform.position, _prevPosition) > 1E-05f ||
			Quaternion.Angle(transform.rotation, _prevRotation) > 1E-05f;

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			writer.Write(transform.position);
			writer.Write(transform.rotation);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			transform.position = reader.ReadVector3();
			transform.rotation = reader.ReadQuaternion();
		}
	}
}
