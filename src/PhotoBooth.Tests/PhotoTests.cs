using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using PhotoBooth.Models;
using PhotoBooth.Service.Helpers;

namespace PhotoBooth.Tests
{
    [TestClass]
    public class PhotoTests
    {
        static readonly Logger SecondTaskLogger = LogManager.GetLogger("secondTaskFile");

        [TestMethod]
        public void AddNew()
        {
            var photo = new Photo();

            bool addPhoto = ContextHelper.Instance.AddPhoto(photo, SecondTaskLogger);
            Assert.IsTrue(addPhoto);
        }
    }
}
