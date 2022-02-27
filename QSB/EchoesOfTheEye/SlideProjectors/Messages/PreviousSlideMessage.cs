﻿using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages
{
	internal class PreviousSlideMessage : QSBWorldObjectMessage<QSBSlideProjector>
	{
		public override void OnReceiveRemote() => WorldObject.PreviousSlide();
	}
}