using System.Collections.Generic;
using System.Linq;

namespace FunctionalToSolid.TheJourney
{
	public class SqlFindDragonService : IFindDragonService
	{
		public IEnumerable<Dragon> FindByRealm(int realmId)
		{
			return Transform(GetDragonsFromSql(realmId)).Concat(GetEvergreenDragons());
		}

		private static IEnumerable<Dragon> Transform(IEnumerable<DragonEntity> getDragonsFromSql)
		{
			return getDragonsFromSql.Select(Transform);
		}

		private static Dragon Transform(DragonEntity dragonEntity)
		{
			return new Dragon(dragonEntity.Name);
		}

		private static IEnumerable<DragonEntity> GetDragonsFromSql(int realmId)
		{
			return DragonEntity.Collection.Where(x => x.RealmId == realmId);
		}

		private static IEnumerable<Dragon> GetEvergreenDragons()
		{
			return new[] { new Dragon("Bahamut") };
		}
	}
}