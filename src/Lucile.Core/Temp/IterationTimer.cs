namespace Codeworx.Core
{
	using System;
	using System.Linq;
	using System.Reactive.Linq;
	using System.Collections.Generic;
	using System.Reactive.Subjects;
	using System.Reactive.Disposables;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Timer To Iterate over a list of <see cref="IIterationItem"/>s
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IterationTimer<T> : IConnectableObservable<T> where T : IIterationItem
	{
		private int currentPosition;

		private DateTime startDate;

		private TimeSpan totalDuration;

		private T[] items;

		private CancellationTokenSource cancellationTokenSource;

		private ManualResetEvent signal;

		private ManualResetEvent stopSignal;

		private ManualResetEvent pauseSignal;

		private DateTime iterationStartDate;

		private DateTime lastPauseTime;

		private int iterations;

		private Subject<T> underlying = new Subject<T>();

		/// <summary>
		/// Initializes a new instance of the <see cref="IterationTimer&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="items">The items to iterate over.</param>
		public IterationTimer(IEnumerable<T> items)
		{
			currentPosition = -1;
			stopSignal = new ManualResetEvent(false);
			pauseSignal = new ManualResetEvent(true);

			if (items == null)
				throw new ArgumentNullException("items");

			if (items == null)
				throw new ArgumentException("Sequence contains no items", "items");

			this.items = items.OrderBy(p => p.Offset).ToArray();

			this.totalDuration = this.items.Select(p => p.Offset + p.Duration).Max();
		}

		public T CurrentItem
		{
			get
			{
				if (currentPosition == -1)
					return default(T);

				return items[currentPosition];
			}
		}

		/// <summary>
		/// Gets the total duration of one iteration.
		/// </summary>
		public TimeSpan IterationDuration
		{
			get
			{
				return totalDuration;
			}
		}

		public bool IsPaused { get; private set; }

		/// <summary>
		/// Processes this iteration.
		/// </summary>
		/// <param name="calculateOffset">Indicates weather the timer was started with a given offset or not. Value must be of type bool.</param>
		private void Process(object calculateOffset)
		{
			if (!(calculateOffset is bool))
				throw new ArgumentOutOfRangeException("calculateOffset", "Value must be of type Boolean.");

			this.iterations = 0;
			this.iterationStartDate = startDate;

			if ((bool)calculateOffset)
			{
				var now = DateTime.Now;
				iterations = Convert.ToInt32(Math.Floor((double)((now - startDate).Ticks / this.IterationDuration.Ticks)));
				iterationStartDate = startDate + TimeSpan.FromTicks(iterations * this.IterationDuration.Ticks);
				this.currentPosition = this.items.Select((p, i) => new { Item = p, Index = i })
											.Where(p => p.Item.Offset <= now - iterationStartDate)
											.OrderByDescending(p => p.Index)
											.Select(p => p.Index)
											.First() - 1;
			}

			while (true)
			{
				if (cancellationTokenSource.Token.IsCancellationRequested)
				{
					var oldunderlying = this.underlying;
					this.underlying = new Subject<T>();
					oldunderlying.OnCompleted();
					return;
				}

				pauseSignal.WaitOne(TimeSpan.FromMinutes(1));

				if (!IsPaused)
				{
					int nextPosition = this.currentPosition < this.items.Length - 1 ? this.currentPosition + 1 : 0;
					T nextItem = this.items[nextPosition];
					DateTime nextStart = iterationStartDate + nextItem.Offset;

					if (nextStart <= DateTime.Now)
					{
						var result = this.items.Select((p, i) => new { Item = p, Index = i })
							.Where(p => p.Index >= nextPosition && iterationStartDate + p.Item.Offset <= DateTime.Now)
							.Select(p => p.Index)
							.ToArray();

						foreach (var item in result)
						{
							try
							{
								underlying.OnNext(this.items[item]);
							}
							catch (Exception ex)
							{
								var oldunderlying = underlying;
								underlying = new Subject<T>();
								oldunderlying.OnError(ex);
								oldunderlying.Dispose();
							}
						}
						this.currentPosition = result.Max();

						nextPosition = this.currentPosition < this.items.Length - 1 ? this.currentPosition + 1 : 0;
						nextItem = this.items[nextPosition];
						if (nextPosition == 0)
						{
							iterations++;
							iterationStartDate = this.startDate + TimeSpan.FromTicks(this.totalDuration.Ticks * iterations);
						}
						nextStart = iterationStartDate + nextItem.Offset;
					}

					int waitTime = Math.Max(Convert.ToInt32(((nextStart) - DateTime.Now).TotalMilliseconds), 0);
					if (!cancellationTokenSource.Token.IsCancellationRequested)
						signal.WaitOne(waitTime);
				}
			}
		}

		/// <summary>
		/// Connects this instance.
		/// </summary>
		/// <returns><see cref="Disposable.Empty"/></returns>
		public IDisposable Connect()
		{
			return Connect(TimeSpan.Zero);
		}

		/// <summary>
		/// Connects this instance.
		/// </summary>
		/// <param name="offset">The offset.</param>
		/// <returns>
		///   <see cref="Disposable.Empty"/>
		/// </returns>
		public IDisposable Connect(TimeSpan offset)
		{
			if (signal != null)
			{
				throw new InvalidOperationException("IterationTimer is already running");
			}

			currentPosition = -1;
			startDate = DateTime.Now - offset;
			signal = new ManualResetEvent(false);

			cancellationTokenSource = new CancellationTokenSource();

			Task.Factory.StartNew(Process, offset != TimeSpan.Zero, cancellationTokenSource.Token)
				.ContinueWith(p =>
				{
					this.currentPosition = -1;
					this.signal = null;
					this.stopSignal.Set();
				});

			return Disposable.Empty;
		}

		/// <summary>
		/// Stops this iteration.
		/// </summary>
		public void Stop()
		{
			if (this.signal == null)
				throw new InvalidOperationException("No Iteration is started");

			this.cancellationTokenSource.Cancel();
			this.pauseSignal.Set();
			this.signal.Set();
			this.stopSignal.WaitOne(TimeSpan.FromMinutes(1));
			this.stopSignal.Reset();
			if (this.signal != null)
				throw new TimeoutException("IterationTimer Stop operation took too long.");
		}

		/// <summary>
		/// Pauses this iteration.
		/// </summary>
		public void Pause() {
			this.IsPaused = true;
			this.lastPauseTime = DateTime.Now;
			this.pauseSignal.Reset();
		}

		/// <summary>
		/// Pauses this iteration.
		/// </summary>
		public void Resume()
		{
			var pauseOffset = DateTime.Now - lastPauseTime;
			this.startDate = this.startDate + pauseOffset;
			this.iterationStartDate = this.iterationStartDate + pauseOffset;
			this.IsPaused = false;
			this.pauseSignal.Set();
		}

		/// <summary>
		/// Subscribes the specified observer.
		/// </summary>
		/// <param name="observer">The observer.</param>
		/// <returns></returns>
		public IDisposable Subscribe(IObserver<T> observer)
		{
			return underlying.Subscribe(observer);
		}
	}
}
