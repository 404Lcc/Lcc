using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsaudio.html")]
	public class RTSAudio : VersionedMonoBehaviour {
		List<Source> sources = new List<Source>();

		class Source {
			public AudioSource source;

			public bool available {
				get {
					return !source.isPlaying;
				}
			}

			public void Play (AudioClip clip) {
				source.PlayOneShot(clip);
			}
		}

		Source GetSource () {
			for (int i = 0; i < sources.Count; i++) {
				if (sources[i].available) {
					return sources[i];
				}
			}

			var go = new GameObject("Source");
			go.transform.SetParent(transform, false);
			var source = new Source {
				source = go.AddComponent<AudioSource>()
			};

			sources.Add(source);
			return source;
		}

		public void Play (AudioClip clip) {
			GetSource().Play(clip);
		}
	}
}
