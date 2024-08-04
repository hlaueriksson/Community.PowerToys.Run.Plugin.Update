using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;

namespace Community.PowerToys.Run.Plugin.Update.Tests.Grok
{
    public class RuntimeInformationTests
    {
        [Test]
        public void ProcessArchitecture_should_work()
        {
            RuntimeInformation.ProcessArchitecture.Should().Be(Architecture.X64);
        }

        [Test]
        public void OSArchitecture_should_work()
        {
            RuntimeInformation.OSArchitecture.Should().Be(Architecture.X64);
        }
    }
}
