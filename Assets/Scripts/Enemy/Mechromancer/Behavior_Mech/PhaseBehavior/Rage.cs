using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Rage")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Rage", message: "Rage", category: "Events", id: "11fb4755781b99f9c5862f16c20553f2")]
public sealed partial class Rage : EventChannel { }

