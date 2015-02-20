namespace FunctionalToSolid.TheJourney
{
	public class Dragon
	{
		public Dragon(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}