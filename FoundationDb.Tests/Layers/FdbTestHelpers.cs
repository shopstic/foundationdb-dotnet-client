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
	* Neither the name of the <organization> nor the
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

using FoundationDb.Client;
using FoundationDb.Client.Tuples;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace FoundationDb.Client.Tests
{

	internal static class FdbTestHelpers
	{
		public static async Task DumpSubspace(FdbDatabase db, FdbSubspace subspace)
		{
			using (var tr = db.BeginTransaction())
			{
				Console.WriteLine("Dumping content of subspace " + subspace.ToString() + " :");
				int count = 0;
				await tr.GetRangeStartsWith(subspace.Tuple).ExecuteAllAsync(
					(key, value) =>
					{
						++count;
						string keyDump = null;
						try
						{
							// attemps decoding it as a tuple
							keyDump = key.ToTuple().ToString();
						}
						catch (Exception)
						{
							// not a tuple, dump as bytes
							keyDump = "'" + key.ToString() + "'";
						}
						
						Console.WriteLine("- " + keyDump + " = " + value.ToString());

					}
				);
				if (count == 0)
					Console.WriteLine("> empty !");
				else
					Console.WriteLine("> Found " + count + " values");
			}
		}

		public static async Task DeleteSubspace(FdbDatabase db, FdbSubspace subspace)
		{
			using (var tr = db.BeginTransaction())
			{
				tr.ClearRange(subspace.Tuple);
				await tr.CommitAsync();
			}
		}

	}

}