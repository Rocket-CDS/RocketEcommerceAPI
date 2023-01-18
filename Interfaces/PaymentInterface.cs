using RocketEcommerceAPI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Components;
using DNNrocketAPI;
using Simplisity;

namespace RocketEcommerceAPI.Interfaces
{
	public abstract class PaymentInterface
	{

		#region "Shared/Static Methods"

		// constructor
		static PaymentInterface()
		{
		}

		// return the provider
		public static PaymentInterface Instance(string assembly, string namespaceclass)
		{
			var objectToInstantiate = namespaceclass + ", " + assembly;
			var objectType = Type.GetType(objectToInstantiate);
			return (PaymentInterface)Activator.CreateInstance(objectType);
		}

		#endregion

		public abstract string GetBankRemotePost(PaymentLimpet paymentLimpet);
		public abstract void ReturnEvent(SimplisityInfo paramInfo);
		public abstract string NotifyEvent(SimplisityInfo paramInfo);
		public abstract bool Active();
		public abstract string PayButtonText();
		public abstract string PayMsg();
		public abstract string PaymentProvKey();

	}
}
