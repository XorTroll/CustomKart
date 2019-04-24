using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CustomKart.UI
{
    public static class Utils
    {
        public static string GetResource(string Key)
        {
            return (Application.Current.MainWindow as MainWindow).FindResource(Key) as string;
        }

        public static void ShowMessage(string Message)
        {
            (Application.Current.MainWindow as MainWindow).ShowNotification(Message);
        }

        public static void ShowMessageWithOption(string Message, string Option, Action ToCall)
        {
            (Application.Current.MainWindow as MainWindow).ShowNotificationWithOption(Message, Option, ToCall);
        }

        public static string GetNDSFilter()
        {
            return GetResource("ndsROM") + " (" + GetResource("commonExt") + ")|*.nds|" + GetResource("ndsROM") + " (" + GetResource("officialExt") + ")|*.srl";
        }
    }
}
