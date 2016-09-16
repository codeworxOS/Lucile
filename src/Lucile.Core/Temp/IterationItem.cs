namespace Codeworx.Core
{
	using System;

	/// <summary>
	/// Generic implementation of IIterationItem
	/// </summary>
	/// <typeparam name="T">Type of the item value.</typeparam>
	public class IterationItem<T> : IIterationItem
	{
		public IterationItem(TimeSpan offset, TimeSpan duration, T item)
		{
			this.Offset = offset;
			this.Duration = duration;
			this.Value = item;
		}

		/// <summary>
		/// Gets the offset.
		/// </summary>
		public TimeSpan Offset
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the duration.
		/// </summary>
		public TimeSpan Duration
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		public T Value
		{
			get;
			private set;
		}
	}
}
