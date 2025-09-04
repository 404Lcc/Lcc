using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Drawing;

	[HelpURL("https://arongranberg.com/astar/documentation/stable/animationlink.html")]
	public class AnimationLink : NodeLink2 {
		public string clip;
		public float animSpeed = 1;
		public bool reverseAnim = true;

		public GameObject referenceMesh;
		public LinkClip[] sequence;
		public string boneRoot = "bn_COG_Root";

		[System.Serializable]
		public class LinkClip {
			public AnimationClip clip;
			public Vector3 velocity;
			public int loopCount = 1;

			public string name {
				get {
					return clip != null ? clip.name : "";
				}
			}
		}

		static Transform SearchRec (Transform tr, string name) {
			int childCount = tr.childCount;

			for (int i = 0; i < childCount; i++) {
				Transform ch = tr.GetChild(i);
				if (ch.name == name) return ch;
				else {
					Transform rec = SearchRec(ch, name);
					if (rec != null) return rec;
				}
			}
			return null;
		}

		public void CalculateOffsets (List<Vector3> trace, out Vector3 endPosition) {
			//Vector3 opos = transform.position;
			endPosition = transform.position;
			if (referenceMesh == null) return;

			GameObject ob = GameObject.Instantiate(referenceMesh, transform.position, transform.rotation) as GameObject;
			ob.hideFlags = HideFlags.HideAndDontSave;

			Transform root = SearchRec(ob.transform, boneRoot);
			if (root == null) throw new System.Exception("Could not find root transform");

			Animation anim = ob.GetComponent<Animation>();
			if (anim == null) anim = ob.AddComponent<Animation>();

			for (int i = 0; i < sequence.Length; i++) {
				anim.AddClip(sequence[i].clip, sequence[i].clip.name);
			}

			Vector3 prevOffset = Vector3.zero;
			Vector3 position = transform.position;
			Vector3 firstOffset = Vector3.zero;

			for (int i = 0; i < sequence.Length; i++) {
				LinkClip c = sequence[i];
				if (c == null) {
					endPosition = position;
					return;
				}

				anim[c.clip.name].enabled = true;
				anim[c.clip.name].weight = 1;

				for (int repeat = 0; repeat < c.loopCount; repeat++) {
					anim[c.clip.name].normalizedTime = 0;
					anim.Sample();
					Vector3 soffset = root.position - transform.position;

					if (i > 0) {
						position += prevOffset - soffset;
					} else {
						firstOffset = soffset;
					}

					for (int t = 0; t <= 20; t++) {
						float tf = t/20.0f;
						anim[c.clip.name].normalizedTime = tf;
						anim.Sample();
						Vector3 tmp = position + (root.position-transform.position) + c.velocity*tf*c.clip.length;
						trace.Add(tmp);
					}
					position = position + c.velocity*1*c.clip.length;

					anim[c.clip.name].normalizedTime = 1;
					anim.Sample();
					Vector3 eoffset = root.position - transform.position;
					prevOffset = eoffset;
				}

				anim[c.clip.name].enabled = false;
				anim[c.clip.name].weight = 0;
			}

			position += prevOffset - firstOffset;

			GameObject.DestroyImmediate(ob);

			endPosition = position;
		}

		public override void DrawGizmos () {
			base.DrawGizmos();
			List<Vector3> buffer = Pathfinding.Pooling.ListPool<Vector3>.Claim();
			Vector3 endPosition = Vector3.zero;
			CalculateOffsets(buffer, out endPosition);
			for (int i = 0; i < buffer.Count-1; i++) {
				Draw.Line(buffer[i], buffer[i+1], Color.blue);
			}
		}
	}
}
