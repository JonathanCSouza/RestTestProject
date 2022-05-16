using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestTestProject
{
    [TestClass]
    public class RestBaseClass
    {
        protected readonly string BaseUrl = "https://reqres.in/";
        public TestContext TestContext { get; set; }

        #region InitAndCleanup
        // Class and/or Assembly init and cleanup

        [ClassCleanup]
        public void GetUsersTestClassCleanup()
        {
            // close handles, do some logging
        }

        [ClassInitialize]
        public void GetUsersTestClassInit(TestContext testContext)
        {
            TestContext = testContext;
            // ensure a base state, kill off potentially running processes
        }
        #endregion

    }
}