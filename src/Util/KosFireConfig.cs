using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kosfire
{
    class KosFireConfig
    {
        public string StickExplodeInClaim = "If true, thrown dynamite sticks can explode in claims";
        public bool CanStickExplodeInClaim;

        public string DestroyBlocksInClaim = "If false (and StickExplodeInClaim true), the dynamtie stick will damage entities but will not destroy blocks in claims";
        public bool CanDestroyBlocksInClaim;

        public string DynamiteStickThrowable = "If false, dynamite sticks will no longer be throwable";
        public bool IsDynamiteStickThrowable;

        public KosFireConfig()
        { }

        public static KosFireConfig Current { get; set; }

        public static KosFireConfig GetDefault()
        {
            KosFireConfig defaultConfig = new KosFireConfig();

            defaultConfig.StickExplodeInClaim.ToString();
            defaultConfig.CanStickExplodeInClaim = true;
            defaultConfig.DestroyBlocksInClaim.ToString();
            defaultConfig.CanDestroyBlocksInClaim = false;
            defaultConfig.DynamiteStickThrowable.ToString();
            defaultConfig.IsDynamiteStickThrowable = true;
            return defaultConfig;
        }
    }
}
