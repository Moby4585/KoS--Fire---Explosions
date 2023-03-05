﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace kosfire
{
    public class EntityThrownDynamite : Entity
    {
        bool beforeCollided;
        bool stuck;

        long msLaunch;
        Vec3d motionBeforeCollide = new Vec3d();

        CollisionTester collTester = new CollisionTester();

        public Entity FiredBy;
        public float Damage;
        public ItemStack ProjectileStack;

        public int blastRadius = 4;
        public int injureRadius = 5;
        public EnumBlastType blastType = EnumBlastType.RockBlast;

        public override bool IsInteractable
        {
            get { return false; }
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            msLaunch = World.ElapsedMilliseconds;

            if (ProjectileStack?.Collectible != null)
            {
                ProjectileStack.ResolveBlockOrItem(World);
            }
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
            if (ShouldDespawn) return;

            EntityPos pos = SidedPos;

            stuck = Collided;
            pos.Pitch = GameMath.PIHALF;
            pos.Roll = 0;
            pos.Yaw = GameMath.PIHALF;

            if (stuck)
            {
                if (!beforeCollided && World is IServerWorldAccessor)
                {
                    float strength = GameMath.Clamp((float)motionBeforeCollide.Length() * 4, 0, 1);

                    if (CollidedHorizontally || CollidedVertically)
                    {
                        OnImpact();
                        return;
                    }

                    // Resend position to client
                    WatchedAttributes.MarkAllDirty();
                }

                beforeCollided = true;
                return;
            }


            if (World is IServerWorldAccessor)
            {
                Entity entity = World.GetNearestEntity(ServerPos.XYZ, 5f, 5f, (e) => {
                    if (e.EntityId == this.EntityId || (FiredBy != null && e.EntityId == FiredBy.EntityId && World.ElapsedMilliseconds - msLaunch < 500) || !e.IsInteractable)
                    {
                        return false;
                    }

                    double dist = e.SelectionBox.ToDouble().Translate(e.ServerPos.X, e.ServerPos.Y, e.ServerPos.Z).ShortestDistanceFrom(ServerPos.X, ServerPos.Y, ServerPos.Z);
                    return dist < 0.5f;
                });

                if (entity != null)
                {
                    //entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Entity, SourceEntity = this, Type = EnumDamageType.BluntAttack }, Damage);
                    OnImpact();
                    return;
                }
            }

            beforeCollided = false;
            motionBeforeCollide.Set(pos.Motion.X, pos.Motion.Y, pos.Motion.Z);

        }


        public void OnImpact()
        {
            bool isInClaim = (Api.World.Claims?.Get(this.SidedPos.AsBlockPos)?.Length ?? 1) > 0;

            if (!KosFireConfig.Current.CanStickExplodeInClaim && isInClaim)
            {
                World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), this, null, false, 32);
                World.SpawnCubeParticles(SidedPos.XYZ.OffsetCopy(0, 0.2, 0), ProjectileStack, 0.8f, 20);
                Die();

                return;
            }

            World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), this, null, false, 32);
            World.SpawnCubeParticles(SidedPos.XYZ.OffsetCopy(0, 0.2, 0), ProjectileStack, 0.8f, 20);
            Die();

            if (isInClaim && !KosFireConfig.Current.CanDestroyBlocksInClaim)
            {
                ((IServerWorldAccessor)Api.World).CreateExplosion(this.SidedPos.AsBlockPos, EnumBlastType.EntityBlast, blastRadius, injureRadius);
                return;
            }

            ((IServerWorldAccessor)Api.World).CreateExplosion(this.SidedPos.AsBlockPos, blastType, blastRadius, injureRadius);

            /*EntityProperties type = World.GetEntityType(new AssetLocation("beemob"));
            Entity entity = World.ClassRegistry.CreateEntity(type);

            if (entity != null)
            {
                entity.ServerPos.X = SidedPos.X + 0.5f;
                entity.ServerPos.Y = SidedPos.Y + 0.5f;
                entity.ServerPos.Z = SidedPos.Z + 0.5f;
                entity.ServerPos.Yaw = (float)World.Rand.NextDouble() * 2 * GameMath.PI;
                entity.Pos.SetFrom(entity.ServerPos);

                entity.Attributes.SetString("origin", "brokenbeenade");
                World.SpawnEntity(entity);
            }*/
        }


        public override bool CanCollect(Entity byEntity)
        {
            return false;
        }

        public override void OnCollideWithLiquid()
        {
            base.OnCollideWithLiquid();
        }

        public override void ToBytes(BinaryWriter writer, bool forClient)
        {
            base.ToBytes(writer, forClient);
            writer.Write(beforeCollided);
            ProjectileStack.ToBytes(writer);
        }

        public override void FromBytes(BinaryReader reader, bool fromServer)
        {
            base.FromBytes(reader, fromServer);
            beforeCollided = reader.ReadBoolean();

            ProjectileStack = World == null ? new ItemStack(reader) : new ItemStack(reader, World);
        }
    }
}