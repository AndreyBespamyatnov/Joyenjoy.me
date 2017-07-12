using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace PhotoBooth.Mobile
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        private const string PhotoBoothIdKey = "Photo Booth Id";
        private static readonly Guid PhotoBoothIdDefault = Guid.Empty;

        private const string UserNameKey = "username_key";
        private static readonly string UserNameDefault = string.Empty;

        private const string SomeIntKey = "int_key";
        private static readonly int SomeIntDefault = 6251986;

        public static Guid PhotoBoothId
        {
            get { return AppSettings.GetValueOrDefault<Guid>(PhotoBoothIdKey, PhotoBoothIdDefault); }
            set { AppSettings.AddOrUpdateValue<Guid>(PhotoBoothIdKey, value); }
        }

        public static string UserName
        {
            get { return AppSettings.GetValueOrDefault<string>(UserNameKey, UserNameDefault); }
            set { AppSettings.AddOrUpdateValue<string>(UserNameKey, value); }
        }

        public static int SomeInt
        {
            get { return AppSettings.GetValueOrDefault<int>(SomeIntKey, SomeIntDefault); }
            set { AppSettings.AddOrUpdateValue<int>(SomeIntKey, value); }
        }
    }
}
