using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Legion.Entity.View.Effect
{
	public partial class SmokeView : UserControl
	{
		public SmokeView()
		{
			InitializeComponent();
		}

		public void RefreshView(double opacity)
		{
			if (opacity <= 0)
			{
				this.Opacity = 0;
				this.Visibility = System.Windows.Visibility.Collapsed;
			}
			else
			{
				this.Opacity = opacity;
			}
		}
	}
}
