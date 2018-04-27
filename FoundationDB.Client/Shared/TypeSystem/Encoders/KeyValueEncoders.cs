﻿#region BSD Licence
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

namespace Doxense.Serialization.Encoders
{
	using System;
	using System.Runtime.CompilerServices;
	using Doxense.Diagnostics.Contracts;
	using Doxense.Memory;
	using JetBrains.Annotations;

	/// <summary>Helper class for all key/value encoders</summary>
	[PublicAPI]
	public static partial class KeyValueEncoders
	{
		/// <summary>Identity function for binary slices</summary>
		public static readonly IdentityEncoder Binary = new IdentityEncoder();

		#region Nested Classes

		/// <summary>Identity encoder</summary>
		public sealed class IdentityEncoder : IKeyEncoder<Slice>, IValueEncoder<Slice>, IKeyEncoding
		{

			internal IdentityEncoder() { }

			#region IKeyEncoder...

			public IKeyEncoding Encoding => this;

			public void WriteKeyTo(ref SliceWriter writer, Slice key)
			{
				writer.WriteBytes(key);
			}

			public void ReadKeyFrom(ref SliceReader reader, out Slice value)
			{
				value = reader.ReadToEnd();
			}

			public Slice EncodeValue(Slice value)
			{
				return value;
			}

			public Slice DecodeValue(Slice encoded)
			{
				return encoded;
			}

			#endregion

			IKeyEncoder<T1> IKeyEncoding.GetEncoder<T1>()
			{
				if (typeof(T1) != typeof(Slice)) throw new NotSupportedException();
				return (IKeyEncoder<T1>) (object) this;
			}

			IDynamicKeyEncoder IKeyEncoding.GetDynamicEncoder() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2> IKeyEncoding.GetEncoder<T1, T2>() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2, T3> IKeyEncoding.GetEncoder<T1, T2, T3>() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2, T3, T4> IKeyEncoding.GetEncoder<T1, T2, T3, T4>() => throw new NotSupportedException();

		}

		/// <summary>Wrapper for encoding and decoding a singleton with lambda functions</summary>
		internal sealed class Singleton<T> : IKeyEncoder<T>, IValueEncoder<T>, IKeyEncoding
		{
			private readonly Func<T, Slice> m_encoder;
			private readonly Func<Slice, T> m_decoder;

			public Singleton(Func<T, Slice> encoder, Func<Slice, T> decoder)
			{
				Contract.Requires(encoder != null && decoder != null);

				m_encoder = encoder;
				m_decoder = decoder;
			}

			public Type[] GetTypes()
			{
				return new[] { typeof(T) };
			}

			public void WriteKeyTo(ref SliceWriter writer, T value)
			{
				writer.WriteBytes(m_encoder(value));
			}

			public void ReadKeyFrom(ref SliceReader reader, out T value)
			{
				value = m_decoder(reader.ReadToEnd());
			}

			public Slice EncodeValue(T value)
			{
				return m_encoder(value);
			}

			public T DecodeValue(Slice encoded)
			{
				return m_decoder(encoded);
			}

			public IKeyEncoding Encoding => this;

			IKeyEncoder<T1> IKeyEncoding.GetEncoder<T1>()
			{
				if (typeof(T1) != typeof(T)) throw new NotSupportedException();
				return (IKeyEncoder<T1>) (object) this;
			}

			IDynamicKeyEncoder IKeyEncoding.GetDynamicEncoder() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2> IKeyEncoding.GetEncoder<T1, T2>() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2, T3> IKeyEncoding.GetEncoder<T1, T2, T3>() => throw new NotSupportedException();

			ICompositeKeyEncoder<T1, T2, T3, T4> IKeyEncoding.GetEncoder<T1, T2, T3, T4>() => throw new NotSupportedException();
		}

		/// <summary>Wrapper for encoding and decoding a pair with lambda functions</summary>
		public abstract class CompositeKeyEncoder<T1, T2> : ICompositeKeyEncoder<T1, T2>
		{

			public abstract IKeyEncoding Encoding { get; }

			public abstract void WriteKeyPartsTo(ref SliceWriter writer, int count, ref (T1, T2) items);

			public abstract void ReadKeyPartsFrom(ref SliceReader reader, int count, out (T1, T2) items);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void WriteKeyTo(ref SliceWriter writer, (T1, T2) items)
			{
				WriteKeyPartsTo(ref writer, 2, ref items);
			}

			public void ReadKeyFrom(ref SliceReader reader, out (T1, T2) items)
			{
				ReadKeyPartsFrom(ref reader, 2, out items);
			}

		}

