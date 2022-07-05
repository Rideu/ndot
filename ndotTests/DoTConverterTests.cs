using Microsoft.VisualStudio.TestTools.UnitTesting;

using ndot;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ndot.Tests
{
    [TestClass()]
    public class DnsResponseTests
    {
        [TestMethod()]
        public void DnsResponse_FromRaw()
        {

            DoTConverter dc = new DoTConverter();
            dc.Open(54);

            var domain = "monitoring.internet-measurement.com";
            var acheck = DnsClient.ResolveNameAsync(domain, DnsRequestType.A, false, 54).Result;
            Assert.IsTrue(acheck.Answers.Count > 0);
        }
    }
}