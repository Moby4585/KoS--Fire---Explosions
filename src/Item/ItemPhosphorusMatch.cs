﻿using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;


namespace kosfire
{
    class ItemPhosphorusMatch : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;

            if (blockSel == null) return;
            Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);

            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return;
            }

            if (!(block is IIgnitable ign) || ign.OnTryIgniteBlock(byEntity, blockSel.Position, 0) == EnumIgniteState.NotIgnitable)
            {
                return;
            }

            handling = EnumHandHandling.PreventDefault;
        }


        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null)
            {
                return false;
            }

            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }


            Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);


            EnumIgniteState igniteState = EnumIgniteState.NotIgnitable;
            if (block is IIgnitable ign) igniteState = ign.OnTryIgniteBlock(byEntity, blockSel.Position, (secondsUsed > 0.1f ? secondsUsed * 20f : 0f));

            if (igniteState == EnumIgniteState.NotIgnitable || igniteState == EnumIgniteState.NotIgnitablePreventDefault)
            {
                return false;
            }

            return igniteState == EnumIgniteState.Ignitable;
        }


        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null) return;

            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player ?? null;

            if (api.Side == EnumAppSide.Client)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("kosfire", "sounds/player/phosphormatch"), byEntity, byPlayer, true, 16);
                return;
            }

            Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);

            EnumIgniteState igniteState = EnumIgniteState.NotIgnitable;
            var ign = block as IIgnitable;
            if (ign != null) igniteState = ign.OnTryIgniteBlock(byEntity, blockSel.Position, (secondsUsed > 0.1f ? secondsUsed * 20f : 0f));

            if (igniteState != EnumIgniteState.IgniteNow)
            {
                return;
            }
            //byEntity.World.PlaySoundAt(new AssetLocation("kosfire", "sounds/player/phosphormatch"), byEntity, byPlayer, true, 16);
            slot.TakeOut(1);
            slot.MarkDirty();

            if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return;
            }


            EnumHandling handled = EnumHandling.PassThrough;
            ign.OnTryIgniteBlockOver(byEntity, blockSel.Position, (secondsUsed > 0.1f ? secondsUsed * 50f : 0f), ref handled);
        }


        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            return true;
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    HotKeyCode = "shift",
                    ActionLangCode = "heldhelp-igniteblock",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }

    }
}
