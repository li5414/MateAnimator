using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("")]
public class AMAudioTrack : AMTrack {

	[SerializeField]
    AudioSource audioSource;

    bool paused;

	protected override void SetSerializeObject(UnityEngine.Object obj) {
		audioSource = obj as AudioSource;
        if(audioSource)
            audioSource.playOnAwake = false;
	}
	
	protected override UnityEngine.Object GetSerializeObject(GameObject targetGO) {
		return targetGO ? targetGO.audio : audioSource;
	}

    public override string getTrackType() {
        return "Audio";
    }

	public override string GetRequiredComponent() {
		return "AudioSource";
	}

    public override void updateCache(AMITarget target) {
        base.updateCache(target);

        if(audioSource)
            audioSource.playOnAwake = false;
    }

    // add a new key
    public void addKey(AMITarget itarget, OnAddKey addCall, int _frame, AudioClip _clip, bool _loop) {
        foreach(AMAudioKey key in keys) {
            // if key exists on frame, update key
            if(key.frame == _frame) {
                key.audioClip = _clip;
                key.loop = _loop;
                // update cache
				updateCache(itarget);
            }
        }
        AMAudioKey a = addCall(gameObject, typeof(AMAudioKey)) as AMAudioKey;
        a.frame = _frame;
        a.audioClip = _clip;
        a.loop = _loop;
        // add a new key
        keys.Add(a);
        // update cache
		updateCache(itarget);
    }

    // sample audio between frames
	public void sampleAudio(AMITarget target, float frame, float speed, int frameRate, bool forcePlay) {
		AudioSource src = GetTarget(target) as AudioSource;
		if(!src) return;
        float time;
        for(int i = keys.Count - 1; i >= 0; i--) {
            AMAudioKey key = keys[i] as AMAudioKey;
            if(!key.audioClip) return;
            if(key.frame <= frame) {
                if(!forcePlay && src.isPlaying && src.clip == key.audioClip) return;

                // get time
                time = ((frame - key.frame) / frameRate);
                // if loop is set to false and is beyond length, then return
                if(!key.loop && time > key.audioClip.length) return;
                // find time based on length
                time = time % key.audioClip.length;
                src.Stop();
				src.clip = null;
                src.clip = key.audioClip;
                src.loop = key.loop;
				src.time = time;
				src.pitch = speed;

				src.Play();

                return;
            }
        }
    }
    // sample audio at frame
	public void sampleAudioAtFrame(AMITarget target, int frame, float speed, int frameRate) {
		AudioSource src = GetTarget(target) as AudioSource;
		if(!src) return;

        for(int i = keys.Count - 1; i >= 0; i--) {
            if(keys[i].frame == frame) {
				if(src.isPlaying) src.Stop();
				src.clip = null;
				src.clip = (keys[i] as AMAudioKey).audioClip;
				src.time = 0f;
				src.loop = (keys[i] as AMAudioKey).loop;
				src.pitch = speed;
				src.Play();
                return;
            }
        }
    }
	public void stopAudio(AMITarget target) {
		AudioSource src = GetTarget(target) as AudioSource;
		if(!src) return;
		src.Stop();
        paused = false;
    }

    public void resumeAudio(AMITarget target) {
        AudioSource src = GetTarget(target) as AudioSource;
        if(!src) return;
        if(paused) {
            src.Play();
            paused = false;
        }
    }

    public void pauseAudio(AMITarget target) {
        AudioSource src = GetTarget(target) as AudioSource;
        if(!src) return;
        if(src.isPlaying) {
            src.Pause();
            paused = true;
        }
    }

    public ulong getTimeInSamples(int frequency, float time) {
        return (ulong)((44100 / frequency) * frequency * time);
    }

	public override AnimatorTimeline.JSONInit getJSONInit(AMITarget target) {
        // no initial values to set
        return null;
    }

	public override List<GameObject> getDependencies(AMITarget target) {
		AudioSource src = GetTarget(target) as AudioSource;
        List<GameObject> ls = new List<GameObject>();
		if(src) ls.Add(src.gameObject);
        return ls;
    }

	public override List<GameObject> updateDependencies(AMITarget target, List<GameObject> newReferences, List<GameObject> oldReferences) {
		AudioSource src = GetTarget(target) as AudioSource;
        List<GameObject> lsFlagToKeep = new List<GameObject>();
		if(!src) return lsFlagToKeep;
        for(int i = 0; i < oldReferences.Count; i++) {
			if(oldReferences[i] == src.gameObject) {
                AudioSource _audioSource = (AudioSource)newReferences[i].GetComponent(typeof(AudioSource));
                // missing audiosource
                if(!_audioSource) {
					Debug.LogWarning("Animator: Audio Track component 'AudioSource' not found on new reference for GameObject '" + src.gameObject.name + "'. Duplicate not replaced.");
                    lsFlagToKeep.Add(oldReferences[i]);
                    return lsFlagToKeep;
                }
                SetTarget(target, newReferences[i].transform);
                break;
            }
        }
        return lsFlagToKeep;
    }

    protected override void DoCopy(AMTrack track) {
        (track as AMAudioTrack).audioSource = audioSource;
    }
}
