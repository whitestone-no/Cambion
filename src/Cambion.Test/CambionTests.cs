using NUnit.Framework;
using System;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.Test
{
    [Order(3)]
    class CambionTests
    {
        private ICambion _cambion;

        [SetUp]
        public void Setup()
        {
            _cambion = new CambionConfiguration()
                .Serializer.UseJsonNet()
                .Transport.UseLoopback()
                .Create();
        }


        [Test]
        public void Register_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _cambion.Register(null); });
        }
    }
}
