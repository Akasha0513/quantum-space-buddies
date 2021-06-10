﻿using OWML.Common;
using OWML.Utils;
using QSB.ShipSync.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.DeathSync
{
	public class RespawnOnDeath : MonoBehaviour
	{
		public static RespawnOnDeath Instance;

		public readonly DeathType[] AllowedDeathTypes = {
			DeathType.BigBang,
			DeathType.Supernova,
			DeathType.TimeLoop
		};

		private readonly Vector3 ShipContainerOffset = new Vector3(-16.45f, -52.67f, 227.39f);
		private readonly Quaternion ShipContainerRotation = Quaternion.Euler(-76.937f, 1.062f, -185.066f);

		private SpawnPoint _shipSpawnPoint;
		private SpawnPoint _playerSpawnPoint;
		private OWRigidbody _shipBody;
		private PlayerSpawner _playerSpawner;
		private FluidDetector _fluidDetector;
		private PlayerResources _playerResources;
		private ShipComponent[] _shipComponents;
		private HatchController _hatchController;
		private ShipCockpitController _cockpitController;
		private PlayerSpacesuit _spaceSuit;
		private ShipTractorBeamSwitch _shipTractorBeam;
		private SuitPickupVolume[] _suitPickupVolumes;

		public void Awake() => Instance = this;

		public void Init()
		{
			var playerTransform = Locator.GetPlayerTransform();
			_playerResources = playerTransform.GetComponent<PlayerResources>();
			_spaceSuit = Locator.GetPlayerSuit();
			_playerSpawner = FindObjectOfType<PlayerSpawner>();
			_shipTractorBeam = FindObjectOfType<ShipTractorBeamSwitch>();
			_suitPickupVolumes = FindObjectsOfType<SuitPickupVolume>();

			_fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();

			_playerSpawnPoint = GetSpawnPoint();
			_shipSpawnPoint = GetSpawnPoint(true);

			var shipTransform = Locator.GetShipTransform();
			if (shipTransform == null)
			{
				DebugLog.ToConsole($"Warning - Init() ran when ship was null?", MessageType.Warning);
				return;
			}
			_shipComponents = shipTransform.GetComponentsInChildren<ShipComponent>();
			_hatchController = shipTransform.GetComponentInChildren<HatchController>();
			_cockpitController = shipTransform.GetComponentInChildren<ShipCockpitController>();
			_shipBody = Locator.GetShipBody();

			if (_shipSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _shipSpawnPoint is null in Init()!", MessageType.Warning);
				return;
			}

			// Move debug spawn point to initial ship position (so ship doesnt spawn in space!)
			var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
			_shipSpawnPoint.transform.SetParent(timberHearth);
			_shipSpawnPoint.transform.localPosition = ShipContainerOffset;
			_shipSpawnPoint.transform.localRotation = ShipContainerRotation;
		}

		public void ResetPlayer()
		{
			DebugLog.DebugWrite($"Trying to reset player.");
			if (_playerSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
				Init();
			}

			// Cant use _playerSpawner.DebugWarp because that will warp the ship if the player is in it
			var playerBody = Locator.GetPlayerBody();
			playerBody.WarpToPositionRotation(_playerSpawnPoint.transform.position, _playerSpawnPoint.transform.rotation);
			playerBody.SetVelocity(_playerSpawnPoint.GetPointVelocity());
			_playerSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
			_playerSpawnPoint.AddObjectToTriggerVolumes(_fluidDetector.gameObject);
			_playerSpawnPoint.OnSpawnPlayer();

			_playerResources.SetValue("_isSuffocating", false);
			_playerResources.DebugRefillResources();
			_spaceSuit.RemoveSuit(true);

			foreach (var pickupVolume in _suitPickupVolumes)
			{
				var containsSuit = pickupVolume.GetValue<bool>("_containsSuit");
				var allowReturn = pickupVolume.GetValue<bool>("_allowSuitReturn");

				if (!containsSuit && allowReturn)
				{

					var interactVolume = pickupVolume.GetValue<MultipleInteractionVolume>("_interactVolume");
					var pickupSuitIndex = pickupVolume.GetValue<int>("_pickupSuitCommandIndex");

					pickupVolume.SetValue("_containsSuit", true);
					interactVolume.ChangePrompt(UITextType.SuitUpPrompt, pickupSuitIndex);

					var suitGeometry = pickupVolume.GetValue<GameObject>("_suitGeometry");
					var suitCollider = pickupVolume.GetValue<OWCollider>("_suitOWCollider");
					var toolGeometries = pickupVolume.GetValue<GameObject[]>("_toolGeometry");

					suitGeometry.SetActive(true);
					suitCollider.SetActivation(true);
					foreach (var geo in toolGeometries)
					{
						geo.SetActive(true);
					}
				}
			}
		}

		public void ResetShip()
		{
			DebugLog.DebugWrite($"Trying to reset ship.");
			if (!ShipTransformSync.LocalInstance.HasAuthority)
			{
				DebugLog.ToConsole($"Warning - Tried to reset ship when not in control!", MessageType.Warning);
				return;
			}

			if (_shipSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _shipSpawnPoint is null!", MessageType.Warning);
				Init();
			}

			if (_shipBody == null)
			{
				DebugLog.ToConsole($"Warning - Tried to reset ship, but the ship is null!", MessageType.Warning);
				return;
			}

			_shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
			_shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);

			foreach (var shipComponent in _shipComponents)
			{
				shipComponent.SetDamaged(false);
			}

			Invoke(nameof(ExitShip), 0.01f);
		}

		private void ExitShip()
		{
			DebugLog.DebugWrite($"Exit ship.");
			_cockpitController.Invoke("ExitFlightConsole");
			_cockpitController.Invoke("CompleteExitFlightConsole");
			_hatchController.SetValue("_isPlayerInShip", false);
			_hatchController.Invoke("OpenHatch");
			_shipTractorBeam.ActivateTractorBeam();
		}

		private SpawnPoint GetSpawnPoint(bool isShip = false)
		{
			var spawnList = _playerSpawner.GetValue<SpawnPoint[]>("_spawnList");
			if (spawnList == null)
			{
				DebugLog.ToConsole($"Warning - _spawnList was null for player spawner!", MessageType.Warning);
				return null;
			}
			return spawnList.FirstOrDefault(spawnPoint =>
					spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth
					&& spawnPoint.IsShipSpawn() == isShip);
		}
	}
}