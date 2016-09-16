using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Core
{
	public interface IIterationItem
	{
		TimeSpan Offset { get; }
		TimeSpan Duration { get; }
	}
}
