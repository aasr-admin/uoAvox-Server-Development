using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

using System;

namespace Server.Items
{
    public class LumberjackCamp : BaseAddon
    {
        [Constructable]
        public LumberjackCamp()
        {
            /// Log Stack
            AddComponent(new AddonComponent(0x1BE2), -4, 0, 0);

            /// Campfire
            AddComponent(new AddonComponent(0xDE3), 0, 1, 0);

            /// Tree Stump With Axe
            AddComponent(new AddonComponent(0x0E58), -4, 1, 0);

            /// Large Log
            AddComponent(new AddonComponent(0x0CF5), -1, 3, 0);
            AddComponent(new AddonComponent(0x0CF6), 0, 3, 0);
            AddComponent(new AddonComponent(0x0CF7), 1, 3, 0);
        }

        public LumberjackCamp(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}