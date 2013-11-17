using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class HKRandomAndStop : HKSimpleOrderBase, HKStopTool.IStopCriteria {
		public HKRandomAndStop( object _ctx): base( _ctx){}
		
		[Input]
		public double MaxLose { get; set; }
		[Input]
		public double MaxWin { get; set; }
		[Input]
		public bool barStatus { get; set; }
		[Input]
		public bool longPosition { get; set; }
		[Input]
		public bool reverse { get; set; }
		
		public TimeSpan StopTimeToday {
			get { return new TimeSpan( 5, 30, 0); }
		}
		
		private int secondToDetect;
		private bool hasBought;
		private Random rand;
		private DateTime currentDate;
		private HKStopTool stopTool;
		
		protected override void Create() {
			// create variable objects, function objects, order objects etc.
			CreateOrder("RandSt");
			rand= new Random();
			stopTool= new HKStopTool( this, this.Close);
		}
		protected override void StartCalc() {
			// assign inputs
			secondToDetect= rand.Next( 0, 7920);
			hasBought= false;
			LongSide= longPosition;
		}
		protected override void CalcBar() {
			// strategy logic
			if( ( currentDate== null || currentDate.Day!= Bars.Time[0].Day) &&
			 	Bars.Time[0].TimeOfDay.TotalHours>= 8.75) {
				currentDate= Bars.Time[0];
				StartCalc();
			}
			
			if( !hasBought && Bars.Time[0].TimeOfDay.TotalHours>= 8.75) {
				if( Bars.Time[0].Subtract( currentDate).TotalSeconds>= secondToDetect) {
					if( barStatus) {
						if( longPosition) {
							if( Bars.Open[0]< Bars.Close[0]!= reverse)
								Order();
						} else {
							if( Bars.Open[0]> Bars.Close[0]!= reverse)
								Order();
						}
					} else {
						if( rand.Next( 16384)< 8192)
							Order();
					}
					hasBought= true;
				}
			}
			
			stopTool.TestToStop( Bars, CurrentPosition, currentDate);
		}
	}
}
