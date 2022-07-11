﻿using UnityEngine;

namespace QSB.Player;

public class RemotePlayerVelocity : MonoBehaviour
{
	private Vector3 _prevRelPosition;

	public Vector3 Velocity { get; private set; }

	public void FixedUpdate()
	{
		var reference = Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().transform;
		var currentRelPosition = reference.InverseTransformPoint(transform.position);
		Velocity = (currentRelPosition - _prevRelPosition) / Time.fixedDeltaTime;
		_prevRelPosition = reference.InverseTransformPoint(transform.position);
	}
}
