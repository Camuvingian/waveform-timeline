using Caliburn.Micro;
using System.ComponentModel.Composition;
using WaveformTimeLine.Demo.Services;

namespace WaveformTimeline.Demo.ViewModels
{
	/// <summary>
	/// All the logic for the main window.
	/// </summary>
	[Export(typeof(IShell))]
	public class ShellViewModel : Screen, IShell
	{
		private WaveformGenerator waveformPlayer;

		public ShellViewModel()
		{
			DisplayName = "Waveform Timeline Demo";
			InitializeData();
		}

		private void InitializeData()
		{
			WaveformPlayer = WaveformGenerator.Instance;
			WaveformPlayer.OpenFile(@"C:\Users\Administrator\Documents\Music\Avi Buffalo - What's In It For_.mp3");
		}

		public WaveformGenerator WaveformPlayer
		{
			get { return waveformPlayer; }
			set
			{
				if (waveformPlayer == value)
					return;
				waveformPlayer = value;
				NotifyOfPropertyChange(() => WaveformPlayer);
			}
		}
	}
}