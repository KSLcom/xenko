﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialPositionSeed")]
    public class InitialPositionSeed : InitializerBase
    {
        public InitialPositionSeed()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var leftCorner = PositionMin * WorldScale;
            var xAxis = new Vector3(PositionMax.X * WorldScale - leftCorner.X, 0, 0);
            var yAxis = new Vector3(0, PositionMax.Y * WorldScale - leftCorner.Y, 0);
            var zAxis = new Vector3(0, 0, PositionMax.Z * WorldScale - leftCorner.Z);

            if (!WorldRotation.IsIdentity)
            {
                WorldRotation.Rotate(ref leftCorner);
                WorldRotation.Rotate(ref xAxis);
                WorldRotation.Rotate(ref yAxis);
                WorldRotation.Rotate(ref zAxis);
            }

            leftCorner += WorldPosition;


            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var particleRandPos = leftCorner;

                particleRandPos += xAxis * randSeed.GetFloat(RandomOffset.Offset3A + SeedOffset);
                particleRandPos += yAxis * randSeed.GetFloat(RandomOffset.Offset3B + SeedOffset);
                particleRandPos += zAxis * randSeed.GetFloat(RandomOffset.Offset3C + SeedOffset);

                (*((Vector3*)particle[posField])) = particleRandPos;

                i = (i + 1) % maxCapacity;
            }
        }

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        [DataMember(30)]
        [Display("Position min")]
        public Vector3 PositionMin { get; set; } = new Vector3(-1, 1, -1);

        [DataMember(40)]
        [Display("Position max")]
        public Vector3 PositionMax { get; set; } = new Vector3(1, 1, 1);

        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The rotation of the entity with regard to its parent</userdoc>
        [DataMember(12)]
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(Particles.InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(Particles.InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(Particles.InheritLocation.Scale);

            WorldScale    = (hasScl) ? Scale : 1f;

            WorldRotation = (hasRot) ? this.Rotation * Rotation : this.Rotation;

            WorldPosition = (hasPos) ? Translation : new Vector3(0, 0, 0);
        }
    }
}
