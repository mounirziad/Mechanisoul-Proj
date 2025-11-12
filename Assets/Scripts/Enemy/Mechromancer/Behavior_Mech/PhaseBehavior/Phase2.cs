using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Phase2")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Phase2", message: "Starting Phase2", category: "Events", id: "83362cd94392d4643e14caff35000ee7")]
public sealed partial class Phase2 : EventChannel { }

