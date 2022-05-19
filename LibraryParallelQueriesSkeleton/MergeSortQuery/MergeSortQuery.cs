using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

using LibraryModel;

namespace MergeSortQuery {
	class MergeSortQuery {
		public Library Library { get; set; }
		public int ThreadCount { get; set; }


		/// <summary>
		/// Filters out copies which are on loan and are located on the desired shelf.
		/// </summary>
		/// <param name="library"></param>
		/// <returns></returns>
		private List<Copy> FilterCopies(Library library)
		{
			List<Copy> filteredCopies = new List<Copy>();

			var allCopies = library.Copies;

			foreach (var copy in allCopies)
			{
                if (copy.State == CopyState.OnLoan)
                {
                    if ((copy.Book.Shelf[2] >= 'A' && copy.Book.Shelf[2] <= 'Q') && char.IsDigit(copy.Book.Shelf[0]) && char.IsDigit(copy.Book.Shelf[1]))
                    {
						filteredCopies.Add(copy);
                    }
                }
			}
			return filteredCopies;
		}

		public List<Copy> ExecuteQuery() {
			CopySorter sorter = new CopySorter(ThreadCount);
			if (ThreadCount == 0)
			{
				throw new InvalidOperationException("Threads property not set and default value 0 is not valid.");
			}
            else
            {
				var fileteredCopies = FilterCopies(Library);
				var sortedCopies = sorter.MergeSortMultiT(fileteredCopies);
				return sortedCopies;
            }
		}
	}

	class CopySorter
    {
		public int ThreadCount { get; set; }
		public CopyComparer Comparer = new CopyComparer();
		public CopySorter(int threadCount)
        {
			ThreadCount = threadCount;
        }

		/// <summary>
		/// Merges two lists of copies into a single list while maintaining the right ordering of the elements.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private List<Copy> Merge(List<Copy> left, List<Copy> right)
        {
			var mergedList = new List<Copy>();

			int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
				if (Comparer.Compare(left[i] ,right[j]) <= 0)
                {
					mergedList.Add(left[i]);
					i++;
                }
                else
                {
					mergedList.Add(right[j]);
					j++;
                }
			}
			
			while (i < left.Count)
            {
				mergedList.Add(left[i]);
				i++;
			}

			while (j < right.Count)
            {
				mergedList.Add(right[j]);
				j++;
			}
				
			return mergedList;
		}

		/// <summary>
		/// A multithreaded mergesort algorithm.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public List<Copy> MergeSortMultiT(List<Copy> list)
        {
			if (list.Count <= 0)
            {
				return list;
            }

			int middle = list.Count/2;
			var leftList = new List<Copy>();
			var rightList = new List<Copy>();

			

			if (ThreadCount > 1)
            {
				for (int i = 0; i < middle; i++)
				{
					leftList.Add(list[i]);
				}

				for (int i = middle; i < list.Count; i++)
				{
					rightList.Add(list[i]);
				}

				ThreadCount--;
				Thread t = new Thread(() => 
				{ 
					leftList = MergeSortMultiT(leftList);
				});

				t.Start();
				rightList = MergeSortMultiT(rightList);
				t.Join();
				return Merge(leftList, rightList);
			}
            else
            {
				list.Sort(Comparer);
				return list;
            }
			
		}
    }

	/// <summary>
	/// Comparer class for comparing different copies of books. 
	/// </summary>
	class CopyComparer : IComparer<Copy>
	{
		private int CompareID(Copy x, Copy y)
		{
		
			var res = String.Compare(x.Id, y.Id);
			if (res != 0)
			{
				return res;
			}
			return res;
		}
		private int CompareShelf(Copy x, Copy y)
		{
			var res = String.Compare(x.Book.Shelf, y.Book.Shelf);
			if (res != 0)
			{
				return res;
			}
			else
			{
				return CompareID(x, y);
			}
		}
		private int CompareFirstName(Copy x, Copy y)
		{
			var res = String.Compare(x.OnLoan.Client.FirstName, y.OnLoan.Client.FirstName);
			if (res != 0)
			{
				return res;
			}
			else
			{
				return CompareShelf(x, y);
			}
		}
		private int CompareLastName(Copy x, Copy y)
		{
			var res = String.Compare(x.OnLoan.Client.LastName, y.OnLoan.Client.LastName);
			if (res != 0)
			{
				return res;
			}
			else
			{
				return CompareFirstName(x, y);
			}
		}

		private int CompareDueDate(Copy x, Copy y)
		{
			var res = x.OnLoan.DueDate.CompareTo(y.OnLoan.DueDate);
			if (res != 0)
			{
				return res;
			}
			else
			{
				return CompareLastName(x, y);
			}
		}

		public int Compare(Copy x, Copy y)
		{
			return CompareDueDate(x, y);
		}
	}
	class ResultVisualizer {
		public static void PrintCopy(StreamWriter writer, Copy c) {
			writer.WriteLine("{0} {1}: {2} loaned to {3}, {4}.", c.OnLoan.DueDate.ToShortDateString(),
				c.Book.Shelf, c.Id, c.OnLoan.Client.LastName, System.Globalization.StringInfo.GetNextTextElement(c.OnLoan.Client.FirstName));
		}
	}
}
