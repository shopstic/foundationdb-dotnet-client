﻿#region BSD Licence
/* Copyright (c) 2013, Doxense SARL
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

namespace FoundationDB.Linq.Expressions
{
	using FoundationDB.Client;
	using FoundationDB.Layers.Indexing;
	using FoundationDB.Layers.Tuples;
	using FoundationDB.Linq.Utils;
	using System;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>Helper class to construct Query Expressions</summary>
	public static class FdbQueryExpressions
	{

#if REFACTORED
		public static FdbQueryIndexExpression<TId, TValue> Index<TId, TValue>(FdbIndex<TId, TValue> index)
		{
			if (index == null) throw new ArgumentNullException("index");

			return new FdbQueryIndexExpression<TId, TValue>(index);
		}
#endif

		public static FdbQueryConstantExpression<T> Constant<T>(T value)
		{
			return new FdbQueryConstantExpression<T>(value);
		}

		public static FdbQuerySingleExpression<T, R> Single<T, R>(FdbQuerySequenceExpression<T> source, string name, Expression<Func<IFdbAsyncEnumerable<T>, CancellationToken, Task<R>>> lambda)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (lambda == null) throw new ArgumentNullException("lambda");

			if (name == null) name = lambda.Name ?? "Lambda";

			return new FdbQuerySingleExpression<T, R>(source, name, lambda);
		}

		public static FdbQueryAsyncEnumerableExpression<T> Sequence<T>(IFdbAsyncEnumerable<T> source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return new FdbQueryAsyncEnumerableExpression<T>(source);
		}

		public static FdbQueryRangeExpression Range(FdbKeySelectorPair range, FdbRangeOptions options = null)
		{
			return new FdbQueryRangeExpression(range, options);
		}

		public static FdbQueryRangeExpression Range(FdbKeySelector start, FdbKeySelector stop, FdbRangeOptions options = null)
		{
			return Range(new FdbKeySelectorPair(start, stop), options);
		}

		public static FdbQueryRangeExpression RangeStartsWith(Slice prefix, FdbRangeOptions options = null)
		{
			return Range(FdbKeySelectorPair.StartsWith(prefix), options);
		}

		public static FdbQueryRangeExpression RangeStartsWith(IFdbTuple tuple, FdbRangeOptions options = null)
		{
			return Range(tuple.ToSelectorPair(), options);
		}

		public static FdbQueryIndexLookupExpression<TId, TValue> Lookup<TId, TValue>(FdbIndex<TId, TValue> index, ExpressionType op, Expression value)
		{
			if (index == null) throw new ArgumentNullException("index");
			if (value == null) throw new ArgumentNullException("value");

			switch(op)
			{
				case ExpressionType.Equal:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
					break;
				default:
					throw new ArgumentException("Index lookups only support the following operators: '==', '!=', '>', '>=', '<' and '<='", "op");
			}

			if (value.Type != typeof(TValue)) throw new ArgumentException("Value must have a type compatible with the index", "value");

			return new FdbQueryIndexLookupExpression<TId, TValue>(index, op, value);
		}

		public static FdbQueryIndexLookupExpression<TId, TValue> Lookup<TId, TValue>(FdbIndex<TId, TValue> index, Expression<Func<TValue, bool>> expression)
		{
			if (index == null) throw new ArgumentNullException("index");

			var binary = expression.Body as BinaryExpression;
			if (binary == null) throw new ArgumentException("Only binary expression are allowed", "expression");

			var constant = binary.Right as ConstantExpression;
			if (constant == null || constant.Type != typeof(TValue)) throw new ArgumentException(String.Format("Left side of expression '{0}' must be a constant of type {1}", binary.Right.ToString(), typeof(TValue).Name));

			return new FdbQueryIndexLookupExpression<TId, TValue>(index, binary.NodeType, constant);
		}

		public static FdbQueryIntersectExpression<K, T> Intersect<K, T>(Expression<Func<T, K>> keySelector, params FdbQuerySequenceExpression<T>[] expressions)
		{
			if (expressions == null) throw new ArgumentNullException("expressions");
			if (expressions.Length <= 1) throw new ArgumentException("There must be at least two sequences to perform an intersection");

			var type = expressions[0].Type;
			//TODO: check that all the other have the same type

			return new FdbQueryIntersectExpression<K, T>(expressions, keySelector, null);
		}

		public static FdbQueryTransformExpression<T, R> Transform<T, R>(FdbQuerySequenceExpression<T> source, Expression<Func<T, R>> transform)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (transform == null) throw new ArgumentNullException("transform");

			if (source.ElementType != typeof(T)) throw new ArgumentException(String.Format("Source sequence has type {0} that is not compatible with transform input type {1}", source.ElementType.Name, typeof(T).Name), "source");

			return new FdbQueryTransformExpression<T, R>(source, transform);
		}

		public static FdbQueryFilterExpression<T> Filter<T>(FdbQuerySequenceExpression<T> source, Expression<Func<T, bool>> filter)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (filter == null) throw new ArgumentNullException("filter");

			if (source.ElementType != typeof(T)) throw new ArgumentException(String.Format("Source sequence has type {0} that is not compatible with filter input type {1}", source.ElementType.Name, typeof(T).Name), "source");

			return new FdbQueryFilterExpression<T>(source, filter);
		}

	}

}
