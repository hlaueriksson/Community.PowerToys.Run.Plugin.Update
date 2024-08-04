using FluentAssertions;
using NUnit.Framework;

namespace Community.PowerToys.Run.Plugin.Update.Tests.Grok
{
    public class VersionTests
    {
        [Test]
        public void Prefix_should_not_work()
        {
            Action act = () => new Version("v0.82.1");
            act.Should().Throw<FormatException>();
        }

        [Test]
        public void Operators_should_work()
        {
            var previous = new Version("0.82.0");
            var current = new Version("0.82.1");
            var next = new Version("0.83.0");

            (current > previous).Should().BeTrue();
            (current < next).Should().BeTrue();
            (current != next).Should().BeTrue();
            (current == new Version("0.82.1")).Should().BeTrue();
        }
    }
}
