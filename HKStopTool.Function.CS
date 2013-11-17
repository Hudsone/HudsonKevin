using System;
using System.Drawing;
using System.Linq;


namespace PowerLanguage
{
	namespace Function
	{
		public sealed class HKStopTool
		{
			public interface IStopCriteria {
				double MaxWin { get; }
				double MaxLose { get; }
				TimeSpan StopTimeToday { get; }
			}
			
			public delegate void DStopOrder();
			
			private IStopCriteria stopCriteria;
			private DStopOrder stopOrder;
			
			public HKStopTool( IStopCriteria stopCriteria, DStopOrder stopOrder) {
				this.stopCriteria= stopCriteria;
				this.stopOrder= stopOrder;
			}
			
			public void TestToStop( IInstrument bars,
									IMarketPosition position,
									DateTime sessionBeginTime) {
				if( position.OpenTrades.Count!= 0) {
					if( stopCriteria.MaxLose!= 0 && position.OpenTrades[0].Profit<= -stopCriteria.MaxLose ||
						stopCriteria.MaxWin!= 0 && position.OpenTrades[0].Profit>= stopCriteria.MaxWin)
						stopOrder();
					else if( stopCriteria.StopTimeToday.TotalMinutes!= 0 &&
							 bars.Time[0].Subtract( sessionBeginTime).TotalMinutes>= stopCriteria.StopTimeToday.TotalMinutes)
						stopOrder();
				}
			}
		}
	}
}