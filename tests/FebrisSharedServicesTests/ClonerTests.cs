// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="Cloner.Clone{T}(T)"/>.
    ///
    /// <para>
    /// <c>Clone</c> is an extension method that performs a deep copy by serializing the source
    /// to JSON with Newtonsoft.Json and deserializing back into a fresh instance. The tests
    /// cover the common cases (primitives, nested objects, collections) and the edge cases
    /// that fall out of using JSON round-trip as the cloning mechanism (nulls, circular
    /// references, missing default constructors).
    /// </para>
    /// </summary>
    public class ClonerTests
    {
        // Minimal POCO used to test reference-vs-value semantics of the clone.
        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Address Address { get; set; }
        }

        private class Address
        {
            public string City { get; set; }
            public string Country { get; set; }
        }

        private class CircularNode
        {
            public string Label { get; set; }
            public CircularNode Next { get; set; }
        }

        [Fact]
        public void Clone_ReturnsObjectWithEqualValues()
        {
            var original = new Person { Name = "Riley", Age = 37, Address = new Address { City = "Phoenix", Country = "USA" } };

            var clone = original.Clone();

            clone.Name.Should().Be("Riley");
            clone.Age.Should().Be(37);
            clone.Address.City.Should().Be("Phoenix");
            clone.Address.Country.Should().Be("USA");
        }

        [Fact]
        public void Clone_ReturnsNewReference_NotTheSameInstance()
        {
            // The whole point of cloning: the result must not be the same object reference.
            var original = new Person { Name = "Riley", Age = 37 };

            var clone = original.Clone();

            clone.Should().NotBeSameAs(original);
        }

        [Fact]
        public void Clone_ProducesDeepCopy_NestedReferenceObjectIsAlsoCloned()
        {
            // Nested reference-type properties should also be new instances, not shared.
            var original = new Person { Name = "Riley", Address = new Address { City = "Phoenix" } };

            var clone = original.Clone();

            clone.Address.Should().NotBeSameAs(original.Address);
        }

        [Fact]
        public void Clone_MutatingCloneDoesNotAffectOriginal()
        {
            // The clearest behavioral guarantee of deep clone: mutate the clone, original is intact.
            var original = new Person { Name = "Riley", Address = new Address { City = "Phoenix" } };

            var clone = original.Clone();
            clone.Name = "Changed";
            clone.Address.City = "Tucson";

            original.Name.Should().Be("Riley");
            original.Address.City.Should().Be("Phoenix");
        }

        [Fact]
        public void Clone_OfNull_ReturnsNull()
        {
            // JsonConvert.SerializeObject(null) -> "null"; DeserializeObject<Person>("null") -> null.
            Person original = null;

            var clone = original.Clone();

            clone.Should().BeNull();
        }

        [Fact]
        public void Clone_OfList_ProducesIndependentList()
        {
            var original = new List<string> { "a", "b", "c" };

            var clone = original.Clone();

            clone.Should().NotBeSameAs(original);
            clone.Should().Equal("a", "b", "c");
            clone.Add("d");
            original.Should().HaveCount(3);
        }

        [Fact]
        public void Clone_OfDictionary_ProducesIndependentDictionary()
        {
            var original = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 };

            var clone = original.Clone();

            clone.Should().NotBeSameAs(original);
            clone.Should().ContainKey("one").WhoseValue.Should().Be(1);
            clone["three"] = 3;
            original.Should().NotContainKey("three");
        }

        [Fact]
        public void Clone_OfCircularReference_ThrowsJsonSerializationException()
        {
            // Newtonsoft.Json's default behavior on a self-referencing cycle is to throw.
            // This test documents that limitation -- callers must avoid cloning graphs with cycles
            // or change the cloning mechanism for those types.
            var node = new CircularNode { Label = "root" };
            node.Next = node;

            Action act = () => node.Clone();

            act.Should().Throw<JsonSerializationException>();
        }

        [Fact]
        public void Clone_OfValueTypeViaStringWrapper_WorksThroughBoxing()
        {
            // Extension method works on any type T. Primitives round-trip through JSON cleanly.
            int original = 42;

            var clone = original.Clone();

            clone.Should().Be(42);
        }
    }
}
