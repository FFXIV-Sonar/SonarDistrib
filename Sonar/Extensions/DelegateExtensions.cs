
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Extensions
{
    [SuppressMessage("Major Code Smell", "S1121")]
    [SuppressMessage("Minor Code Smell", "S4136")]
	public static class DelegateExtensions
	{
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke(this Action del, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke();
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke(this Action del)
		{
			foreach (var item in Unsafe.As<Action[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke();
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1>(this Action<T1> del, T1 arg1, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1>(this Action<T1> del, T1 arg1)
		{
			foreach (var item in Unsafe.As<Action<T1>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2>(this Action<T1, T2> del, T1 arg1, T2 arg2, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2>(this Action<T1, T2> del, T1 arg1, T2 arg2)
		{
			foreach (var item in Unsafe.As<Action<T1, T2>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> del, T1 arg1, T2 arg2, T3 arg3, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> del, T1 arg1, T2 arg2, T3 arg3)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
		{
			foreach (var item in Unsafe.As<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>[]>(del.GetInvocationList()))
			{
				try
				{
					item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<TResult>(this Func<TResult> del, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke());
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<TResult>(this Func<TResult> del)
		{
			var invocations = Unsafe.As<Func<TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke());
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, TResult>(this Func<T1, TResult> del, T1 arg1, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, TResult>(this Func<T1, TResult> del, T1 arg1)
		{
			var invocations = Unsafe.As<Func<T1, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, TResult>(this Func<T1, T2, TResult> del, T1 arg1, T2 arg2, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, TResult>(this Func<T1, T2, TResult> del, T1 arg1, T2 arg2)
		{
			var invocations = Unsafe.As<Func<T1, T2, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> del, T1 arg1, T2 arg2, T3 arg3, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> del, T1 arg1, T2 arg2, T3 arg3)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
			return results;
		}

		/// <summary>Invoke safely while swallowing exceptions</summary>
		public static IEnumerable<TResult> SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
		{
			var invocations = Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>[]>(del.GetInvocationList());
			List<TResult> results = new(invocations.Length);
			for (var i = 0; i < invocations.Length; i++)
			{
				try
				{
					results.Add(invocations[i].Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
				}
				catch
				{
					/* Swallow */
				}
			}
			return results;
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<TResult>(this Func<TResult> del, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke();
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<TResult>(this Func<TResult> del)
		{
			foreach (var item in Unsafe.As<Func<TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke();
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, TResult>(this Func<T1, TResult> del, T1 arg1, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, TResult>(this Func<T1, TResult> del, T1 arg1)
		{
			foreach (var item in Unsafe.As<Func<T1, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, TResult>(this Func<T1, T2, TResult> del, T1 arg1, T2 arg2, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, TResult>(this Func<T1, T2, TResult> del, T1 arg1, T2 arg2)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> del, T1 arg1, T2 arg2, T3 arg3, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> del, T1 arg1, T2 arg2, T3 arg3)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while discarding results and capturing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out IEnumerable<Exception> exceptions)
		{
		    List<Exception>? exs = null;
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
				}
				catch (Exception ex)
				{
				    (exs ??= new()).Add(ex);
				}
			}
			exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
		}

		/// <summary>Invoke safely while discarding results and swallowing exceptions</summary>
		public static void SafeInvokeAndDiscard<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> del, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
		{
			foreach (var item in Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>[]>(del.GetInvocationList()))
			{
				try
				{
					_ = item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
				}
				catch
				{
					/* Swallow */
				}
			}
		}

		
		/// <summary>Invoke safely while capturing exceptions</summary>
        public static void SafeInvoke(this EventHandler handler, object sender, EventArgs e, out IEnumerable<Exception> exceptions)
        {
            List<Exception>? exs = null;
            foreach (var item in Unsafe.As<EventHandler[]>(handler.GetInvocationList()))
            {
                try
                {
                    item.Invoke(sender, e);
                }
                catch (Exception ex)
                {
                    (exs ??= new()).Add(ex);
                }
            }
            exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
        }

		/// <summary>Invoke safely while capturing exceptions</summary>
        public static void SafeInvoke<T>(this EventHandler<T> handler, object sender, T e, out IEnumerable<Exception> exceptions)
        {
            List<Exception>? exs = null;
            foreach (var item in Unsafe.As<EventHandler<T>[]>(handler.GetInvocationList()))
            {
                try
                {
                    item.Invoke(sender, e);
                }
                catch (Exception ex)
                {
                    (exs ??= new()).Add(ex);
                }
            }
            exceptions = exs is not null ? exs : Enumerable.Empty<Exception>();
        }

		/// <summary>Invoke safely while swallowing exceptions</summary>
        public static void SafeInvoke(this EventHandler handler, object sender, EventArgs e)
        {
            foreach (var item in Unsafe.As<EventHandler[]>(handler.GetInvocationList()))
            {
                try
                {
                    item.Invoke(sender, e);
                }
                catch
                {
					/* Swallow */
                }
            }
        }

		/// <summary>Invoke safely while swallowing exceptions</summary>
        public static void SafeInvoke<T>(this EventHandler<T> handler, object sender, T e)
        {
            foreach (var item in Unsafe.As<EventHandler<T>[]>(handler.GetInvocationList()))
            {
                try
                {
                    item.Invoke(sender, e);
                }
                catch
                {
					/* Swallow */
                }
            }
        }
	}
}