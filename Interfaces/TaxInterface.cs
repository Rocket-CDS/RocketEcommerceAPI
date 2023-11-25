using RocketEcommerceAPI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Components;
using DNNrocketAPI;
using Simplisity;

namespace RocketEcommerceAPI.Interfaces
{
	public abstract class TaxInterface
    {


		#region "Shared/Static Methods"

		// constructor
		static TaxInterface()
		{
		}
		public static TaxInterface Instance(string assembly, string namespaceclass)
		{
			var objectToInstantiate = namespaceclass + ", " + assembly;
			var objectType = Type.GetType(objectToInstantiate);
			return (TaxInterface)Activator.CreateInstance(objectType);
		}

		#endregion

		public abstract int CalculateTaxCost(CartLimpet  cartData);
		public abstract int CalculateTaxCost(OrderLimpet orderData);
		public abstract bool Active();
        public abstract string TaxProvKey();

    }
}
