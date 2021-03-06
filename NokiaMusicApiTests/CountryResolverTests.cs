﻿// -----------------------------------------------------------------------
// <copyright file="CountryResolverTests.cs" company="NOKIA">
// Copyright (c) 2013, Nokia
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Threading.Tasks;
using Nokia.Music.Commands;
using Nokia.Music.Internal.Request;
using Nokia.Music.Tests.Internal;
using Nokia.Music.Tests.Properties;
using NUnit.Framework;

namespace Nokia.Music.Tests
{
    [TestFixture]
    public class CountryResolverTests
    {
        [Test]
        public void CheckApiCredentialsValidated()
        {
            string nullKey = null;
            Assert.Throws(typeof(ApiCredentialsRequiredException), new TestDelegate(() => { new CountryResolver(nullKey); }));
        }

        [Test]
        public void EnsureDefaultRequestHandlerIsCreated()
        {
            CountryResolver client = new CountryResolver("test");
            Assert.AreEqual(client.RequestHandler.GetType(), typeof(ApiRequestHandler), "Expected the default handler");
        }

        [Test]
        [ExpectedException(typeof(InvalidApiCredentialsException))]
        public async Task EnsureInvalidApiCredentialsExceptionThrownWhenServerGives403()
        {
            CountryResolver client = new CountryResolver("badkey", new MockApiRequestHandler(FakeResponse.Forbidden()));
            await client.CheckAvailabilityAsync("gb");
        }

        [Test]
        [ExpectedException(typeof(InvalidCountryCodeException))]
        public async Task EnsureCheckAvailabilityThrowsExceptionForNullCountryCode()
        {
            ICountryResolver client = new CountryResolver("test", new MockApiRequestHandler(Resources.country));
            await client.CheckAvailabilityAsync(null);
        }

        [Test]
        public void ApiMethodsDefaultToGetHttpMethod()
        {
            var resolver = new CountryResolverCommand("test", new MockApiRequestHandler(Resources.country));
            Assert.AreEqual(HttpMethod.Get, resolver.HttpMethod);
        }

        [Test]
        public void ApiMethodsDefaultToNullContentType()
        {
            var resolver = new CountryResolverCommand("test", new MockApiRequestHandler(Resources.country));
            Assert.IsNull(resolver.ContentType);
        }

        [Test]
        public async Task EnsureCheckAvailabilityWorksForValidCountry()
        {
            CountryResolver client = new CountryResolver("test", new MockApiRequestHandler(Resources.country));
            bool result = await client.CheckAvailabilityAsync("gb");
            Assert.IsTrue(result, "Expected a true result");
        }

        [Test]
        public async Task EnsureCheckAvailabilityReturnsFailsForInvalidCountry()
        {
            CountryResolver client = new CountryResolver("test", new MockApiRequestHandler(FakeResponse.NotFound("{}")));
            bool result = await client.CheckAvailabilityAsync("xx");
            Assert.IsFalse(result, "Expected a false result");
        }

        [Test]
        [ExpectedException(typeof(ApiCallFailedException))]
        public async Task EnsureCheckAvailabilityIsTreatedAsErrorForNetworkFailure()
        {
            CountryResolver client = new CountryResolver("test", new MockApiRequestHandler(FakeResponse.NotFound()));
            bool result = await client.CheckAvailabilityAsync("xx");
        }

        [Test]
        public async Task EnsureCountryResolverPassesDefaultSettings()
        {
            MockApiRequestHandler mockHandler = new MockApiRequestHandler(Resources.country);
            ICountryResolver client = new CountryResolver("test1", mockHandler);
            bool result = await client.CheckAvailabilityAsync("xx");
            Assert.AreEqual("test1", mockHandler.LastUsedSettings.ClientId);
            Assert.AreEqual(null, mockHandler.LastUsedSettings.CountryCode);
            Assert.AreEqual(false, mockHandler.LastUsedSettings.CountryCodeBasedOnRegionInfo);
        }

        [Test]
        [ExpectedException(typeof(ApiCallFailedException))]
        public async Task EnsureCheckAvailabilityReturnsErrorForFailedCall()
        {
            CountryResolver client = new CountryResolver("test", new MockApiRequestHandler(FakeResponse.GatewayTimeout()));
            bool result = await client.CheckAvailabilityAsync("gb");
        }
    }
}
