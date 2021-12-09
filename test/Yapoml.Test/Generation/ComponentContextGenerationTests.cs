﻿using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using Yapoml.Generation;
using Yapoml.Parsers;

namespace Yapoml.Test.Generation
{
    internal class ComponentContextGenerationTests
    {
        private Parser _parser = new Parser();

        [Test]
        public void Parse_Component()
        {
            File.WriteAllText("my_page.pc.yaml", @"
name: c1
by: qwe
"
                );

            var gc = new GlobalGenerationContext(Environment.CurrentDirectory, "A.B", _parser);

            gc.AddFile($"{Environment.CurrentDirectory}\\my_page.pc.yaml");

            gc.Spaces.Should().BeEmpty();

            gc.Components.Should().HaveCount(1);
        }
    }
}
