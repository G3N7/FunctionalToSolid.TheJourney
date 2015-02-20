using System.Collections.Generic;

namespace FunctionalToSolid.TheJourney
{
	internal class DragonEntity
	{
		public string Name { get; set; }
		public int RealmId { get; set; }

		public static IEnumerable<DragonEntity> Collection = new[]
		{
			new DragonEntity {Name = "Balthazar", RealmId = TheFuncyJourney.MainRealmId},
			new DragonEntity {Name = "Niv-Mizzet", RealmId = TheFuncyJourney.MainRealmId},
			new DragonEntity {Name = "Ben", RealmId = 9999},
		};
	}
}