﻿using System.Collections.Generic;
using Yapoml.Framework.Workspace.Parsers.Yaml.Pocos;
using Yapoml.Framework.Workspace.Services;

namespace Yapoml.Framework.Workspace
{
    public class ComponentContext
    {
        public ComponentContext(WorkspaceContext workspace, SpaceContext space, PageContext page, ComponentContext parentComponent, Component component, INameNormalizer nameNormalizer)
        {
            Workspace = workspace;
            Space = space;

            Name = nameNormalizer.Normalize(component.Name);

            if (component.By != null)
            {
                By = new ByContext(component.By.Method, component.By.Value);
            }

            if (parentComponent != null)
            {
                Namespace = $"{parentComponent.Namespace}.{parentComponent.SingularName}Component";
            }
            else if (page != null)
            {
                Namespace = $"{page.Namespace}.{page.Name}";
            }
            else if (space != null)
            {
                Namespace = space.Namespace;
            }
            else
            {
                Namespace = workspace.RootNamespace;
            }

            if (component.Components != null)
            {
                foreach (var nestedComponent in component.Components)
                {
                    Components.Add(new ComponentContext(workspace, space, null, this, nestedComponent, nameNormalizer));
                }
            }

            if (component.Ref != null)
            {
                ReferencedComponentName = nameNormalizer.Normalize(component.Ref);
            }

            if (component.BaseComponent != null)
            {
                BaseComponentName = nameNormalizer.Normalize(component.BaseComponent);
            }
        }

        public WorkspaceContext Workspace { get; }

        public SpaceContext Space { get; }

        public string Name { get; }

        public string Namespace { get; }

        public ByContext By { get; set; }

        public string ReferencedComponentName { get; }

        public ComponentContext ReferencedComponent { get; set; }

        public string BaseComponentName { get; }

        public ComponentContext BaseComponent { get; set; }

        private bool? _isPlural;

        public bool IsPlural
        {
            get
            {

                if (!_isPlural.HasValue)
                {
                    _isPlural = new PluralizationService().IsPlural(Name);
                }

                return _isPlural.Value;
            }
        }

        private string _singularName;

        public string SingularName
        {
            get
            {
                if (_singularName is null)
                {
                    _singularName = new PluralizationService().Singularize(Name);
                }

                return _singularName;
            }
        }

        public IList<ComponentContext> Components { get; } = new List<ComponentContext>();

        public class ByContext
        {
            public ByContext(By.ByMethod method, string value)
            {
                Method = method;
                Value = value;
                Segments = SegmentsParser.ParseSegments(value);
            }

            public By.ByMethod Method { get; }
            public string Value { get; }
            public IList<string> Segments { get; }
        }
    }
}
