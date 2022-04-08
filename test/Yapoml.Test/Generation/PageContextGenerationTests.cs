﻿using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using Yapoml.Generation;
using Yapoml.Generation.Parsers;

namespace Yapoml.Test.Generation
{
    internal class PageContextGenerationTests
    {
        private Parser _parser = new Parser();

        [Test]
        public void Parse_Page()
        {
            File.WriteAllText("my_page.po.yaml", @"
C1:
  by: qwe
"
                );

            var gc = new GlobalGenerationContext(Environment.CurrentDirectory, "A.B", _parser);

            gc.AddFile(Path.Combine(Environment.CurrentDirectory, "my_page.po.yaml"));

            gc.Spaces.Should().BeEmpty();

            gc.Pages.Should().HaveCount(1);
            var page = gc.Pages[0];
            page.Name.Should().Be("my_page");
            page.Namespace.Should().Be("A.B");

            gc.Pages[0].Components.Should().HaveCount(1);

            gc.Pages[0].Url.Should().BeNull();
        }

        [Test]
        public void Parse_Page_Url()
        {
            File.WriteAllText("my_page.po.yaml", @"
url:
  path: projects/{projectId}/users/{userId}/roles
  params:
    - count
    - offset
"
                );

            var gc = new GlobalGenerationContext(Environment.CurrentDirectory, "A.B", _parser);

            gc.AddFile(Path.Combine(Environment.CurrentDirectory, "my_page.po.yaml"));

            var url = gc.Pages[0].Url;

            url.Path.Should().Be("projects/{projectId}/users/{userId}/roles");
            url.Params.Should().HaveCount(2);

            var countParam = url.Params[0];
            countParam.Should().Be("count");


            var offsetParam = url.Params[1];
            offsetParam.Should().Be("offset");

            url.Segments.Should().HaveCount(2);

            var projectSegment = url.Segments[0];
            projectSegment.Should().Be("projectId");

            var userIdSegment = url.Segments[1];
            userIdSegment.Should().Be("userId");
        }
    }
}