		/// <summary>Wrapper for encoding and decoding a triplet with lambda functions</summary>
		public abstract class CompositeKeyEncoder<T1, T2, T3> : ICompositeKeyEncoder<T1, T2, T3>
		{

			public abstract IKeyEncoding Encoding { get; }

			public abstract void WriteKeyPartsTo(ref SliceWriter writer, int count, ref (T1, T2, T3) items);

			public abstract void ReadKeyPartsFrom(ref SliceReader reader, int count, out (T1, T2, T3) items);

			public void WriteKeyTo(ref SliceWriter writer, (T1, T2, T3) items)
			{
				WriteKeyPartsTo(ref writer, 3, ref items);
			}

			public void ReadKeyFrom(ref SliceReader reader, out (T1, T2, T3) items)
			{
				ReadKeyPartsFrom(ref reader, 3, out items);
			}

		}

		/// <summary>Wrapper for encoding and decoding a quad with lambda functions</summary>
		public abstract class CompositeKeyEncoder<T1, T2, T3, T4> : ICompositeKeyEncoder<T1, T2, T3, T4>
		{

			public abstract IKeyEncoding Encoding { get; }

			public abstract void WriteKeyPartsTo(ref SliceWriter writer, int count, ref (T1, T2, T3, T4) items);

			public abstract void ReadKeyPartsFrom(ref SliceReader reader, int count, out (T1, T2, T3, T4) items);

			public void WriteKeyTo(ref SliceWriter writer, (T1, T2, T3, T4) items)
			{
				WriteKeyPartsTo(ref writer, 4, ref items);
			}

			public void ReadKeyFrom(ref SliceReader reader, out (T1, T2, T3, T4) items)
			{
				ReadKeyPartsFrom(ref reader, 4, out items);
			}

		}

		/// <summary>Wrapper for encoding and decoding five items with lambda functions</summary>
		public abstract class CompositeKeyEncoder<T1, T2, T3, T4, T5> : ICompositeKeyEncoder<T1, T2, T3, T4, T5>
		{

			public abstract IKeyEncoding Encoding { get; }

			public abstract void WriteKeyPartsTo(ref SliceWriter writer, int count, ref (T1, T2, T3, T4, T5) items);

			public abstract void ReadKeyPartsFrom(ref SliceReader reader, int count, out (T1, T2, T3, T4, T5) items);

			public void WriteKeyTo(ref SliceWriter writer, (T1, T2, T3, T4, T5) items)
			{
				WriteKeyPartsTo(ref writer, 5, ref items);
			}

			public void ReadKeyFrom(ref SliceReader reader, out (T1, T2, T3, T4, T5) items)
			{
				ReadKeyPartsFrom(ref reader, 5, out items);
			}

		}

		/// <summary>Wrapper for encoding and decoding six items with lambda functions</summary>
		public abstract class CompositeKeyEncoder<T1, T2, T3, T4, T5, T6> : ICompositeKeyEncoder<T1, T2, T3, T4, T5, T6>
		{

			public abstract IKeyEncoding Encoding { get; }

			public abstract void WriteKeyPartsTo(ref SliceWriter writer, int count, ref (T1, T2, T3, T4, T5, T6) items);

			public abstract void ReadKeyPartsFrom(ref SliceReader reader, int count, out (T1, T2, T3, T4, T5, T6) items);

			public void WriteKeyTo(ref SliceWriter writer, (T1, T2, T3, T4, T5, T6) items)
			{
				WriteKeyPartsTo(ref writer, 6, ref items);
			}

			public void ReadKeyFrom(ref SliceReader reader, out (T1, T2, T3, T4, T5, T6) items)
			{
				ReadKeyPartsFrom(ref reader, 6, out items);
			}

		}

		#endregion

		#region Keys...

		/// <summary>Binds a pair of lambda functions to a key encoder</summary>
		/// <typeparam name="T">Type of the key to encode</typeparam>
		/// <param name="encoder">Lambda function called to encode a key into a binary slice</param>
		/// <param name="decoder">Lambda function called to decode a binary slice into a key</param>
		/// <returns>Key encoder usable by any Layer that works on keys of type <typeparamref name="T"/></returns>
		[NotNull]
		public static IKeyEncoder<T> Bind<T>([NotNull] Func<T, Slice> encoder, [NotNull] Func<Slice, T> decoder)
		{
			Contract.NotNull(encoder, nameof(encoder));
			Contract.NotNull(decoder, nameof(decoder));
			return new Singleton<T>(encoder, decoder);
		}

		#endregion

	}
}
