﻿using FishNet.Managing;
using FishNet.Documenting;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FishNet.Example.CustomSyncObject
{
    /// <summary>
    /// This is the data type I want to create a custom SyncType for.
    /// </summary>
    public struct Structy
    {
        public string Name;
        public ushort Age;

        public Structy(string name, ushort age)
        {
            Name = name;
            Age = age;
        }
    }

    /// <summary>
    /// It's very important to exclude this from codegen.
    /// However, whichever value you are synchronizing must not be excluded. This is why the value is outside the StructySync class.
    /// </summary>
    public class StructySync : SyncBase, ICustomSync
    {
        #region Types.
        /// <summary>
        /// Information about how the struct has changed.
        /// You could send the entire struct on every change
        /// but this is an example of how you might send individual changed
        /// fields.
        /// </summary>
        private struct ChangeData
        {
            internal CustomOperation Operation;
            internal Structy Data;

            public ChangeData(CustomOperation operation, Structy data)
            {
                Operation = operation;
                Data = data;
            }
        }

        /// <summary>
        /// Types of changes. This is related to ChangedData
        /// where you can specify what has changed.
        /// </summary>
        public enum CustomOperation : byte
        {
            Full = 0,
            Name = 1,
            Age = 2
        }
        #endregion

        #region Public.
        /// <summary>
        /// Delegate signature for when Structy changes.
        /// </summary>
        /// <param name = "op"></param>
        /// <param name = "oldItem"></param>
        /// <param name = "newItem"></param>
        public delegate void CustomChanged(CustomOperation op, Structy oldItem, Structy newItem, bool asServer);

        /// <summary>
        /// Called when the Structy changes.
        /// </summary>
        public event CustomChanged OnChange;
        /// <summary>
        /// Current value of Structy.
        /// </summary>
        public Structy Value = new();
        #endregion

        #region Private.
        /// <summary>
        /// Initial value when initialized. Used to reset this sync type.
        /// </summary>
        private Structy _initialValue;
        /// <summary>
        /// Changed data which will be sent next tick.
        /// </summary>
        private readonly List<ChangeData> _changed = new();
        /// <summary>
        /// True if values have changed since initialization.
        /// </summary>
        private bool _valuesChanged;
        /// <summary>
        /// Last value after dirty call.
        /// </summary>
        private Structy _lastDirtied = new();
        #endregion

        protected override void Initialized()
        {
            base.Initialized();
            _initialValue = Value;
        }

        /// <summary>
        /// Adds an operation and invokes locally.
        /// </summary>
        /// <param name = "operation"></param>
        /// <param name = "index"></param>
        /// <param name = "prev"></param>
        /// <param name = "next"></param>
        private void AddOperation(CustomOperation operation, Structy prev, Structy next)
        {
            if (!IsInitialized)
                return;

            if (NetworkManager != null && !NetworkBehaviour.IsServerStarted)
            {
                NetworkManager.LogWarning($"Cannot complete operation as server when server is not active.");
                return;
            }

            /* Set as changed even if cannot dirty.
             * Dirty is only set when there are observers,
             * but even if there are not observers
             * values must be marked as changed so when
             * there are observers, new values are sent. */
            _valuesChanged = true;
            Dirty();

            // Data can currently only be set from server, so this is always asServer.
            bool asServer = true;
            // Add to changed.
            ChangeData cd = new(operation, next);
            _changed.Add(cd);
            OnChange?.Invoke(operation, prev, next, asServer);
        }

        /// <summary>
        /// Writes all changed values.
        /// </summary>
        /// <param name = "writer"></param>
        /// <param name = "resetSyncTick">True to set the next time data may sync.</param>
        protected internal override void WriteDelta(PooledWriter writer, bool resetSyncTick = true)
        {
            base.WriteDelta(writer, resetSyncTick);
            writer.WriteInt32(_changed.Count);

            for (int i = 0; i < _changed.Count; i++)
            {
                ChangeData change = _changed[i];
                writer.WriteUInt8Unpacked((byte)change.Operation);

                // Clear does not need to write anymore data so it is not included in checks.
                if (change.Operation == CustomOperation.Age)
                {
                    writer.WriteUInt16(change.Data.Age);
                }
                else if (change.Operation == CustomOperation.Name)
                {
                    writer.WriteString(change.Data.Name);
                }
            }

            _changed.Clear();
        }

        /// <summary>
        /// Writes all values if not initial values.
        /// </summary>
        /// <param name = "writer"></param>
        protected internal override void WriteFull(PooledWriter writer)
        {
            if (!_valuesChanged)
                return;

            base.WriteHeader(writer, false);
            // Write one change.
            writer.WriteInt32(1);
            // Write if changed is from the server, so always use the server _value.           
            writer.WriteUInt8Unpacked((byte)CustomOperation.Full);
            // Write value.
            writer.Write(Value);
        }

        /// <summary>
        /// Reads and sets the current values for server or client.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [APIExclude]
        protected internal override void Read(PooledReader reader, bool asServer)
        {
            SetReadArguments(reader, asServer, out bool newChangeId, out bool _, out bool canModifyValues);

            int changes = reader.ReadInt32();
            for (int i = 0; i < changes; i++)
            {
                CustomOperation operation = (CustomOperation)reader.ReadUInt8Unpacked();
                Structy prev = Value;
                Structy next = prev;

                // Full.
                if (operation == CustomOperation.Full)
                    next = reader.Read<Structy>();
                // Name.
                else if (operation == CustomOperation.Name)
                    next.Name = reader.ReadStringAllocated();
                // Age
                else if (operation == CustomOperation.Age)
                    next.Age = reader.ReadUInt16();

                if (canModifyValues)
                    Value = next;

                if (newChangeId)
                    OnChange?.Invoke(operation, prev, next, asServer);
            }
        }

        /// <summary>
        /// Checks Value for changes and sends them to clients.
        /// </summary>
        public void ValuesChanged()
        {
            Structy prev = _lastDirtied;
            Structy current = Value;

            if (prev.Name != current.Name)
                AddOperation(CustomOperation.Name, prev, current);
            if (prev.Age != current.Age)
                AddOperation(CustomOperation.Age, prev, current);

            _lastDirtied = Value;
        }

        /// <summary>
        /// Resets to initialized values.
        /// </summary>
        protected internal override void ResetState(bool asServer)
        {
            base.ResetState(asServer);
            _changed.Clear();
            Value = _initialValue;
            _valuesChanged = false;
        }

        /// <summary>
        /// Return the serialized type.
        /// </summary>
        /// <returns></returns>
        public object GetSerializedType() => typeof(Structy);
    }
}