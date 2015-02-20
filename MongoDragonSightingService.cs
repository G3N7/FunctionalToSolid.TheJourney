using System.Collections.Generic;
using System.Linq;

namespace FunctionalToSolid.TheJourney
{
	public class MongoDragonSightingService : IDragonSightingService
	{
		public IEnumerable<DragonSighting> GetByRealm(int realmId)
		{
			return DragonSighting.Collection.Where(x => x.RealmId == realmId);
		}
	}
}