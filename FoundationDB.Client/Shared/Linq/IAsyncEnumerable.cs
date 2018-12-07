#region BSD License
/* Copyright (c) 2013-2018, Doxense SAS
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of Doxense nor the
	  names of its contributors may be used to endorse or promote products
	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

#if !USE_SHARED_FRAMEWORK

#if !NATIVE_ASYNC

 namespace System.Collections.Generic
{
	using System;
	using JetBrains.Annotations;

	/// <summary>Asynchronous version of the <see cref="System.Collections.Generic.IEnumerable{T}"/> interface, allowing elements of the enumerable sequence to be retrieved asynchronously.</summary>
	/// <typeparam name="T">Element type.</typeparam>
	public interface IAsyncEnumerable<out T>
	{
		/// <summary>Gets an asynchronous enumerator over the sequence.</summary>
		/// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
		[NotNull]
		IAsyncEnumerator<T> GetAsyncEnumerator();
	}

}

#else

namespace System.Threading.Tasks
{
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks.Sources;

	internal struct ManualResetValueTaskSourceLogic<TResult>
	{
		private ManualResetValueTaskSourceCore<TResult> _core;
		public ManualResetValueTaskSourceLogic(IStrongBox<ManualResetValueTaskSourceLogic<TResult>> parent) : this() { }
		public short Version => _core.Version;
		public TResult GetResult(short token) => _core.GetResult(token);
		public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);
		public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);
		public void Reset() => _core.Reset();
		public void SetResult(TResult result) => _core.SetResult(result);
		public void SetException(Exception error) => _core.SetException(error);
	}
}

namespace System.Runtime.CompilerServices
{
	internal interface IStrongBox<T> { ref T Value { get; } }
}

#endif

namespace Doxense.Linq
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using JetBrains.Annotations;

	// note: these interfaces are modeled after the IAsyncEnumerable<T> and IAsyncEnumerator<T> found in Rx
	//TODO: if/when async enumerables are avail in C#, we would just need to either remove these interfaces, or make them implement the real stuff

	/// <summary>Asynchronous version of the <see cref="System.Collections.Generic.IEnumerable{T}"/> interface, allowing elements of the enumerable sequence to be retrieved asynchronously.</summary>
	public interface IConfigurableAsyncEnumerable<out T> : IAsyncEnumerable<T>
	{

		/// <summary>Gets an asynchronous enumerator over the sequence.</summary>
		/// <param name="ct">Token used to cancel the iterator from the outside</param>
		/// <param name="hint">Defines how the enumerator will be used by the caller. The source provider can use the mode to optimize how the results are produced.</param>
		/// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
		[NotNull]
		IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct, AsyncIterationHint hint);
	}

}

#endif
