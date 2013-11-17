using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class HKSimpleOrderBase : SignalObject {
		public HKSimpleOrderBase(object _ctx):base(_ctx){}
		
		private IOrderMarket buyOrder;
		private IOrderMarket buyClose;
		private IOrderMarket sellOrder;
		private IOrderMarket sellClose;
		private IOrderMarket order;
		private IOrderMarket close;
		private bool longSide;
		
		public bool LongSide {
			get { return longSide; }
			set {
				longSide= value;
				if( longSide) {
					order= buyOrder;
					close= buyClose;
				} else {
					order= sellOrder;
					close= sellClose;
				}
			}
		}
		
		protected void CreateOrder(string prefix) {
			buyOrder= OrderCreator.MarketNextBar( new SOrderParameters( Contracts.Default, prefix+ "Buy", EOrderAction.Buy));
			buyClose= OrderCreator.MarketNextBar( new SOrderParameters( Contracts.Default, prefix+ "Buy", EOrderAction.Sell));
			sellOrder= OrderCreator.MarketNextBar( new SOrderParameters( Contracts.Default, prefix+ "Sell", EOrderAction.SellShort));
			sellClose= OrderCreator.MarketNextBar( new SOrderParameters( Contracts.Default, prefix+ "Sell", EOrderAction.BuyToCover));
		}
		
		public void Order() {
			order.Send();
		}
		
		public void Close() {
			close.Send();
		}
		
		protected override void Create() {
		}
		protected override void StartCalc() {
		}
		protected override void CalcBar(){
		}
	}
}