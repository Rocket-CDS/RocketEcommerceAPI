using RocketEcommerceAPI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Components;
using DNNrocketAPI;
using Simplisity;

namespace RocketEcommerceAPI.Interfaces
{
	public abstract class ShippingInterface
	{


		#region "Shared/Static Methods"

		// constructor
		static ShippingInterface()
		{
		}
		public static ShippingInterface Instance(string assembly, string namespaceclass)
		{
			var objectToInstantiate = namespaceclass + ", " + assembly;
			var objectType = Type.GetType(objectToInstantiate);
			return (ShippingInterface)Activator.CreateInstance(objectType);
		}

		#endregion

		public abstract int CalculateShippingCost(CartLimpet  cartData);
		public abstract int CalculateShippingCost(OrderLimpet orderData);
		public abstract bool Active();
		public abstract string SelectText();
		public abstract string Msg();
		public abstract string ShipProvKey();

	}
}
