using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class HKMaxOfBars : IndicatorObject {
		public HKMaxOfBars(object _ctx):base(_ctx){}
		[Input]
		public bool Long { get; set; }
		[Input]
		public int NumberOfBars { get; set; }
		
		private IPlotObject plot1;
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Yellow));
			Long= true;
			NumberOfBars= 55;
		}
		protected override void StartCalc() {
			// assign inputs 
		}
		protected override void CalcBar(){
			// indicator logic 
			if( Long) {
				plot1.Set( Bars.High.Highest( NumberOfBars, 1));
			} else {
				plot1.Set( Bars.Low.Lowest( NumberOfBars, 1));
			}
		}
	}
}