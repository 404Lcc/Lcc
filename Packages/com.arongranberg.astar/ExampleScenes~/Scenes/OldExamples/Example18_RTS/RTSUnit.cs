using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;

namespace Pathfinding.Examples.RTS {
	using Pathfinding;
	using Pathfinding.RVO;
	using Pathfinding.Util;
	using Pathfinding.Pooling;

	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsunit.html")]
	public class RTSUnit : VersionedMonoBehaviour {
		public GameObject selectionIndicator;
		public GameObject deathEffect;
		public int team;
		public float detectionRange;

		public float maxHealth;

		public enum Type {
			Infantry,
			Heavy,
			Worker,
			Harvester,

			HarvesterDropoff = 100,
			HarvesterDropoffQueue,
			ResourceCrystal = 200,
		}

		public Type type;
		[System.NonSerialized]
		public float health;
		IAstarAI ai;
		RVOController rvo;
		MovementMode movementMode;
		Vector3 lastDestination;
		RTSUnit attackTarget;
		RTSWeapon weapon;
		float lastSeenAttackTarget = float.NegativeInfinity;
		bool reachedDestination;
		public int storedCrystals;
		public RTSUnit reservedBy;
		public bool locked;
		new Transform transform;

		/// <summary>Position at the start of the current frame</summary>
		protected Vector3 position;

		public System.Action<bool> onMakeActiveUnit;

		public RTSPlayer owner {
			get {
				return RTSManager.instance.GetPlayer(team);
			}
		}

		public bool selectionIndicatorEnabled {
			get {
				if (selectionIndicator == null) return false;
				return selectionIndicator.activeSelf;
			}
			set {
				if (selectionIndicator != null) selectionIndicator.SetActive(value);
			}
		}

		public RTSHarvestableResource resource {
			get {
				return GetComponent<RTSHarvestableResource>();
			}
		}

		public float radius {
			get {
				return rvo != null ? rvo.radius : 1f;
			}
		}

		bool mSelected;
		public bool selected {
			get {
				return mSelected;
			}
			set {
				mSelected = value;
				selectionIndicatorEnabled = value;
				if (value) {
					RTSManager.instance.units.OnSelected(this);
				} else {
					RTSManager.instance.units.OnDeselected(this);
				}
			}
		}

		public void OnMakeActiveUnit (bool active) {
			if (onMakeActiveUnit != null) onMakeActiveUnit(active);
		}

		public void SetDestination (Vector3 destination, MovementMode mode) {
			if (ai != null && this) {
				reachedDestination = false;
				movementMode = mode;
				ai.destination = lastDestination = destination;
				(ai as AIBase).rvoDensityBehavior.ClearDestinationReached();
				ai.SearchPath();
				if (mode == MovementMode.Move) {
					attackTarget = null;
				}
			}
		}

		protected override void Awake () {
			base.Awake();
			transform = (this as MonoBehaviour).transform;
			ai = GetComponent<IAstarAI>();
			rvo = GetComponent<RVOController>();
			weapon = GetComponent<RTSWeapon>();
		}

		static System.Action<RTSUnit[], int> OnUpdateDelegate;
		void OnEnable () {
			if (OnUpdateDelegate == null) OnUpdateDelegate = OnUpdate;
			RTSManager.instance.units.AddUnit(this);
			selected = false;
			health = maxHealth;
			movementMode = MovementMode.AttackMove;
			reachedDestination = true;
			if (ai != null) lastDestination = ai.destination;
			BatchedEvents.Add(this, BatchedEvents.Event.Update, OnUpdateDelegate);
		}

		void OnDisable () {
			BatchedEvents.Remove(this);
			if (RTSManager.instance != null) RTSManager.instance.units.RemoveUnit(this);
		}

