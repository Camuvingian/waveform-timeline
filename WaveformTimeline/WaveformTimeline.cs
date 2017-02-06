using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WaveformTimeline
{
	/// <summary>
	/// Control that displays a stereo waveform and allows the users to select/highlight a timespan. 
	/// The control also allows regions to be marked indicating parts of the timeline to be 
	/// considered by the user. 
	/// </summary>
	[DisplayName("Waveform Timeline")]
	[Description("Controls which displays a sterio waveform and alows the user to select a timespan.")]
	[ToolboxItem(true)]
	[TemplatePart(Name = "PART_Waveform", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Timeline", Type = typeof(Canvas))] 
	[TemplatePart(Name =	"PART_Selection", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Regions", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Progress", Type = typeof(Canvas))]
	public class WaveformTimeline : Control 
	{
		private IWaveformPlayer soundPlayer;

		private Canvas waveformCanvas;
		private Canvas timelineCanvas;
		private Canvas selectionCanvas;
		private Canvas regionsCanvas;
		private Canvas progressCanvas;

		private readonly Path leftPath = new Path();
		private readonly Path rightPath = new Path();
		private readonly Path progressIndicator = new Path();

		private readonly Line centerLine = new Line();
		private readonly Line proressLine = new Line();

		private readonly Rectangle selectionRegion = new Rectangle();
		private readonly Rectangle timelineBackgroundeion = new Rectangle();
		private readonly List<Rectangle> highlightRegions = new List<Rectangle>();



	}
}
