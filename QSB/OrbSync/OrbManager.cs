﻿using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.OrbSync
{
    public class OrbManager : MonoBehaviour
    {
        public static OrbManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void BuildOrbSlots()
        {
            DebugLog.DebugWrite("Building QSBOrbSlots...");

            var orbSlots = Resources.FindObjectsOfTypeAll<NomaiInterfaceSlot>();
            for (var id = 0; id < orbSlots.Length; id++)
            {
                var qsbOrbSlot = WorldRegistry.GetObject<QSBOrbSlot>(id) ?? new QSBOrbSlot();
                qsbOrbSlot.Init(orbSlots[id], id);
            }

            DebugLog.DebugWrite($"Finished orb build with {WorldRegistry.OldOrbList.Count} interface orbs and {WorldRegistry.OrbSyncList.Count} orb syncs.");
        }

        public void BuildOrbs()
        {
            DebugLog.DebugWrite("Building orb syncs...");
            WorldRegistry.OldOrbList.Clear();

            WorldRegistry.OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
            if (NetworkServer.active)
            {
                DebugLog.DebugWrite("IS SERVER - INSTANTIATING!");
                WorldRegistry.OrbSyncList.Clear();
                WorldRegistry.OldOrbList.ForEach(x => NetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
            }
        }

        public void QueueBuildSlots()
        {
            DebugLog.DebugWrite("Queueing build of QSBOrbSlots...");
            QSB.Helper.Events.Unity.RunWhen(() => QSB.HasWokenUp, BuildOrbSlots);
        }
    }
}