		static void OnUpdate (RTSUnit[] units, int count) {
			// Get some lists and arrays from an object pool
			List<RTSUnit>[] unitsByOwner = ArrayPool<List<RTSUnit> >.ClaimWithExactLength(RTSManager.instance.PlayerCount);
			for (int i = 0; i < unitsByOwner.Length; i++) {
				unitsByOwner[i] = ListPool<RTSUnit>.Claim();
			}
			for (int i = 0; i < count; i++) {
				units[i].position = units[i].transform.position;
				unitsByOwner[units[i].owner.index].Add(units[i]);
			}
			for (int i = 0; i < count; i++) {
				units[i].OnUpdate(unitsByOwner);
			}

			// Release allocated lists back to a pool
			for (int i = 0; i < unitsByOwner.Length; i++) {
				ListPool<RTSUnit>.Release(ref unitsByOwner[i]);
			}
			ArrayPool<List<RTSUnit> >.Release(ref unitsByOwner, true);
		}

		// Update is called once per frame
		protected virtual void OnUpdate (List<RTSUnit>[] unitsByOwner) {
			if (ai == null) {
				// Stationary unit

				if (weapon != null) {
					float minDist = detectionRange*detectionRange;
					for (int player = 0; player < unitsByOwner.Length; player++) {
						if (!owner.IsHostile(RTSManager.instance.GetPlayer(player))) continue;

						for (int i = 0; i < unitsByOwner[player].Count; i++) {
							var unit = unitsByOwner[player][i];
							var dist = (unit.position - position).sqrMagnitude;
							if (dist < minDist) {
								attackTarget = unit;
								minDist = dist;
							}
						}
					}

					if (attackTarget != null) {
						if (!weapon.InRangeOf(attackTarget.position)) {
							attackTarget = null;
						} else {
							if (weapon.Aim(attackTarget)) {
								weapon.Attack(attackTarget);
							}
						}
					}
				}

				if (attackTarget != null) {
					lastSeenAttackTarget = Time.time;
				}
			} else {
				rvo.locked = false | locked;

				// this.reachedDestination will be true once the AI has reached its destination
				// and it will stay true until the next time SetDestination is called.
				reachedDestination |= (ai as AIBase).rvoDensityBehavior.reachedDestination;

				if (weapon != null) {
					bool canAttack = movementMode == MovementMode.AttackMove;
					// This takes into account path calculations as well as if the AI stops far away from the destination due to being part of a large group
					canAttack |= reachedDestination && movementMode == MovementMode.Move;

					if (canAttack) {
						float minDist = detectionRange*detectionRange;

						Profiler.BeginSample("Distance");
						var pos = position;
						for (int player = 0; player < unitsByOwner.Length; player++) {
							if (!owner.IsHostile(RTSManager.instance.GetPlayer(player))) continue;

							var enemies = unitsByOwner[player];
							for (int i = 0; i < enemies.Count; i++) {
								var enemy = enemies[i];
								var dist = (enemy.position - pos).sqrMagnitude;
								if (dist < minDist) {
									attackTarget = enemy;
									minDist = dist;
								}
							}
						}
						Profiler.EndSample();

						float rangeFuzz = 1.1f;
						if (attackTarget != null && (attackTarget.position - position).magnitude > detectionRange*rangeFuzz) {
							attackTarget = null;
						}

						bool wantsToAttack = false;

						if (attackTarget != null) {
							if (!weapon.InRangeOf(attackTarget.position)) {
								ai.destination = attackTarget.position;
							} else {
								wantsToAttack = true;
								if (weapon.Aim(attackTarget)) {
									weapon.Attack(attackTarget);
								}
							}
						}

						if (attackTarget != null) {
							lastSeenAttackTarget = Time.time;
						}

						if (!weapon.canMoveWhileAttacking && (wantsToAttack || weapon.isAttacking)) {
							rvo.locked = true;
						}
					}
				}

				// Move back to original destination in case we followed an enemy for some time
				if (Time.time - lastSeenAttackTarget > 2) {
					ai.destination = lastDestination;
				}
			}
		}

		public void Die () {
			StartCoroutine(DieCoroutine());
		}

		IEnumerator DieCoroutine () {
			yield return new WaitForEndOfFrame();
			if (deathEffect != null) GameObject.Instantiate(deathEffect, transform.position, transform.rotation);
			GameObject.Destroy(gameObject);
		}

		public void ApplyDamage (float damage) {
			health = Mathf.Clamp(health - damage, 0, maxHealth);
			if (health <= 0) {
				Die();
			}
		}
	}
}
