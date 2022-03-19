﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yapoml.Generation.Parsers;

namespace Yapoml.Generation
{
    public class GlobalGenerationContext
    {
        public GlobalGenerationContext(string rootDirectoryPath, string rootNamespace, IParser parser)
        {
            RootDirectoryPath = rootDirectoryPath.Replace("/", "\\").TrimEnd('\\');
            RootNamespace = rootNamespace;
            Parser = parser;
        }

        private IParser Parser { get; }

        public string RootDirectoryPath { get; }

        public string RootNamespace { get; }

        public IList<SpaceGenerationContext> Spaces { get; } = new List<SpaceGenerationContext>();

        public IList<PageGenerationContext> Pages { get; } = new List<PageGenerationContext>();

        public IList<ComponentGenerationContext> Components { get; } = new List<ComponentGenerationContext>();

        public void AddFile(string filePath)
        {
            var space = CreateOrAddSpaces(filePath);

            if (filePath.ToLowerInvariant().EndsWith(".po.yaml"))
            {
                var page = Parser.ParsePage(filePath);
                var fileName = Path.GetFileName(filePath);
                var pageName = fileName.Substring(0, fileName.Length - ".po.yaml".Length);

                PageGenerationContext pageContext;

                if (space == null)
                {
                    pageContext = new PageGenerationContext(pageName, this, null, page);

                    Pages.Add(pageContext);
                }
                else
                {
                    pageContext = new PageGenerationContext(pageName, this, space, page);

                    space.Pages.Add(pageContext);
                }

                if (page.BasePage != null)
                {
                    _inheritedPages.Add(pageContext, page.BasePage);
                }
            }
            else if (filePath.ToLowerInvariant().EndsWith(".pc.yaml"))
            {
                var component = Parser.ParseComponent(filePath);

                var fileName = Path.GetFileName(filePath);

                var componentName = fileName.Substring(0, fileName.Length - ".pc.yaml".Length);

                if (space == null)
                {
                    var componentContext = new ComponentGenerationContext(componentName, this, null, component);

                    Components.Add(componentContext);
                }
                else
                {
                    var componentContext = new ComponentGenerationContext(componentName, this, space, component);

                    space.Components.Add(componentContext);
                }
            }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString();
            }
        }

        public void ResolveReferences()
        {
            ResolveInheritedPages();
        }

        private IDictionary<PageGenerationContext, string> _inheritedPages = new Dictionary<PageGenerationContext, string>();

        private void ResolveInheritedPages()
        {
            foreach (var pageContext in _inheritedPages)
            {
                var basePageName = pageContext.Value;
                var childPage = pageContext.Key;

                foreach (var page in Pages)
                {
                    if (EvaluatePage(page, basePageName))
                    {
                        childPage.BasePageContext = page;
                    }
                }

                if (childPage.BasePageContext == null)
                {
                    foreach(var space in Spaces)
                    {
                        var basePage = DiscoverSpace(space, basePageName);

                        if (basePage != null)
                        {
                            childPage.BasePageContext = basePage;
                        }
                    }
                }

                if (childPage.BasePageContext == null)
                {
                    throw new Exception($"Cannot resolve '{basePageName}' base page for '{childPage.Name}' page.");
                }
            }
        }

        private PageGenerationContext DiscoverSpace(SpaceGenerationContext space, string basePageName)
        {
            PageGenerationContext basePage = null;

            foreach (var page in space.Pages)
            {
                if (EvaluatePage(page, basePageName))
                {
                    return page;
                }
            }

            foreach (var innerSpace in space.Spaces)
            {
                return DiscoverSpace(innerSpace, basePageName);
            }

            return basePage;
        }

        private bool EvaluatePage(PageGenerationContext page, string basePageName)
        {
            return page.Name.Equals(basePageName, StringComparison.OrdinalIgnoreCase);
        }

        private SpaceGenerationContext CreateOrAddSpaces(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);

            var path = directory.Substring(RootDirectoryPath.Length);

            var parts = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 0)
            {
                SpaceGenerationContext nestedSpace = Spaces.FirstOrDefault(s => s.Namespace == $"{RootNamespace}.{parts[0]}");

                if (nestedSpace == null)
                {
                    nestedSpace = new SpaceGenerationContext(parts[0], this, null);

                    Spaces.Add(nestedSpace);
                }

                for (int i = 1; i < parts.Length; i++)
                {
                    var candidateNestedSpace = nestedSpace.Spaces.FirstOrDefault(s => s.Name == parts[i]);

                    if (candidateNestedSpace == null)
                    {
                        var newNestedSpace = new SpaceGenerationContext(parts[i], this, nestedSpace);

                        nestedSpace.Spaces.Add(newNestedSpace);

                        nestedSpace = newNestedSpace;
                    }
                    else
                    {
                        nestedSpace = candidateNestedSpace;
                    }
                }

                return nestedSpace;
            }
            else
            {
                return null;
            }
        }
    }
}
