using System;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestTestProject
{
// NOTE: Flurl throws on any non-200 response
    [TestClass]
    public class GetUsersRestsTests : RestBaseClass
    {
        private const string PathSegment = "/api/users";

        [TestMethod]
        public async Task VerifyTotalUserCountEqualsTotalDataArraySizeAsync()
        {
            var endPointUrl = BaseUrl.AppendPathSegment(PathSegment)
                .SetQueryParam("page",1);

            // If you are interested in the actual response code then you need to capture the response before deserialization
            var response = await endPointUrl.GetAsync();
            response.StatusCode.Should().Be(200, "We only allow 200 for this test vs. the range of 200..299 which is typically allowed.");
            ResponseCode = response.StatusCode;

            var usersPage = await response.GetJsonAsync()
                .ConfigureAwait(true);

            int userCount = 0;
            while (usersPage.page <= usersPage.total_pages) // 1 based loop
            {
                userCount += (int)usersPage.data.Count;
                usersPage = await endPointUrl.SetQueryParam("page", (int)usersPage.page + 1)
                    .GetJsonAsync()
                    .ConfigureAwait(true);
            }
          
            TestContext.WriteLine($"The sum of all Data array.counts (userCount) is: {userCount} and the total of all users (usersPage.total) is: {usersPage.total}");
            userCount.Should().Be((int)usersPage.total,
                "The Users object total was not equal to the count of all the Users data array counts.");
        }

        [TestMethod]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(15)]
        public async Task VerifyChangingUsersPerPageModifiesTheNumberOfUsersPerPageAsync(int perPageValue)
        {
            var endPointUrl = BaseUrl.AppendPathSegment(PathSegment)
                .SetQueryParam("page", 1)
                .SetQueryParam("per_page", perPageValue);

            // If you are desperate for speed increase, don't use dynamic as the data is stored on the heap.
            dynamic usersPage = await endPointUrl
                .GetJsonAsync()
                .ConfigureAwait(true);

            while (usersPage.page <= usersPage.total_pages) // 1 based loop
            {
                // We are not testing the case where page > total_pages - fyi if we did the correct answer is 0.
                int correctDataCount = usersPage.page < usersPage.total_pages ? perPageValue 
                    : (int)usersPage.total % perPageValue == 0  ? perPageValue
                    : (int)usersPage.total % perPageValue;

                int userCount = (int)usersPage.data.Count;

                // ok for debugging, but shouldn't be in checked-in code. If wanted use Logger.
                TestContext.WriteLine($"The length of Data array (userCount) is: {userCount} and my calculated correctDataCount is: {correctDataCount}");
                
                userCount.Should().Be(correctDataCount, "The number of users in the data array is not correct.");

                usersPage = await endPointUrl
                    .SetQueryParam("page", (int)usersPage.page + 1)
                    .GetJsonAsync()
                    .ConfigureAwait(true);
            }
        }

        #region InitAndCleanup
        // Test Case init and cleanup

        [TestInitialize]
        public void TestCaseInit()
        {
            // Count something, Log title and timestamp
            TestContext.WriteLine($"{TestContext.TestName} Starting.");
        }

        [TestCleanup]
        public void TestCaseCleanup()
        {
            // Maybe custom logging, you could do a runtime timestamp, or perhaps on a fail add files/screenshot to results
            if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            {
                var responseMsg = ResponseCode == 0 ? "between 200 and 299" : ResponseCode.ToString();
                TestContext.WriteLine($"The response status code was: {responseMsg}.");
            }

            TestContext.WriteLine($"{TestContext.TestName} Ending.");
        }
        #endregion

    }
}
