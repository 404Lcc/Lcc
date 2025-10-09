using System;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using TimeManagerCls = FishNet.Managing.Timing.TimeManager;

namespace FishNet.Component.Prediction
{
    public abstract class NetworkColliderBase : NetworkBehaviour
    {
        #region Types.
        protected struct CollisionData
        {
            /// <summary>
            /// Tick when entering collision.
            /// </summary>
            public uint EnterTick;
            /// <summary>
            /// Tick when exiting collision.
            /// </summary>
            public uint ExitTick;

            public CollisionData(uint enterTick) : this()
            {
                EnterTick = enterTick;
                ExitTick = Managing.Timing.TimeManager.UNSET_TICK;
            }

            public CollisionData(uint enterTick, uint exitTick) : this()
            {
                EnterTick = enterTick;
                ExitTick = exitTick;
            }
        }
        #endregion

        /// <summary>
        /// True to run collisions for colliders which are triggers, false to run collisions for colliders which are not triggers.
        /// </summary>
        [HideInInspector]
        protected bool IsTrigger;
        /// <summary>
        /// Maximum number of simultaneous hits to check for. Larger values decrease performance but allow detection to work for more overlapping colliders. Typically the default value of 16 is more than sufficient.
        /// </summary>
        [FormerlySerializedAs("_maximumSimultaneousHits")]
        [Tooltip("Maximum number of simultaneous hits to check for. Larger values decrease performance but allow detection to work for more overlapping colliders. Typically the default value of 16 is more than sufficient.")]
        [SerializeField]
        protected ushort MaximumSimultaneousHits = 16;
        /// <summary>
        /// Units to extend collision traces by. This is used to prevent missed overlaps when colliders do not intersect enough.
        /// </summary>
        [FormerlySerializedAs("_additionalSize")]
        [Tooltip("Units to extend collision traces by. This is used to prevent missed overlaps when colliders do not intersect enough.")]
        [Range(0f, 100f)]
        [SerializeField]
        protected float AdditionalSize = 0.1f;
        /// <summary>
        /// Layers to trace on. This is used when value is not nothing.
        /// </summary>
        [FormerlySerializedAs("_layers")]
        [Tooltip("Layers to trace on. This is used when value is not nothing.")]
        [SerializeField]
        protected LayerMask Layers = (LayerMask)0;
        /// <summary>
        /// True if colliders have been searched for at least once.
        /// We cannot check the null state on _colliders because Unity has a habit of initializing collections on it's own.
        /// </summary>
        private bool _collidersFound;
        /// <summary>
        /// Last layer of the gameObject.
        /// </summary>
        private int _lastGameObjectLayer = -1;
        /// <summary>
        /// Interactable layers for the layer of this gameObject.
        /// </summary>
        [HideInInspector]
        protected int InteractableLayers;

        protected virtual void Awake()
        {
            TryFindColliders(force: true);
            ;
        }

        public override void OnStartNetwork()
        {
            // Events needed by server and client.
            TimeManager.OnPostPhysicsSimulation += TimeManager_OnPostPhysicsSimulation;
        }

        public override void OnStartClient()
        {
            // Events only needed by the client.
            PredictionManager.OnPostReplicateReplay += PredictionManager_OnPostReplicateReplay;
            PredictionManager.OnPostReconcileSyncTransforms += PredictionManager_OnPreReconcile;
        }

        public override void OnStopClient()
        {
            // Events only needed by the client.
            PredictionManager.OnPostReplicateReplay -= PredictionManager_OnPostReplicateReplay;
            PredictionManager.OnPostReconcileSyncTransforms -= PredictionManager_OnPreReconcile;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnPostPhysicsSimulation -= TimeManager_OnPostPhysicsSimulation;
        }

        /// <summary>
        /// Called by the PredictionManager immediately before a reconcile begins.
        /// </summary>
        protected virtual void PredictionManager_OnPreReconcile(uint clientTick, uint serverTick)
        {
            CheckColliders(clientTick);
        }

        /// <summary>
        /// When using TimeManager for physics timing, this is called immediately after the physics simulation has occured for the tick.
        /// While using Unity for physics timing, this is called during Update, only if a physics frame.
        /// This may be useful if you wish to run physics differently for stacked scenes.
        private void TimeManager_OnPostPhysicsSimulation(float delta)
        {
            CheckColliders(TimeManager.LocalTick);
        }

        /// <summary>
        /// Called after physics is simulated when replaying a replicate method.
        /// </summary>
        private void PredictionManager_OnPostReplicateReplay(uint clientTick, uint serverTick)
        {
            CheckColliders(clientTick);
        }

        /// <summary>
        /// Returns if colliders should be checked. If colliders can be checked data needed by all collider checks (2D and 3D) is set.
        /// </summary>
        /// <returns>True if collision checking should proceed, false if not.</returns>
        protected bool TryPrepareColliderCheck(uint tick)
        {
            // Should not be possible as tick always starts on 1.
            if (tick == TimeManagerCls.UNSET_TICK)
                return false;

            /* Previous may not be set here if there were
             * no collisions during the previous tick. */

            // If layers are specified then do not use GOs layers, use specified.
            if (Layers != (LayerMask)0)
            {
                InteractableLayers = Layers;
            }
            // Use GOs layers.
            else
            {
                int currentLayer = gameObject.layer;
                if (_lastGameObjectLayer != currentLayer)
                {
                    _lastGameObjectLayer = currentLayer;
                    InteractableLayers = GameKit.Dependencies.Utilities.Layers.GetInteractableLayersValue(currentLayer);
                }
            }

            return true;
        }

        /// <summary>
        /// Implement collider checking logic within this method.
        /// </summary>
        protected abstract void CheckColliders(uint clientTick);

        /// <summary>
        /// Clears stored collider states.
        /// </summary>
        /// <param name = "invokeOnExit">True to invoke OnExit if a collider is stored in the OnEntered state. When called during a reconcile this used the current ClientReplayTick, otherwise uses LocalTick.</param>
        protected abstract void ClearColliderDataHistory(bool invokeOnExit);

        /// <summary>
        /// Finds colliders on this object to check.
        /// </summary>
        /// <param name = "force">True to set colliders again even if already found. This action will clear stored collider states.</param>
        /// <returns>True if colliders should be found again.</returns>
        public virtual bool TryFindColliders(bool force = false) => !_collidersFound || force;
    }
}