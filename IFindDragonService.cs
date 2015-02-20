using System.Collections.Generic;

namespace FunctionalToSolid.TheJourney
{
	public interface IFindDragonService
	{
		IEnumerable<Dragon> FindByRealm(int realmId);
	}
}