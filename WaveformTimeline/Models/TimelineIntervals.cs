namespace WaveformTimeline.Models
{
	/// <summary>
	/// Class that holds the time intervals for the major and minor ticks on the timeline.
	/// </summary>
	public class TimelineIntervals
	{
		public TimelineIntervals(double major, double minor)
		{
			Major = major;
			Minor = minor;
		}

		/// <summary>
		/// The major tick interval.
		/// </summary>
		public double Major { get; set; }

		/// <summary>
		/// The minor tick interval.
		/// </summary>
		public double Minor { get; set; }
	}
}