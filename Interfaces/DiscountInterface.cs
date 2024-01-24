using RocketEcommerceAPI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Components;
using DNNrocketAPI;
using Simplisity;

namespace RocketEcommerceAPI.Interfaces
{
	public abstract class DiscountInterface
    {


		#region "Shared/Static Methods"

		// constructor
		static DiscountInterface()
		{
		}
		public static DiscountInterface Instance(string assembly, string namespaceclass)
		{
			var objectToInstantiate = namespaceclass + ", " + assembly;
			var objectType = Type.GetType(objectToInstantiate);
			return (DiscountInterface)Activator.CreateInstance(objectType);
		}

        #endregion
        public abstract int CalculateDiscountCost(CartLimpet  cartData);
		public abstract bool Active();

    }
}
