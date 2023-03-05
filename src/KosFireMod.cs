using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;

namespace kosfire
{
    public class KosFireMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ItemPhosphorusMatch", typeof(ItemPhosphorusMatch));
            api.RegisterItemClass("ItemDynamiteStick", typeof(ItemDynamiteStick));

            api.RegisterEntity("EntityThrownDynamite", typeof(EntityThrownDynamite));


            /*if (api is ICoreServerAPI sapi)
            {
                sapi.World.Logger.StoryEvent("kosFireMod loaded");
            }*/

            //Check for Existing Config file, create one if none exists
            try
            {
                var Config = api.LoadModConfig<KosFireConfig>("kosfireandexplosions.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    KosFireConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    KosFireConfig.Current = KosFireConfig.GetDefault();
                }
            }
            catch
            {
                KosFireConfig.Current = KosFireConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                api.StoreModConfig(KosFireConfig.Current, "kosfireandexplosions.json");
            }
        }
    }
}
