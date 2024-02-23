using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engine.Facet
{
	public class AddStaticCommand : BaseCommand
	{
		public AddStaticCommand()
		{
			AccessLevel = AccessLevel.GameMaster;		
			Supports = CommandSupport.Simple; //Supports = CommandSupport.AllNPCs | CommandSupport.AllItems;
			Commands = new string[] { "addStatic" };
			ObjectTypes = ObjectTypes.All;
			Usage = "addStatic itemId Hue [altitude]";
			Description = "Add a static.";
		}

		public override bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
		{
			bool retVal = true;

			if (e.Length < 1 || e.Length > 3)
			{
				e.Mobile.SendMessage("You must specify the Item ID and, optionally a Hue and a Z value");
				retVal = false;
			}

			return retVal;
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			int newHue = 0;
			int newID = e.GetInt32(0);

			if (e.Length >= 2)
			{
				newHue = e.GetInt32(1);
			}

			if (obj is IPoint3D)
			{
				IPoint3D location = e.Mobile.Location;

				if (obj is IPoint3D)
				{
					location = (IPoint3D)obj;
				}

				int newZ = location.Z;

				if (e.Length == 3)
				{
					newZ = e.GetInt32(2);
				}

				new AddStatic(e.Mobile.Map.MapID, (ushort)newID, (sbyte)newZ, location.X, location.Y, (ushort)newHue).DoOperation();
			}
		}
	}
}