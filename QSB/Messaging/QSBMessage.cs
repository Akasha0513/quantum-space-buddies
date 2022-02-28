﻿using Mirror;

namespace QSB.Messaging
{
	public abstract class QSBMessage
	{
		/// <summary>
		/// set automatically by Send
		/// </summary>
		internal uint From;
		/// <summary>
		/// (default) uint.MaxValue = send to everyone <br/>
		/// 0 = send to host
		/// </summary>
		public uint To = uint.MaxValue;

		/// <summary>
		/// call the base method when overriding
		/// </summary>
		public virtual void Serialize(NetworkWriter writer)
		{
			writer.Write(From);
			writer.Write(To);
		}

		/// <summary>
		/// call the base method when overriding
		/// <para/>
		/// note: no constructor is called before this,
		/// so fields won't be initialized.
		/// </summary>
		public virtual void Deserialize(NetworkReader reader)
		{
			From = reader.Read<uint>();
			To = reader.Read<uint>();
		}

		/// <summary>
		/// checked before calling either OnReceive
		/// </summary>
		public virtual bool ShouldReceive => true;
		public virtual void OnReceiveLocal() { }
		public virtual void OnReceiveRemote() { }

		public override string ToString() => GetType().Name;
	}

	public abstract class QSBMessage<T> : QSBMessage
	{
		protected T Value;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.Read<T>();
		}
	}

	public abstract class QSBMessage<T, U> : QSBMessage
	{
		protected T Value1;
		protected U Value2;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value1);
			writer.Write(Value2);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value1 = reader.Read<T>();
			Value2 = reader.Read<U>();
		}
	}
}