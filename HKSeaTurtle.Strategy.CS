using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class HKSeaTurtle : HKSimpleOrderBase {
		public HKSeaTurtle(object _ctx):base(_ctx){}
		
		[Input]
		public int BreakoutSystem { get; set; }
		
		private const int maxUnit= 4;
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
		}
		protected override void StartCalc() {
			// assign inputs 
		}
		protected override void CalcBar(){
			// strategy logic 
			if( dayMark== null || dayMark.Day!= Bars.Time[0].Day) {
				N= this.AverageTrueRange( 20, 0, 2);
				valueVar= N* ( 5* 4);
				unit= (int) (( InitialCapital* 0.01)/ valueVar);
				dayMark= Bars.Time[0];
			}
			
			if( BreakoutSystem== 1) {
			} else {
				if( tradePosition.Count< maxUnit) {
					if( tradeContracts.Count!= 0) {
						if( tradeContracts.Count!= tradePosition.Count) {
							if( CurrentPosition.OpenTrades.Count== tradeContracts.Count) {
								for( int i= 0; i< tradePosition.Count; i++)
									tradePosition[i]+= ( LongSide? N/ 2: -N/ 2);
								tradePosition.Insert( 0, CurrentPosition.OpenTrades[0].EntryOrder.Price+ (LongSide? -2* N: 2* N));
							}
						} else {
							if( LongSide) {
								if( Bars.Low[0]< _LowestBar.LowestBar( BarsOfData( 2).Low, 10)) {
									CloseAll();
								} else if( Bars.High[0]>= CurrentPosition.OpenTrades[0].EntryOrder.Price+ N/ 2) {
									Order( unit);
									tradeContracts.Push( unit);
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
								if( Bars.High[0]> _HighestBar.HighestBar( BarsOfData( 2).High, 10)) {
									CloseAll();
								} else if( Bars.Low[0]<= CurrentPosition.OpenTrades[0].EntryOrder.Price- N/ 2) {
									Order( unit);
									tradeContracts.Push( unit);
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
					} else {
						if( Bars.High[0]> _HighestBar.HighestBar( BarsOfData( 2).High, 55)) {
							SetUnit( unit);
							LongSide= true;
							Order();
							tradeContracts.Push( unit);
						} else if( Bars.Low[0]< _LowestBar.LowestBar( BarsOfData( 2).Low, 55)) {
							SetUnit( unit);
							LongSide= false;
							Order();
							tradeContracts.Push( unit);
						}
					}
				}
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