﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Physics
{
    public class Constraint : IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (InternalConstraint == null) return;

            InternalConstraint.Dispose();
            InternalConstraint = null;

            if (RigidBodyA != null && RigidBodyA.LinkedConstraints.Contains(this))
            {
                RigidBodyA.LinkedConstraints.Remove(this);
            }

            if (RigidBodyB != null && RigidBodyB.LinkedConstraints.Contains(this))
            {
                RigidBodyB.LinkedConstraints.Remove(this);
            }
        }

        /// <summary>
        /// Gets the rigid body a.
        /// </summary>
        /// <value>
        /// The rigid body a.
        /// </value>
        public RigidBody RigidBodyA { get; internal set; }
        /// <summary>
        /// Gets the rigid body b.
        /// </summary>
        /// <value>
        /// The rigid body b.
        /// </value>
        public RigidBody RigidBodyB { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Constraint"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled
        {
            get { return InternalConstraint.IsEnabled; }
            set { InternalConstraint.IsEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the breaking impulse threshold.
        /// </summary>
        /// <value>
        /// The breaking impulse threshold.
        /// </value>
        public float BreakingImpulseThreshold
        {
            get { return InternalConstraint.BreakingImpulseThreshold; }
            set { InternalConstraint.BreakingImpulseThreshold = value; }
        }

        bool feedbackEnabled;

        /// <summary>
        /// Gets the applied impulse.
        /// </summary>
        /// <value>
        /// The applied impulse.
        /// </value>
        public float AppliedImpulse
        {
            get
            {
                if (feedbackEnabled) return InternalConstraint.AppliedImpulse;
                InternalConstraint.EnableFeedback(true);
                feedbackEnabled = true;
                return InternalConstraint.AppliedImpulse;
            }
        }

        internal PhysicsEngine Engine;

        internal BulletSharp.TypedConstraint InternalConstraint;
    }
}
