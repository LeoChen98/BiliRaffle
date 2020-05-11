using System;
using System.Globalization;
using System.Windows.Data;

namespace BiliRaffle
{
    public class IsEqualConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (int.Parse(value.ToString()) == int.Parse(parameter.ToString()))
                    return true;
                else
                    return false;
            }
            catch
            { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }

    public class IsNullOrEmptyConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}