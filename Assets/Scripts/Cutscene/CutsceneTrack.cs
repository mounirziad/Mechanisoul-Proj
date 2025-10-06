using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9654f, 0.2706f, 0.5f)]
[TrackClipType(typeof(CutsceneClip))]
[TrackBindingType(typeof(CutsceneManager))]
public class CutsceneTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<CutsceneMixerBehaviour>.Create(graph, inputCount);
    }
}