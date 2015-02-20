using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace FunctionalToSolid.TheJourney
{
	public interface IRecentlySeenDragonService
	{
		IEnumerable<Dragon> FindByRealmId(int realmId);
	}
}