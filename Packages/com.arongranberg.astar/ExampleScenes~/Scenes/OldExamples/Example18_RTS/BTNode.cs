using System;
using UnityEngine;
using System.Linq;
using Pathfinding.RVO;

namespace Pathfinding.Examples.RTS {
	public enum Status {
		Invalid,
		Failure,
		Success,
		Running
	};

	public class BTContext {
		public RTSUnit unit;
		public Transform transform;
		public Animator animator;
	}

	/// <summary>Implements a simple behavior tree. This is the base class for all nodes in the tree.</summary>
	public abstract class BTNode {
		protected Status lastStatus;
		public Status Tick (BTContext ctx) {
			if (lastStatus == Status.Invalid) OnInit(ctx);
			lastStatus = DoTick(ctx);
			if (lastStatus == Status.Invalid) throw new System.Exception();
			return lastStatus;
		}

		public void Terminate (BTContext ctx) {
			OnTerminate(ctx);
			lastStatus = Status.Invalid;
		}

		protected virtual void OnInit (BTContext ctx) {
		}

		protected virtual void OnTerminate (BTContext ctx) {
		}

		protected abstract Status DoTick(BTContext ctx);
	}

	public class BTTransparent : BTNode {
		public BTNode child;
		protected override void OnTerminate (BTContext ctx) {
			child.Terminate(ctx);
		}

		protected override Status DoTick (BTContext ctx) {
			return child.Tick(ctx);
		}
	}

	public class Once : BTNode {
		public BTNode child;

		public Once (BTNode child) { this.child = child; }

		protected override void OnTerminate (BTContext ctx) {
			if (lastStatus == Status.Running) child.Terminate(ctx);
		}

		protected override Status DoTick (BTContext ctx) {
			if (lastStatus == Status.Success) return Status.Success;
			var s = child.Tick(ctx);
			if (s == Status.Success) {
				child.Terminate(ctx);
			}
			return s;
		}
	}

	public class SimpleAction : BTNode {
		public System.Action<BTContext> action;
		public SimpleAction (System.Action<BTContext> action) { this.action = action; }

		protected override Status DoTick (BTContext ctx) {
			action(ctx);
			return Status.Success;
		}
	}

	public class Condition : BTNode {
		public System.Func<BTContext, bool> predicate;
		public Condition (System.Func<BTContext, bool> predicate) { this.predicate = predicate; }

		protected override Status DoTick (BTContext ctx) {
			return predicate(ctx) ? Status.Success : Status.Failure;
		}
	}

	public class BTSequence : BTNode {
		public BTNode[] children;
		int childIndex = -1;

		public BTSequence (BTNode[] children) {
			this.children = children;
		}

		protected override void OnInit (BTContext ctx) {
			childIndex = 0;
		}

		protected override void OnTerminate (BTContext ctx) {
			for (int i = 0; i <= childIndex; i++) {
				children[i].Terminate(ctx);
			}
			childIndex = -1;
		}

		protected override Status DoTick (BTContext ctx) {
			int i;

			for (i = 0; i < children.Length; i++) {
				var s = children[i].Tick(ctx);
				if (s != Status.Success) {
					// Terminate all nodes that executed the last frame, but did not execute this frame
					for (int j = i + 1; j <= childIndex; j++) children[j].Terminate(ctx);
					childIndex = i;
					return s;
				}
			}
			childIndex = i - 1;
			return Status.Success;
		}
	}

	public class BTSelector : BTNode {
		public BTNode[] children;
		int childIndex = -1;

		public BTSelector (BTNode[] children) {
			this.children = children;
		}

		protected override void OnInit (BTContext ctx) {
		}

		protected override void OnTerminate (BTContext ctx) {
			for (int i = 0; i <= childIndex; i++) {
				children[i].Terminate(ctx);
			}
			childIndex = -1;
		}

		protected override Status DoTick (BTContext ctx) {
			int i;

			for (i = 0; i < children.Length; i++) {
				var s = children[i].Tick(ctx);
				if (s != Status.Failure) {
					// Terminate all nodes that executed the last frame, but did not execute this frame
					for (int j = i + 1; j <= childIndex; j++) children[j].Terminate(ctx);

					childIndex = i;
					return s;
				}
			}
			childIndex = i - 1;
			return Status.Failure;
		}
	}

	class Binding<T> {
		T val;
		public System.Func<T> getter;
		public System.Action<T> setter;
		public T value {
			get {
				if (getter != null) return getter();
				return val;
			}
			set {
				if (setter != null) setter(value);
				val = value;
			}
		}
	}

	public struct Value<T> {
		T val;
		Binding<T> binding;
		public T value {
			get {
				if (binding != null) return binding.value;
				return val;
			}
			set {
				if (binding != null) binding.value = value;
				val = value;
			}
		}

		public void Bind (ref Value<T> other) {
			if (other.binding != null && binding == null) binding = other.binding;
			else if (binding == null && other.binding != null) other.binding = binding;
			else if (binding == null) binding = other.binding = new Binding<T>();
			else throw new System.Exception("Too complex binding");
		}

		public void Bind (System.Func<T> other) {
			if (binding != null) throw new System.Exception("Already has a binding");
			binding = new Binding<T>();
			binding.getter = other;
			binding.setter = _ => {
				throw new System.InvalidOperationException("Trying to assign a value which has been bound as read-only (using a delegate)");
			};
		}

		public Value<T> Bound {
			get {
				var r = this;
				if (binding == null) Bind(ref r);
				return r;
			}
		}

		public Value (System.Func<T> getter) {
			val = default(T);
			binding = null;
			Bind(getter);
		}

		public static implicit operator Value<T>(System.Func<T> getter) {
			var val = new Value<T>();

			val.Bind(getter);
			return val;
		}
	}

