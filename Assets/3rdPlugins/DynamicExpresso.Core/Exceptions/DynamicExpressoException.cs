﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
	[Serializable]
	public class DynamicExpressoException : Exception
	{
		public DynamicExpressoException() { }
		public DynamicExpressoException(string message) : base(message) { }
		public DynamicExpressoException(string message, Exception inner) : base(message, inner) { }
		protected DynamicExpressoException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
