using System;
using System.Collections.Generic;

namespace FunctionalToSolid.TheJourney
{
	public class DragonSighting
	{
		public DateTime SeenOn { get; set; }
		public string Name { get; set; }
		public int RealmId { get; set; }

		public static IEnumerable<DragonSighting> Collection = new[]
		{
			new DragonSighting { Name = "Niv-Mizzet", SeenOn = DateTime.Now.AddHours(-1), RealmId = TheFuncyJourney.MainRealmId},
		};
	}
}