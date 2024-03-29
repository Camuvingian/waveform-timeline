﻿using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace WaveformTimeline.Demo.Services
{
	[Export(typeof(IServiceLocator))]
	public class MefServiceLocator : IServiceLocator
	{
		private readonly CompositionContainer compositionContainer;

		[ImportingConstructor]
		public MefServiceLocator(CompositionContainer compositionContainer)
		{
			this.compositionContainer = compositionContainer;
		}

		public T GetInstance<T>() where T : class
		{
			var instance = compositionContainer.GetExportedValue<T>();
			if (instance != null)
				return instance;

			throw new Exception(
				String.Format("Could not locate any instances of contract {0}.", typeof(T)));
		}
	}
}