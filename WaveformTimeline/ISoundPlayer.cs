using System.ComponentModel;

namespace WaveformTimeline
{
	/// <summary>
	/// Denotes a type used for playing sound.
	/// </summary>
	public interface ISoundPlayer : INotifyPropertyChanged
	{
		/// <summary>
		/// Is the player currently playing.
		/// </summary>
		bool IsPlaying { get; set; }
	}
}