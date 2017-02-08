using System;
using System.Windows;

namespace WaveformTimeline.Demo.Helpers
{
	public interface IViewLocator
	{
		UIElement GetOrCreateViewType(Type viewType);
	}
}
