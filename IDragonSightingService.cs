using System.Collections.Generic;

namespace FunctionalToSolid.TheJourney
{
	public interface IDragonSightingService
	{
		IEnumerable<DragonSighting> GetByRealm(int realmId);
	}
}