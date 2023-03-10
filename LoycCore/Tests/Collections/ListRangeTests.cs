using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loyc.Collections;
using Loyc.MiniTest;

namespace Loyc.Collections.Tests
{
	using System;
	
	/// <summary>Tests the IAddRange interface of a list class.</summary>
	[TestFixture]
	public class AddRangeTest<ListT> where ListT : IAddRange<int>, ICloneable<ListT>, IReadOnlyCollection<int> //, IGetIteratorSlice<int>
	{
		protected Func<ListT> _newList;

		public AddRangeTest(Func<ListT> newList)
		{
			_newList = newList;
		}

		[Test]
		public void TestAddRange()
		{
			var list = _newList();
			list.AddRange(Loyc.Range.Inclusive(1, 3));
			list.AddRange(Enumerable.Range(5, 3));
			ExpectList(list, 1, 2, 3, 5, 6, 7);
			list.AddRange(Loyc.Range.Inclusive(10, 11));
			list.AddRange(Enumerable.Range(20, 2));
			ExpectList(list, 1, 2, 3, 5, 6, 7, 10, 11, 20, 21);
			list.AddRange(Loyc.Range.Inclusive(30, 30));
			list.AddRange(Enumerable.Range(40, 1));
			ExpectList(list, 1, 2, 3, 5, 6, 7, 10, 11, 20, 21, 30, 40);
			list.AddRange(Loyc.Range.ExcludeHi(0, 0));
			list.AddRange(Enumerable.Repeat(0, 0));
			ExpectList(list, 1, 2, 3, 5, 6, 7, 10, 11, 20, 21, 30, 40);

			list.AddRange(Loyc.Range.Only(-99));
			if (list.First() == -99)
			{
				// It's a sorted list.
				list.AddRange(ListExt.Single(4));
				ExpectList(list, -99, 1, 2, 3, 4, 5, 6, 7, 10, 11, 20, 21, 30, 40);
				list.AddRange(Enumerable.Range(-2, 3));
				ExpectList(list, -99, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 10, 11, 20, 21, 30, 40);
				list.AddRange(Enumerable.Range(12, 8));
				ExpectList(list, -99, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 30, 40);
			}
			else
				ExpectList(list, 1, 2, 3, 5, 6, 7, 10, 11, 20, 21, 30, 40, -99);
		}

		protected static void ExpectList<T>(IEnumerable<T> list, params T[] expected)
		{
			ExpectList(list, (IList<T>)expected);
		}
		protected static void ExpectList<T>(IEnumerator<T> it, params T[] expected)
		{
			ExpectList(it, (IList<T>)expected);
		}
		protected static void ExpectList<T>(IEnumerable<T> list, IList<T> expected)
		{
			int i = 0;
			foreach (T item in list)
			{
				Assert.Less(i, expected.Count);
				Assert.AreEqual(expected[i], item);
				i++;
			}
			Assert.AreEqual(expected.Count, i);
		}
		protected static void ExpectList<T>(IEnumerator<T> it, IList<T> expected)
		{
			Assert.AreEqual(null, FirstIndexWhereDifferent(it, expected));
		}

		protected static int? FirstIndexWhereDifferent<T>(IEnumerable<T> it, IList<T> expected)
			=> FirstIndexWhereDifferent(it.GetEnumerator(), expected);
		protected static int? FirstIndexWhereDifferent<T>(IEnumerator<T> it, IList<T> expected)
		{
			var comp = EqualityComparer<T>.Default;
			int i;
			for (i = 0; i < expected.Count; i++) {
				if (!it.MoveNext())
					return i;
				if (!comp.Equals(expected[i], it.Current))
					return i;
			}
			if (!it.MoveNext())
				return i;
			return null;
		}

		protected static void AreEqual<T>(IEnumerator<T> it, IList<T> expected)
		{
			for (int i = 0; i < expected.Count; i++) {
				Assert.That(it.MoveNext());
				Assert.AreEqual(expected[i], it.Current);
			}
			Assert.That(!it.MoveNext());
		}
	}