	public class BTMove : BTNode {
		public Value<Vector3> destination;

		public BTMove (Value<Vector3> destination) {
			this.destination = destination;
		}

		protected override void OnInit (BTContext ctx) {
			ctx.unit.SetDestination(destination.value, MovementMode.Move);
		}

		protected override Status DoTick (BTContext ctx) {
			var dest = destination.value;

			if ((Time.frameCount % 100) == 0) ctx.unit.SetDestination(dest, MovementMode.Move);
			if (VectorMath.SqrDistanceXZ(ctx.transform.position, dest) < 0.5f * 0.5f) {
				return Status.Success;
			} else {
				return Status.Running;
			}
		}
	}

	public class FindClosestUnit : BTNode {
		public Value<RTSUnit> target;
		public bool reserve;
		RTSUnit.Type type;

		public FindClosestUnit (RTSUnit.Type type) {
			this.type = type;
		}

		protected override void OnTerminate (BTContext ctx) {
			if (reserve && target.value != null) {
				if (target.value.reservedBy != ctx.unit) throw new System.Exception();
				target.value.reservedBy = null;
			}
			target.value = null;
		}

		RTSUnit FindClosest (Vector3 point) {
			var units = RTSManager.instance.units.units;
			RTSUnit closest = null;
			var dist = float.PositiveInfinity;

			for (int i = 0; i < units.Count; i++) {
				var unit = units[i];
				if (unit.type != type || (reserve && unit.reservedBy != null)) {
					continue;
				}
				if (unit.resource != null && !unit.resource.harvestable) {
					continue;
				}

				var d = (unit.transform.position - point).sqrMagnitude;
				if (d < dist) {
					dist = d;
					closest = unit;
				}
			}
			return closest;
		}

		protected override Status DoTick (BTContext ctx) {
			if (target.value != null) {
				return Status.Success;
			}

			target.value = FindClosest(ctx.transform.position);
			if (target.value != null) {
				if (reserve) target.value.reservedBy = ctx.unit;
				return Status.Success;
			}
			return Status.Failure;
		}
	}

	static class Behaviors {
		public static BTNode HarvestBehavior () {
			var reserve = new FindClosestUnit(RTSUnit.Type.ResourceCrystal) { reserve = true };
			var dropoff = new FindClosestUnit(RTSUnit.Type.HarvesterDropoff) { reserve = true };
			var dropoffQueue = new FindClosestUnit(RTSUnit.Type.HarvesterDropoffQueue);

			return new BTSelector(new BTNode[] {
				new HarvestMode() {
					child = new BTSelector(new BTNode[] {
						new BTSequence(new BTNode[] {
							new Condition(ctx => ctx.unit.storedCrystals > 0),
							new BTSequence(new BTNode[] {
								dropoff,
								new BTMove(new Value<Vector3>(() => dropoff.target.value.transform.position)),
								new SimpleAction(ctx => {
									ctx.unit.owner.resources.AddResource(RTSUnit.Type.ResourceCrystal, ctx.unit.storedCrystals);
									ctx.unit.storedCrystals = 0;
								}),
							})
							//new Deposit(move1),
						}),
						new BTSequence(new BTNode[] {
							new Condition(ctx => ctx.unit.storedCrystals == 0),
							new BTSequence(new BTNode[] {
								reserve,
								new BTMove(new Value<Vector3>(() => reserve.target.value.transform.position)),
								new Harvest { resource = new Value<RTSHarvestableResource>(() => reserve.target.value.resource), duration = 5 },
							}),
						})
					})
				},
				new BTSequence(new BTNode[] {
					dropoffQueue,
					new BTMove(new Value<Vector3>(() => dropoffQueue.target.value.transform.position)),
				})
			});
		}
	}

	public class HarvestMode : BTTransparent {
		protected override void OnTerminate (BTContext ctx) {
			ctx.unit.GetComponent<RVOController>().layer = RVO.RVOLayer.Layer2;
			base.OnTerminate(ctx);
		}

		protected override Status DoTick (BTContext ctx) {
			var s = base.DoTick(ctx);

			ctx.unit.GetComponent<RVOController>().layer = s == Status.Running ? RVO.RVOLayer.Layer3 : RVO.RVOLayer.Layer2;
			return s;
		}
	}

	public class Harvest : BTNode {
		public Value<RTSHarvestableResource> resource;
		public float duration = 5;
		float time;

		protected override void OnInit (BTContext ctx) {
			ctx.animator.SetBool("harvesting", true);

			//ctx.unit.locked = true;
		}

		protected override void OnTerminate (BTContext ctx) {
			Debug.Log("Terminated harvesting");
			ctx.animator.SetBool("harvesting", false);
		}

		protected override Status DoTick (BTContext ctx) {
			time += Time.deltaTime;
			if (time > duration) {
				ctx.animator.SetBool("harvesting", false);
				if (ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("RTSHarvesterHarvesting") || ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("RTSHarvesterHarvestingExit")) {
					return Status.Running;
				} else {
					ctx.unit.storedCrystals += 50;
					resource.value.value -= 50;
					time = 0;
					//ctx.unit.locked = false;
					return Status.Success;
				}
			} else {
				return Status.Running;
			}
		}
	}

	public class BTHarvest {
		/*RTSCommandMove moveToDepositPoint;
		RTSCommandMove moveToHarvestPoint;

		void Deposit () {

		}

		public override void Tick () {
		    if (HasResources()) {
		        if (moveToDepositPoint.Tick() == Success) {
		            Deposit();
		        }
		    } else {
		        var target = ReserveTarget();
		        moveToHarvestPoint.target = target;
		        if (moveToHarvestPoint.Tick() == Success) {
		            Harvest(target);
		        }
		    }
		}*/
	}
}
