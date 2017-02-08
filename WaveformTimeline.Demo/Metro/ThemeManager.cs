using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace WaveformTimeline.Demo.Metro
{
	[Export(typeof(IThemeManager))]
	public class ThemeManager : IThemeManager
	{
		private readonly ResourceDictionary themeResources;

		public ThemeManager()
		{
			themeResources = new ResourceDictionary
			{
				Source = new Uri("pack://application:,,,/WaveformTimeline.Demo;component/Metro/ThemeBase.xaml")
			};
		}

		public ResourceDictionary GetThemeResources()
		{
			return this.themeResources;
		}
	}
}