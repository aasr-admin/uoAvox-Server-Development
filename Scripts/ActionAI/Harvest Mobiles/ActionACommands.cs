using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

using System;

namespace Server.Commands
{
    /// For ActionAI: Laborers
    public class Plot_Command
    {
        public static void Initialize()
        {
            CommandSystem.Register("Plot", AccessLevel.GameMaster, Plot_OnCommand);
        }

        [Usage("Plot")]
        public static void Plot_OnCommand(CommandEventArgs e)
        {
            Console.WriteLine("(new Tuple<Point3D, Direction> (new Point3D{0}, Direction.{1})),", e.Mobile.Location, e.Mobile.Direction);
        }
    }


    /// For ActionAI: Caravans
    public class Mark_Command
    {
        public static void Initialize()
        {
            CommandSystem.Register("Mark", AccessLevel.GameMaster, Mark_OnCommand);
        }

        [Usage("Mark")]
        public static void Mark_OnCommand(CommandEventArgs e)
        {
            Console.WriteLine("(new Point3D{0}),", e.Mobile.Location);
        }
    }
}