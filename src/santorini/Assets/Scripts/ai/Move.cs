namespace etf.santorini.sv150155d.ai
{
	public struct Move
	{
		public (char row, int col) FromPosition { get; set; }
		public (char row, int col) ToPosition { get; set; }
		public (char row, int col) BuildOn { get; set; }

		public Move((char row, int col) fromPosition, (char row, int col) toPosition, (char row, int col) buildOn)
		{
			FromPosition = fromPosition;
			ToPosition = toPosition;
			BuildOn = buildOn;
		}
	}
}