	/// <summary>Tests the IAddRange and IListRangeMethod interfaces of a list class.</summary>
	[TestFixture]
	public class ListRangeTests<ListT> : AddRangeTest<ListT>
		where ListT : IListSource<int>, IListRangeMethods<int>, ICloneable<ListT>
	{
		protected int _randomSeed;
		protected Random _r;
		protected bool _testExceptions;

		public ListRangeTests(bool testExceptions, Func<ListT> newList) : this(testExceptions, newList, Environment.TickCount) { }
		public ListRangeTests(bool testExceptions, Func<ListT> newList, int randomSeed) : base(newList)
		{
			_testExceptions = testExceptions;
			_r = new Random(_randomSeed = randomSeed);
			_newList = newList;
		}

		int Count(ListT list) { return (list as IReadOnlyCollection<int>).Count; }

		[Test]
		public void TestInsertRange()
		{
			ListT list = _newList();
			List<int> list2 = new List<int>();

			int amount;
			int iteration = 0;
			for (int i = 0; i < 5000; i += amount)
			{
				Assert.AreEqual(Count(list), list2.Count);
				
				amount = _r.Next(100);
				int at = _r.Next(Count(list) + 1);
				var e = Enumerable.Range(i, amount);
				list2.InsertRange(at, e);
				if (_r.Next(5) > 0) {
					list.InsertRange(at, e);
				} else {
					var temp = new List<int>(e);
					list.InsertRange(at, temp);
				}
				if (++iteration <= 5 || (iteration % 10) == 0)
					ExpectList(list, list2);
			}
			ExpectList(list, list2);
		}

		[Test]
		public void TestRemoveRange()
		{
			var e = Enumerable.Range(-1, 5000);
			ListT list = _newList();
			List<int> list2 = new List<int>(e);
			list.AddRange(e);

			for (int iteration = 0; list2.Count > 0; iteration++)
			{
				int at = _r.Next(list2.Count+1);
				int amount = Math.Min(list2.Count - at, _r.Next(100));

				list.RemoveRange(at, amount);
				list2.RemoveRange(at, amount);
				
				if (iteration % 10 == 0)
					ExpectList(list, list2);
			}

			ExpectList(list, list2);
		}

		[Test]
		public void TestRemoveAll()
		{
			ListT list = _newList();
			list.AddRange(Enumerable.Range(0, _r.Next(5000)));
			list.RemoveRange(0, Count(list));
			ExpectList(list);
		}

		[Test]
		public void TestIterateRange()
		{
			ListT list = _newList();
			list.AddRange(Enumerable.Range(0, 5000));

			for (int i = 0; i < 100; i++)
			{
				int at = _r.Next(Count(list)+1);
				int amount = _r.Next(100);
				ExpectList(list.Slice(at, amount), 
					Enumerable.Range(at, Math.Min(amount, Count(list) - at)).ToArray());
			}
		}

		//[Test]
		//public void TestSort()
		//{
		//	for (int size = 0; size <= 2000; size = size*2 + _r.Next(4))
		//	{
		//		ListT list = _newList();
		//		List<int> list2 = new List<int>(size);
		//		int threshold = _r.Next(255);
		//		for (int i = 0; i < size; i++)
		//		{
		//			int n = _r.Next(size+1);
		//			if (_r.Next(256) < threshold) {
		//				// Front-inserts are needed to test DList<T>.Sort() thoroughly
		//				list.InsertRange(0, Range.Single(n));
		//				list2.Insert(0, n);
		//			} else {
		//				list.AddRange(Range.Single(n));
		//				list2.Add(n);
		//			}
		//		}

		//		//list.Sort(); has been removed from IListRangeMethods<T>
		//	}
		//}

		protected int StressTestIterations = 1000;
		protected int MaxListSize = 10000;

		[Test]
		public void StressTest()
		{
			ListT list = _newList();
			List<int> list2 = new List<int>();

			for (int i = 0; i < StressTestIterations; i++)
			{
				Assert.AreEqual(list2.Count, Count(list));
				int at = _r.Next(Count(list) + 1);
				int amount = _r.Next(30) * _r.Next(30); // parabolic distribution
				amount = StressTestIteration(ref list, list2, i, at, amount);
			}
		}

		// Note: list is passed by reference in case ListT is a value type (InternalList)
		protected virtual int StressTestIteration(ref ListT list, List<int> list2, int i, int at, int amount)
		{
			int act = _r.Next(3);

			if (act == 0 && Count(list) < MaxListSize)
			{
				IEnumerable<int> e = Enumerable.Range(i * 1000, amount);
				list.InsertRange(at, e);
				list2.InsertRange(at, e);
			}
			else if (act == 2)
			{
				while (act != 0 && amount > Count(list) - at)
					amount /= 2;
				list.RemoveRange(at, amount);
				list2.RemoveRange(at, amount);
			}
			else
			{
				int amount2 = Math.Min(amount, Count(list) - at);
				ExpectList(list.Slice(at, amount),
					list2.AsListSource().Slice(at, amount2).AsList());
			}
			return amount;
		}
	}
}
