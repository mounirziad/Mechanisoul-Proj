using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Death")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Death", message: "Starting Death", category: "Events", id: "7ab0f859c4769869c6d9a3387ab3349c")]
public sealed partial class Death : EventChannel { }

