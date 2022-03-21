﻿using System.Collections.Generic;
using Yapoml.Generation.Parsers.Yaml.Pocos;

namespace Yapoml.Generation
{
    public class ComponentGenerationContext
    {
        public ComponentGenerationContext(string name, GlobalGenerationContext globalGenerationContext, SpaceGenerationContext spaceGenerationContext, Component component)
        {
            GlobalGenerationContext = globalGenerationContext;
            SpaceGenerationContext = spaceGenerationContext;

            if (!string.IsNullOrEmpty(component.Name))
            {
                Name = component.Name;
            }
            else
            {
                Name = name;
            }

            By = component.By;

            if (spaceGenerationContext != null)
            {
                Namespace = spaceGenerationContext.Namespace;
            }
            else
            {
                Namespace = globalGenerationContext.RootNamespace;
            }

            if (component.Components != null)
            {
                foreach (var nestedComponent in component.Components)
                {
                    ComponentGenerationContextes.Add(new ComponentGenerationContext(nestedComponent.Name, globalGenerationContext, spaceGenerationContext, nestedComponent));
                }
            }
        }

        public GlobalGenerationContext GlobalGenerationContext { get; }

        public SpaceGenerationContext SpaceGenerationContext { get; }

        public string Name { get; }

        public string Namespace { get; }

        public By By { get; }

        public bool IsPlural
        {
            get
            {
                var pluralityService = new Services.PluralizationService();

                return pluralityService.IsPlural(Name);
            }
        }

        public string SingularName
        {
            get
            {
                var pluralityService = new Services.PluralizationService();

                return pluralityService.Singularize(Name);
            }
        }

        public IList<ComponentGenerationContext> ComponentGenerationContextes { get; } = new List<ComponentGenerationContext>();
    }
}
