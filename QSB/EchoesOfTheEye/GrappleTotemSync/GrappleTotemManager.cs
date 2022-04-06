﻿using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.GrappleTotemSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.GrappleTotemSync;

public class GrappleTotemManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBGrappleTotem, LanternZoomPoint>();
}
