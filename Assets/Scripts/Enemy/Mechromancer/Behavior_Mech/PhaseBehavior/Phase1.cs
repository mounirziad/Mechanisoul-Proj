using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Phase1")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Phase1", message: "Starting Phase1", category: "Events", id: "316cb29d177d5c8ab257896080ca4d37")]
public sealed partial class Phase1 : EventChannel { }

