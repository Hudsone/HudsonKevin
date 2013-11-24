using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class HKSeaTurtle : HKSimpleOrderBase {
		public HKSeaTurtle(object _ctx):base(_ctx){}
		
		public enum TimeInterval {
			Minute,
			Day
		}
		
		[Input]
		public int BreakoutSystem { get; set; }
		[Input]
		public int DayDataIdx { get; set; }
		[Input]
		public TimeInterval DataTimeInterval { get; set; }
		
		private const int maxUnit= 4;
		private double[] scaleFactor= new double[2]{ 6* 60, 1};
		
		private List< double> tradePosition;
		private Stack< int> tradeContracts;
		private DateTime dayMark;
		
		private double N;
		private double valueVar;
		private int unit;
		
		protected override void Create() {
			// create variable objects, function objects, order objects etc.
			CreateOrder("SeaTurtle");
			tradePosition= new List< double>();
			tradeContracts= new Stack< int>();
			BreakoutSystem= 1;
			DayDataIdx= 1;
			DataTimeInterval= HKSeaTurtle.TimeInterval.Day;
		}
		protected override void StartCalc() {
			// assign inputs 
		}
		protected override void CalcBar(){
			// strategy logic 
			if( dayMark== null || dayMark.Day!= Bars.Time[0].Day) {
				N= this.AverageTrueRange( 20, 0, DayDataIdx);
				valueVar= N* ( 5* 4);
				unit= ( int) (( InitialCapital* 0.01)/ ( valueVar* scaleFactor[ ( int) DataTimeInterval]));
				dayMark= Bars.Time[0];
				Output.WriteLine("N= {0}, valueVar= {1}, unit= {2}, dayMark= {3}", N, valueVar, unit, dayMark.Day);
			}
			
			if( BreakoutSystem== 1) {
			} else {
				if( tradeContracts.Count!= 0) {
					// Output.WriteLine("3. {0}, {1}", tradeContracts.Count, tradePosition.Count);
					if( tradeContracts.Count!= tradePosition.Count) {
						AdjustStopInfo();
					} else {
						StrategyAfterEngaged();
					}
				} else {
					OpenTrade();
				}
			}
		}
		
		private void AdjustStopInfo() {
			Output.WriteLine("4. {0}, {1}", CurrentPosition.OpenTrades.Count, tradeContracts.Count);
			if( CurrentPosition.OpenTrades.Count== tradeContracts.Count) {
				for( int i= 0; i< tradePosition.Count; i++)
					tradePosition[i]+= ( LongSide? N/ 2: -N/ 2);
				tradePosition.Insert( 0, CurrentPosition.OpenTrades[0].EntryOrder.Price+ (LongSide? -2* N: 2* N));
			}
		}
		
		private void StrategyAfterEngaged() {
			if( LongSide) {
				if( Bars.Low[0]< BarsOfData( DayDataIdx).Low.Lowest( 10, 1)) {
					CloseAll();
				} else if( Bars.High[0]>= CurrentPosition.OpenTrades[0].EntryOrder.Price+ N/ 2) {
					if( tradePosition.Count< maxUnit && unit!= 0) {
						Order( unit);
						tradeContracts.Push( unit);
					}
				} else {
					int closedTrade= 0;
					for( int i= 0; i< tradePosition.Count; i++) {
						if( Bars.Low[0]<= tradePosition[i]) {
							Close( tradeContracts.Pop());
							closedTrade++;
						}
					}
					tradePosition.RemoveRange( 0, closedTrade);
				}
			} else {
				Output.WriteLine( "2. Bars.High[0]: {0}, Highest: {1}", Bars.High[0], BarsOfData( DayDataIdx).High.Highest( 10, 1));
				if( Bars.High[0]> BarsOfData( DayDataIdx).High.Highest( 10, 1)) {
					CloseAll();
				} else if( Bars.Low[0]<= CurrentPosition.OpenTrades[0].EntryOrder.Price- N/ 2) {
					if( tradePosition.Count< maxUnit && unit!= 0) {
						Order( unit);
						tradeContracts.Push( unit);
					}
				} else {
					int closedTrade= 0;
					for( int i= 0; i< tradePosition.Count; i++) {
						if( Bars.High[0]>= tradePosition[i]) {
							Close( tradeContracts.Pop());
							closedTrade++;
						}
					}
					tradePosition.RemoveRange( 0, closedTrade);
				}
			}
		}
		
		private void OpenTrade() {
			if( unit== 0)
				return;
			
			// Output.WriteLine( "1. Bars.High[0]: {0}, Highest: {1}",
			// 				  Bars.High[0], BarsOfData( DayDataIdx).High.Highest( 55));
			if( Bars.High[0]>= BarsOfData( DayDataIdx).High.Highest( 55)) {
				LongSide= true;
				Order( unit);
				tradeContracts.Push( unit);
			} else if( Bars.Low[0]<= BarsOfData( DayDataIdx).Low.Lowest( 55)) {
				LongSide= false;
				Order( unit);
				tradeContracts.Push( unit);
			}
		}
		
		private void CloseAll() {
			foreach( int contracts in tradeContracts) {
				Close( contracts);
			}
			tradePosition.Clear();
			tradeContracts.Clear();
		}
	}
}