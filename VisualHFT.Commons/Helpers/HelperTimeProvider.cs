using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT
{
    public static class HelperTimeProvider
    {
        private static DateTime? _fixedTime = null;

        public static void SetFixedTime(DateTime dateTime)
        {
            _fixedTime = dateTime;
        }

        public static void ResetToSystemTime()
        {
            _fixedTime = null;
        }

        public static DateTime Now
        {
            get { return _fixedTime ?? DateTime.Now; }
        }

    }
}
