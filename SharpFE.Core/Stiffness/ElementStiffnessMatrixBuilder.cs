﻿namespace SharpFE.Stiffness
{
    using System;
    using System.Collections.Generic;
    using SharpFE;
    
    public abstract class ElementStiffnessMatrixBuilder<T> : IStiffnessProvider
        where T : FiniteElement
    {
        /// <summary>
        /// The global stiffness matrix of this element
        /// </summary>
        private StiffnessMatrix _globalStiffnessMatrix;
        
        private int elementStateAtWhichGlobalStiffnessMatrixWasLastBuilt;
        
        protected ElementStiffnessMatrixBuilder(T finiteElement)
        {
            Guard.AgainstNullArgument(finiteElement, "finiteElement");
            this.Element = finiteElement;
            
            this.HasBeenInitialized = true;
        }
        
        public T Element
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets the stiffness matrix of this element.
        /// </summary>
        public StiffnessMatrix GlobalStiffnessMatrix
        {
            get
            {
                if (this.Element.IsDirty(this.elementStateAtWhichGlobalStiffnessMatrixWasLastBuilt))
                {
                    this.BuildGlobalStiffnessMatrix();
                    this.elementStateAtWhichGlobalStiffnessMatrixWasLastBuilt = this.Element.GetHashCode();
                }
                
                return this._globalStiffnessMatrix;
            }
        }
        
        protected bool HasBeenInitialized
        {
            get;
            private set;
        }
        
        public abstract KeyedRowColumnMatrix<DegreeOfFreedom, NodalDegreeOfFreedom> GetShapeFunctionVector(FiniteElementNode location);
        public abstract KeyedRowColumnMatrix<Strain, NodalDegreeOfFreedom> GetStrainDisplacementMatrix();
        public abstract StiffnessMatrix GetLocalStiffnessMatrix();
        
        /// <summary>
        /// Gets the exact stiffness value for a given node and degree of freedom combinations.
        /// </summary>
        /// <param name="rowNode">The node defining the row (force equations)</param>
        /// <param name="rowDegreeOfFreedom">The degree of freedom defining the row (force equations)</param>
        /// <param name="columnNode">The node defining the column (displacement equations)</param>
        /// <param name="columnDegreeOfFreedom">the degree of freedom defining the column (displacement equations)</param>
        /// <returns>A value representing the stiffness at the given locations</returns>
        /// <exception cref="ArgumentException">Thrown if either of the nodes is not part of this element, or either of the degrees of freedom are not supported by this element.</exception>
        public double GetGlobalStiffnessAt(FiniteElementNode rowNode, DegreeOfFreedom rowDegreeOfFreedom, FiniteElementNode columnNode, DegreeOfFreedom columnDegreeOfFreedom)
        {
            if (rowNode == null)
            {
                throw new ArgumentNullException("rowNode");
            }
            
            if (columnNode == null)
            {
                throw new ArgumentNullException("columnNode");
            }
            
            return this.GlobalStiffnessMatrix.At(rowNode, rowDegreeOfFreedom, columnNode, columnDegreeOfFreedom);
        }
        
        /// <summary>
        /// Prepares and generates the stiffness matrix.
        /// It creates an entirely new matrix from the current set of nodes and the supported degrees of freedom of this element.
        /// It calls the GenerateStiffnessMatrix method which inheriting classes are expected to implement.
        /// It sets the stiffnessMatrixHasBeenGenerated flag to true.
        /// </summary>
        private void BuildGlobalStiffnessMatrix()
        {
            this.ThrowIfNotInitialized();
            
            StiffnessMatrix k = this.GetLocalStiffnessMatrix();
            if (k.Determinant() != 0)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "The stiffness matrix for an individual element should be singular and non-invertible. i.e. it should have a zero determinant.  This is not the case for element {0} of type {1}",
                    this.Element,
                    this.Element.GetType()));
            }
            
            KeyedMatrix<NodalDegreeOfFreedom> t = this.BuildStiffnessRotationMatrixFromLocalToGlobalCoordinates();
            
            ////FIXME these multiplications assume the keys of both matrices are ordered and identical
            KeyedMatrix<NodalDegreeOfFreedom> kt = k.Multiply(t); // K*T
            KeyedMatrix<NodalDegreeOfFreedom> ttransposed = t.Transpose(); // T^
            KeyedMatrix<NodalDegreeOfFreedom> ttransposedkt = ttransposed.Multiply(kt); // (T^)*K*T
            this._globalStiffnessMatrix = new StiffnessMatrix(ttransposedkt);
            
            if (this._globalStiffnessMatrix.Determinant() != 0)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "The global stiffness matrix for an individual element should be singular and non-invertible. i.e. it should have a zero determinant.  This is not the case for element {0} of type {1}",
                    this.Element,
                    this.Element.GetType()));
            }
        }
        
        /// <summary>
        /// Builds the rotational matrix from local coordinates to global coordinates.
        /// </summary>
        private KeyedMatrix<NodalDegreeOfFreedom> BuildStiffnessRotationMatrixFromLocalToGlobalCoordinates()
        {
            this.ThrowIfNotInitialized();
            
            IList<DegreeOfFreedom> dof = new List<DegreeOfFreedom>(3) { DegreeOfFreedom.XX, DegreeOfFreedom.YY, DegreeOfFreedom.ZZ };
            
            KeyedMatrix<DegreeOfFreedom> rotationMatrix = this.CalculateElementRotationMatrix();
            
            KeyedMatrix<NodalDegreeOfFreedom> elementRotationMatrixFromLocalToGlobalCoordinates = new KeyedMatrix<NodalDegreeOfFreedom>(this.Element.SupportedNodalDegreeOfFreedoms);

            int numberOfNodes = this.Element.Nodes.Count;
            foreach (FiniteElementNode node in this.Element.Nodes)
            {
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), rotationMatrix.At(DegreeOfFreedom.X, DegreeOfFreedom.X));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), rotationMatrix.At(DegreeOfFreedom.X, DegreeOfFreedom.Y));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), rotationMatrix.At(DegreeOfFreedom.X, DegreeOfFreedom.Z));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), rotationMatrix.At(DegreeOfFreedom.Y, DegreeOfFreedom.X));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), rotationMatrix.At(DegreeOfFreedom.Y, DegreeOfFreedom.Y));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), rotationMatrix.At(DegreeOfFreedom.Y, DegreeOfFreedom.Z));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), new NodalDegreeOfFreedom(node, DegreeOfFreedom.X), rotationMatrix.At(DegreeOfFreedom.Z, DegreeOfFreedom.X));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Y), rotationMatrix.At(DegreeOfFreedom.Z, DegreeOfFreedom.Y));
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), new NodalDegreeOfFreedom(node, DegreeOfFreedom.Z), rotationMatrix.At(DegreeOfFreedom.Z, DegreeOfFreedom.Z));
                
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.XX), new NodalDegreeOfFreedom(node, DegreeOfFreedom.XX), 1.0);
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.YY), new NodalDegreeOfFreedom(node, DegreeOfFreedom.YY), 1.0);
                elementRotationMatrixFromLocalToGlobalCoordinates.At(new NodalDegreeOfFreedom(node, DegreeOfFreedom.ZZ), new NodalDegreeOfFreedom(node, DegreeOfFreedom.ZZ), 1.0);
            }
            
            return elementRotationMatrixFromLocalToGlobalCoordinates;
        }
        
        internal KeyedMatrix<DegreeOfFreedom> CalculateElementRotationMatrix()
        {
            this.ThrowIfNotInitialized();
            
            KeyedMatrix<DegreeOfFreedom> rotationMatrix = CreateFromRows(this.Element.LocalXAxis, this.Element.LocalYAxis, this.Element.LocalZAxis);
            rotationMatrix = rotationMatrix.NormalizeRows(2);
            return rotationMatrix;
        }
        
        private static KeyedMatrix<DegreeOfFreedom> CreateFromRows(KeyedVector<DegreeOfFreedom> axis1, KeyedVector<DegreeOfFreedom> axis2, KeyedVector<DegreeOfFreedom> axis3)
        {
            Guard.AgainstBadArgument(
                () => { return axis1.Count != 3; },
                "All axes should be 3D, i.e. have 3 items",
                "axis1");
            Guard.AgainstBadArgument(
                () => { return axis2.Count != 3; },
                "All axes should be 3D, i.e. have 3 items",
                "axis2");
            Guard.AgainstBadArgument(
                () => { return axis3.Count != 3; },
                "All axes should be 3D, i.e. have 3 items",
                "axis3");
            Guard.AgainstBadArgument(
                () => { return axis1.SumMagnitudes() == 0; },
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Axis should not be zero: {0}",
                    axis1),
                "axis1");
            Guard.AgainstBadArgument(
                () => { return axis2.SumMagnitudes() == 0; },
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Axis should not be zero: {0}",
                    axis2),
                "axis2");
            Guard.AgainstBadArgument(
                () => { return axis3.SumMagnitudes() == 0; },
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Axis should not be zero: {0}",
                    axis3),
                "axis3");
            
            KeyedVector<DegreeOfFreedom> axis1Norm = axis1.Normalize(2);
            KeyedVector<DegreeOfFreedom> axis2Norm = axis2.Normalize(2);
            KeyedVector<DegreeOfFreedom> axis3Norm = axis3.Normalize(2);
            
            IList<DegreeOfFreedom> dof = new List<DegreeOfFreedom>(3) { DegreeOfFreedom.X, DegreeOfFreedom.Y, DegreeOfFreedom.Z };
            
            KeyedMatrix<DegreeOfFreedom> result = new KeyedMatrix<DegreeOfFreedom>(dof);
            result.At(DegreeOfFreedom.X, DegreeOfFreedom.X, axis1Norm[DegreeOfFreedom.X]);
            result.At(DegreeOfFreedom.X, DegreeOfFreedom.Y, axis1Norm[DegreeOfFreedom.Y]);
            result.At(DegreeOfFreedom.X, DegreeOfFreedom.Z, axis1Norm[DegreeOfFreedom.Z]);
            result.At(DegreeOfFreedom.Y, DegreeOfFreedom.X, axis2Norm[DegreeOfFreedom.X]);
            result.At(DegreeOfFreedom.Y, DegreeOfFreedom.Y, axis2Norm[DegreeOfFreedom.Y]);
            result.At(DegreeOfFreedom.Y, DegreeOfFreedom.Z, axis2Norm[DegreeOfFreedom.Z]);
            result.At(DegreeOfFreedom.Z, DegreeOfFreedom.X, axis3Norm[DegreeOfFreedom.X]);
            result.At(DegreeOfFreedom.Z, DegreeOfFreedom.Y, axis3Norm[DegreeOfFreedom.Y]);
            result.At(DegreeOfFreedom.Z, DegreeOfFreedom.Z, axis3Norm[DegreeOfFreedom.Z]);
            
            return result;
        }
        
        protected void ThrowIfNotInitialized()
        {
            if (!this.HasBeenInitialized)
            {
                throw new InvalidOperationException("This StiffnessMatrixBuilder has not been initialized correctly.  The Initialize(FiniteElement) method should be called first.");
            }
        }
    }
}