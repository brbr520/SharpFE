﻿//-----------------------------------------------------------------------
// <copyright file="SpringElementTest.cs" company="SharpFE">
//     Copyright Iain Sproat, 2012.
// </copyright>
//-----------------------------------------------------------------------
using System;
using NUnit.Framework;
using SharpFE;

namespace SharpFE.Core.Tests.Elements
{
    [TestFixture]
    public class Linear1DBeamElementTest
    {
    	private NodeFactory nodeFactory;
        private ElementFactory elementFactory;
        private FiniteElementNode start;
        private FiniteElementNode end;
        private GenericElasticMaterial material;
        private SolidRectangle section;
        private Linear1DBeam SUT;
        
        [SetUp]
        public void SetUp()
        {
        	nodeFactory = new NodeFactory(ModelType.Truss1D);
            start = nodeFactory.Create(0);
            end = nodeFactory.Create(1);
            elementFactory = new ElementFactory();
			material = new GenericElasticMaterial(0, 0.1, 0, 0);
			section = new SolidRectangle(0.1, 1);
            SUT = elementFactory.CreateLinear1DBeam(start, end, material, section);
        }
        
        [Test]
        public void It_can_be_constructed()
        {
        	Assert.IsNotNull(SUT);
        }
        
        [Test]
        public void HasCorrectSupportedDOF()
        {
        	Assert.IsFalse(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.X));
        	Assert.IsFalse(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.Y));
        	Assert.IsTrue(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.Z));
        	Assert.IsFalse(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.XX));
        	Assert.IsTrue(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.YY));
        	Assert.IsFalse(SUT.IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom.ZZ));
        }
        
        [Test]
        public void ElementEndsCanBeReleased()
        {
            Assert.Ignore();
        }
    }
}