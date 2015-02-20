using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FunctionalToSolid.TheJourney
{
	public class TheFuncyJourney
	{
		//Lets do a thought experiment, if we constrain our programming to functions, data records,
		//	and not relying on any shared state (in memory).
		
		//We would use function composition, described as structuring functions passed to other functions
		//	that will yield a final value when executed, to accomplish behaviors in our application.

		//Lets establish a baseline.
		//  if f(x) => y and g(y) => z then g(f(x)) => z

		private static int f(int x)
		{
			return x + 1;
		}

		private static int g(int y)
		{
			return y * 2;
		}

		[Test]
		public void ComposeGWithF()
		{
			//when composed
			var result = g(f(1));
			Assert.That(result, Is.EqualTo(4));

			//lets break that down...
			var fResult = f(1);
			Assert.That(fResult, Is.EqualTo(2));

			//note that we are passing the result in
			var gResult = g(fResult);
			//however the operates exactly the same as when we composed the functions at compile time.
			Assert.That(gResult, Is.EqualTo(4));
		}









		//now lets alias this composed behavior into h, we will say h(x) => z
		
		private static int h(int x)
		{
			return g(f(x));
		}

		[Test]
		public void AliasedInvocation()
		{
			const int x = 1;

			var resultC = g(f(x));

			var resultA = h(x);

			Assert.That(resultA, Is.EqualTo(resultC));
		}

























		// Ok, so we just established that if we want a 'z' and we have a 'x' 
		//	we can get there with some function composition.
		// So lets now think about how we could get to a 'z' that is a list of dragons,
		//	from an 'x' we will say is a realm id.

		// I almost always start with h, lets call it FindDragonsByRealm(realmId) => IEnumerable<Dragon>
		private static IEnumerable<Dragon> FindDragonsByRealm(int realmId)
		{
			// but under the hood we find the complexities of HOW we find dragons.
			return Transform(GetDragonsFromSql(realmId)).Concat(GetEvergreenDragons());
			// lets take a moment to think about how readable makes our logic.
		}

		#region Minutia

		private static IEnumerable<Dragon> GetEvergreenDragons()
		{
			return new[] { new Dragon("Bahamut") };
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

		#endregion

		public const int MainRealmId = 1;
		[Test]
		public void FindDragonsByRealm()
		{
			var results = FindDragonsByRealm(MainRealmId).ToArray();
			Assert.That(results, Has.Length.EqualTo(3));
		}
		
















		// lets add one more layer of complexity for the sake of demonstration.
		private static readonly TimeSpan RecentThreshold = TimeSpan.FromDays(30);
		private static IEnumerable<Dragon> FindRecentlySeenDragons(int realmId)
		{
			return FilterByLastSeen(
				FindDragonsByRealm(realmId),
				GetDragonSightingsFromMongo(realmId),
				RecentThreshold);
		}

		private static IEnumerable<Dragon> FilterByLastSeen(
			IEnumerable<Dragon> allDragons,
			IEnumerable<DragonSighting> sightings,
			TimeSpan recentThreshold)
		{
			var releventSightings = sightings.Where(ds => ds.SeenOn > DateTime.Now.Subtract(recentThreshold));
			var dragonsNamesSeenRecently = releventSightings.Select(ds => ds.Name).Distinct();
			return allDragons.Where(d => dragonsNamesSeenRecently.Contains(d.Name));
		}

		private static IEnumerable<DragonSighting> GetDragonSightingsFromMongo(int realmId)
		{
			return DragonSighting.Collection.Where(x => x.RealmId == realmId);
		}

		[Test]
		public void FindRecentlySeenDragons()
		{
			var results = FindRecentlySeenDragons(MainRealmId).ToArray();
			Assert.That(results, Has.Length.EqualTo(1));

			// ok so at this point I hope your thinking to yourself, wow that would be a bear to test,
			//   we would need to setup the data in the database to be exactly what we expect for each test.
			//   To make matters worse I would need to do it in both SQL and Mongo.  "Ouch" -E.T.
		}















		// One strategy we could take would be to pass our functions in as params,
		//	then our signature would look like:
		private static IEnumerable<Dragon> FindRecentlySeenDragons(
			Func<int, IEnumerable<Dragon>> findDragonsByRealm,
			Func<int, IEnumerable<DragonSighting>> getDragonSightings,
			int realmId)
		{
			return FilterByLastSeen(findDragonsByRealm(realmId), getDragonSightings(realmId), RecentThreshold);
		}

		[Test]
		public void FindRecentlySeenDragonsFunc()
		{
			var results = FindRecentlySeenDragons(
				FindDragonsByRealm,
				GetDragonSightingsFromMongo,
				MainRealmId).ToArray();
			Assert.That(results, Has.Length.EqualTo(1));

			// Now we have ended up with two categories of inputs,
			//	those related to the 'how' and those realted to the 'when given'.
		}





































		//  o          `O    Oo    `o    O  o.OOoOoo       O       o OooOOo. 
		//  O           o   o  O    o   O    O             o       O O     `O
		//  o           O  O    o   O  O     o             O       o o      O
		//  O           O oOooOoOo  oOo      ooOO          o       o O     .o
		//  o     o     o o      O  o  o     O             o       O oOooOO' 
		//  O     O     O O      o  O   O    o             O       O o       
		//  `o   O o   O' o      O  o    o   O             `o     Oo O       
		//   `OoO' `OoO'  O.     O  O     O ooOooOoO        `OoooO'O o'

		// Now lets try to solve this same problem of testabiltiy with the SOLID Principles and some C# language features common to most OO languages.

		public class RecentlySeenDragonFunc : IRecentlySeenDragonService
		{
			private readonly Func<int, IEnumerable<Dragon>> _findDragonService;
			private readonly Func<int, IEnumerable<DragonSighting>> _getDragonSightings;
			private readonly TimeSpan _recentThreshold;

			public RecentlySeenDragonFunc(
				Func<int, IEnumerable<Dragon>> findDragonsByRealm,
				Func<int, IEnumerable<DragonSighting>> getDragonSightings,
				TimeSpan recentThreshold)
			{
				//here we are able to isolate the params related to the 'how' in the ctor letting our logic that calls
				//	the method, to concentrate on variable data passed in.
				_findDragonService = findDragonsByRealm;
				_getDragonSightings = getDragonSightings;
				_recentThreshold = recentThreshold;
			}

			public IEnumerable<Dragon> FindByRealmId(int realmId)
			{
				//note that our functional approach lends itself to composition,
				//	we will collect our data and pass it into our filtration logic
				return FilterByLastSeen(_findDragonService(realmId), _getDragonSightings(realmId), _recentThreshold);
			}

			private static IEnumerable<Dragon> FilterByLastSeen(
				IEnumerable<Dragon> allDragons,
				IEnumerable<DragonSighting> sightings,
				TimeSpan recentThreshold)
			{
				var releventSightings = sightings.Where(ds => ds.SeenOn > DateTime.Now.Subtract(recentThreshold));
				var dragonsNamesSeenRecently = releventSightings.Select(ds => ds.Name).Distinct();
				return allDragons.Where(d => dragonsNamesSeenRecently.Contains(d.Name));
			}
		}

		[Test]
		public void ObjectWithFunComposition()
		{

			var realRecentlySeenDragonService = new RecentlySeenDragonFunc(FindDragonsByRealm, GetDragonSightingsFromMongo, RecentThreshold);

			var results = realRecentlySeenDragonService.FindByRealmId(MainRealmId).ToArray();
			Assert.That(results, Has.Length.EqualTo(1));
		}
		
















		public class RecentlySeenDragonService : IRecentlySeenDragonService
		{
			private readonly IFindDragonService _findDragonService;
			private readonly IDragonSightingService _dragonSightingService;
			private readonly TimeSpan _recentThreshold;

			public RecentlySeenDragonService(
				IFindDragonService findDragonService,
				IDragonSightingService dragonSightingService,
				TimeSpan recentThreshold)
			{
				_findDragonService = findDragonService;
				_dragonSightingService = dragonSightingService;
				_recentThreshold = recentThreshold;
			}

			public IEnumerable<Dragon> FindByRealmId(int realmId)
			{
				return FilterByLastSeen(
					_findDragonService.FindByRealm(realmId),
					_dragonSightingService.GetByRealm(realmId),
					_recentThreshold);
			}

			private static IEnumerable<Dragon> FilterByLastSeen(
				IEnumerable<Dragon> allDragons,
				IEnumerable<DragonSighting> sightings,
				TimeSpan recentThreshold)
			{
				var releventSightings = sightings.Where(ds => ds.SeenOn > DateTime.Now.Subtract(recentThreshold));
				var dragonsNamesSeenRecently = releventSightings.Select(ds => ds.Name).Distinct();
				return allDragons.Where(d => dragonsNamesSeenRecently.Contains(d.Name));
			}
		}

		[Test]
		public void SolidComposition()
		{
			IDragonSightingService realDragonSightingService = new MongoDragonSightingService();
			IFindDragonService realFindDragonService = new SqlFindDragonService();
			var realRecentlySeenDragonService = new RecentlySeenDragonService(realFindDragonService, realDragonSightingService, RecentThreshold);
			
			var results = realRecentlySeenDragonService.FindByRealmId(MainRealmId).ToArray();
			Assert.That(results, Has.Length.EqualTo(1));
		}

		// DIP - Dependency Inversion Principle - This allows us to separate out the 'how' arguments into the constructor 
		//		giving us a functional principle called partial application.
		// ISP - Interface Segregation Principle - Keeps us from coupling our composition of 'how' things get done.
		// SRP - Single Responsibility Principle - forces us to breakup the how of our complex problem into composible steps, and allows flexablity
		//		to refactor pieces at a time without having to replace entire systems at once i.e. persistance, transport tech, endpoints.
		// OCP - Open/Closed Principle - and LSP - Liskov Substitution Principle - provide us safety, allowing us to naively compose the who via contracts.
	}


}
