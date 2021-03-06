﻿//-----------------------------------------------------------------------
// <copyright file="ElementStiffnessMatrixBuilderFactory.cs" company="Iain Sproat">
//     Copyright Iain Sproat, 2012.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpFE.Stiffness
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Factory to create ElementStiffnessMatrixBuilders for the correct finite element type
    /// </summary>
    public class ElementStiffnessMatrixBuilderFactory : IElementStiffnessMatrixBuilderFactory
    {
        /// <summary>
        /// 
        /// </summary>
        private IDictionary<Type, Type> lookup = new Dictionary<Type, Type>();
        
        /// <summary>
        /// 
        /// </summary>
        public ElementStiffnessMatrixBuilderFactory()
        {
            this.RegisterTypes();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public IElementStiffnessCalculator Create<T>(T element)
            where T : IFiniteElement
        {
            Guard.AgainstNullArgument(element, "element");
            Guard.AgainstBadArgument("element",
                                     () => { return !this.lookup.ContainsKey(element.GetType()); },
                                     "ElementStiffnessMatrixBuilderFactory has not registered a builder for the element type {0}",
                                     element.GetType().FullName);
            
            Type builderType = this.lookup[element.GetType()];
            object[] parameters = new object[]
            {
                element
            };
            object builder = Activator.CreateInstance(builderType, parameters);
            Guard.AgainstInvalidState(() => { return builder == null; },
                                      "Failed to create an instance of type {0}. Most likely as the type does not implement a constructor expecting a single parameter of type FiniteElement",
                                      builderType.FullName);
            
            IElementStiffnessCalculator castBuilder = builder as IElementStiffnessCalculator;
            Guard.AgainstInvalidState(() => { return castBuilder == null; },
                                      "Tried to cast to IStiffnessMatrixBuilder but it is null.  Most likely because the registered builder, {0}, for element type {1} does not inherit from IStiffnessMatrixBuilder",
                                      builderType.FullName,
                                      element.GetType().FullName);
            
            
            return castBuilder;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void RegisterTypes()
        {
            this.lookup.Add(typeof(LinearConstantSpring), typeof(LinearTrussStiffnessMatrixBuilder));
            this.lookup.Add(typeof(LinearTruss), typeof(LinearTrussStiffnessMatrixBuilder));
            this.lookup.Add(typeof(Linear1DBeam), typeof(Linear1DBernoulliBeamStiffnessMatrixBuilder));
            this.lookup.Add(typeof(Linear3DBeam), typeof(Linear3DBernoulliBeamStiffnessMatrixBuilder));
            this.lookup.Add(typeof(LinearConstantStrainTriangle), typeof(LinearConstantStrainTriangleStiffnessMatrixBuilder));
            this.lookup.Add(typeof(LinearConstantStressQuadrilateral), typeof(LinearConstantStressQuadrilateralStiffnessMatrixBuilder));
        }
    }
}
