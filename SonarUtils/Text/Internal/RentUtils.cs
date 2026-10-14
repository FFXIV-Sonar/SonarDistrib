using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SonarUtils.Text.Internal
{
    internal static class RentUtils
    {
        public static int[][] RentMatrix(int width, int height)
        {
            var arrays = ArrayPool<int[]>.Shared.Rent(width);
            foreach (ref var array in arrays.AsSpan(0, width))
            {
                array = ArrayPool<int>.Shared.Rent(height);
                // Pooled primitive arrays from the array pool are created uninitialized: GC.AllocateUninitializedArray<T>(minimumLength)
                // This clear ensure they're initialized.
                Array.Clear(array, 0, height);
            }
            return arrays;
        }

        public static void ReturnMatrix(int[][] arrays)
        {
            foreach (ref var arrayRef in arrays.AsSpan())
            {
                var array = arrayRef;
                if (array is not null) ArrayPool<int>.Shared.Return(array, false); // Cleared on rent due to this being a primitive array
            }
            ArrayPool<int[]>.Shared.Return(arrays, true);
        }
    }
}
