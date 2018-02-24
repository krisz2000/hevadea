﻿using System;
using System.Collections.Generic;

namespace Maker.Rise.Utils.FiniteStateMachine
{
    // <summary>
    /// A Finite State Machine. 
    /// T is a type which will be used as descriptors of the state. Usually this is an enum, string or an integral type,
    /// but any type can be used.
    /// </summary>
    /// <typeparam name="T">A type which will be used as descriptors of the state. Usually this is an enum, string or an integral type,
    /// but any type can be used.</typeparam>
    public class FSM<T>
    {
        Dictionary<T, FsmStateBehaviour<T>> m_StateBehaviours = new Dictionary<T, FsmStateBehaviour<T>>();
        T currentState = default(T);
        FsmStateBehaviour<T> m_CurrentStateBehaviour;
        float StateAge = -1f;
        public string Name { get; private set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSM{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the FSM, used in throw exception and for debug purposes.</param>
        public FSM(string name)
        {
            Name = name;
        }


        /// <summary>
        /// Gets or sets a callback which will be called when the FSM logs state transitions. Used to track state transition for debug purposes.
        /// </summary>
        /// <value>
        /// The debug log handler.
        /// </value>
        public Action<string> DebugLogHandler { get; set; }


        /// <summary>
        /// Adds the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The newly created behaviour, so that it could be configured with a fluent-like syntax.</returns>
        public FsmStateBehaviour<T> Add(T state)
        {
            var behaviour = new FsmStateBehaviour<T>(state);
            m_StateBehaviours.Add(state, behaviour);
            return behaviour;
        }

        /// <summary>
        /// Gets the number of states currently in the FSM.
        /// </summary>
        /// <value>
        /// The number of states currently in the FSM.
        /// </value>
        public int Count
        {
            get { return m_StateBehaviours.Count; }
        }

        /// <summary>
        /// Processes the logic for the FSM. 
        /// </summary>
        /// <param name="time">The time, expressed in seconds.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Process(float time)
        {
            if (StateAge < 0f)
                StateAge = time;

            float totalTime = time;
            float stateTime = (totalTime - StateAge);
            float stateProgress = 0f;

            if (m_CurrentStateBehaviour == null)
            {
                throw new InvalidOperationException(string.Format("[FSM {0}] : Can't call 'Process' before setting the starting state.", Name));
            }

            if (m_CurrentStateBehaviour.Duration.HasValue)
            {
                stateProgress = Math.Max(0f, Math.Min(1f, stateTime / m_CurrentStateBehaviour.Duration.Value));
            }

            var data = new FsmStateData<T>()
            {
                Machine = this,
                Behaviour = m_CurrentStateBehaviour,
                State = currentState,
                StateTime = stateTime,
                AbsoluteTime = totalTime,
                StateProgress = stateProgress
            };

            m_CurrentStateBehaviour.Trigger(data);

            if (stateProgress >= 1f && m_CurrentStateBehaviour.NextStateSelector != null)
            {
                CurrentState = m_CurrentStateBehaviour.NextStateSelector();
                StateAge = time;
            }
        }

        /// <summary>
        /// Gets or sets the current state of the FSM.
        /// </summary>
        public T CurrentState
        {
            get { return currentState; }
            set
            {
                if (m_CurrentStateBehaviour != null)
                    m_CurrentStateBehaviour.TriggerLeave();

                StateAge = -1f;

                m_CurrentStateBehaviour = m_StateBehaviours[value];
                currentState = value;

                if (m_CurrentStateBehaviour != null)
                    m_CurrentStateBehaviour.TriggerEnter();
            }
        }

        /// <summary>
        /// Moves the FSM to the next state as configured using FsmStateBehaviour.GoesTo(...).
        /// Note: to change the state freely, use the CurrentState property.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the behaviour has not a next state / state selector configured.</exception>
        public void Next()
        {
            if (m_CurrentStateBehaviour.NextStateSelector != null)
                this.CurrentState = m_CurrentStateBehaviour.NextStateSelector();
            else
                throw new InvalidOperationException(string.Format("[FSM {0}] : Can't call 'Next' on current behaviour.", Name));
        }

        /// <summary>
        /// Saves a snapshot of the FSM
        /// </summary>
        /// <returns>The snapshot.</returns>
        public FsmSnapshot<T> SaveSnapshot()
        {
            return new FsmSnapshot<T>(StateAge, currentState);
        }

        /// <summary>
        /// Restores a snapshot of the FSM taken with SaveSnapshot
        /// </summary>
        /// <param name="snap">The snapshot.</param>
        public void RestoreSnapshot(FsmSnapshot<T> snap)
        {
            CurrentState = snap.CurrentState;
            StateAge = snap.StateAge;
        }
    }
}
