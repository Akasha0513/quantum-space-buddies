﻿using OWML.Common;
using OWML.Utils;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QuantumUNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace QSB.ShipSync
{
	class ShipManager : MonoBehaviour
	{
		public static ShipManager Instance;

		public InteractZone HatchInteractZone;
		public HatchController HatchController;
		public ShipTractorBeamSwitch ShipTractorBeam;

		private uint _currentFlyer = uint.MaxValue;
		public uint CurrentFlyer
		{
			get => _currentFlyer;
			set
			{
				if (_currentFlyer != uint.MaxValue && value != uint.MaxValue)
				{
					DebugLog.ToConsole($"Warning - Trying to set current flyer while someone is still flying? Current:{_currentFlyer}, New:{value}", MessageType.Warning);
				}
				_currentFlyer = value;
			}
		}

		private void Awake()
		{
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
			Instance = this;
		}

		private void OnSceneLoaded(OWScene scene)
		{
			if (scene == OWScene.EyeOfTheUniverse)
			{
				return;
			}

			var shipTransform = GameObject.Find("Ship_Body");
			HatchController = shipTransform.GetComponentInChildren<HatchController>();
			HatchInteractZone = HatchController.GetComponent<InteractZone>();
			ShipTractorBeam = Resources.FindObjectsOfTypeAll<ShipTractorBeamSwitch>().First();

			var sphereShape = HatchController.GetComponent<SphereShape>();
			sphereShape.radius = 2.5f;
			sphereShape.center = new Vector3(0, 0, 1);

			if (QSBCore.IsServer)
			{
				DebugLog.DebugWrite($"SPAWN SHIP");
				QNetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.ShipPrefab));
			}
		}
	}
}