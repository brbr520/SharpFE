﻿// <copyright file="DenseKeyedMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Keyed.Double
{
    using System;
    using Algorithms.LinearAlgebra;
    using Distributions;
    using Generic;
    using Properties;
    using Storage;
    using Threading;

    /// <summary>
    /// A Matrix class with dense storage. The underlying storage is a one dimensional array in column-major order.
    /// </summary>
    [Serializable]
    public class DenseKeyedMatrix : KeyedMatrix
    {
        /// <summary>
        /// Number of rows.
        /// </summary>
        /// <remarks>Using this instead of the RowCount property to speed up calculating
        /// a matrix index in the data array.</remarks>
        readonly int _rowCount;

        /// <summary>
        /// Number of columns.
        /// </summary>
        /// <remarks>Using this instead of the ColumnCount property to speed up calculating
        /// a matrix index in the data array.</remarks>
        readonly int _columnCount;

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        readonly double[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class.
        /// </summary>
        public DenseKeyedMatrix(DenseKeyedColumnMajorMatrixStorage<double> storage)
            : base(storage)
        {
            _rowCount = storage.RowCount;
            _columnCount = storage.ColumnCount;
            _values = storage.Data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public DenseKeyedMatrix(int order)
            : this(new DenseKeyedColumnMajorMatrixStorage<double>(order, order))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        public DenseKeyedMatrix(int rows, int columns)
            : this(new DenseKeyedColumnMajorMatrixStorage<double>(rows, columns))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class with all entries set to a particular value.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="value">The value which we assign to each element of the matrix.</param>
        public DenseKeyedMatrix(int rows, int columns, double value)
            : this(rows, columns)
        {
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class from a one dimensional array. This constructor
        /// will reference the one dimensional array and not copy it.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. see: http://en.wikipedia.org/wiki/Row-major_order </param>
        public DenseKeyedMatrix(int rows, int columns, double[] array)
            : this(new DenseKeyedColumnMajorMatrixStorage<double>(rows, columns, array))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class from a 2D array. This constructor
        /// will allocate a completely new memory block for storing the dense matrix.
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        public DenseKeyedMatrix(double[,] array)
            : this(array.GetLength(0), array.GetLength(1))
        {
            for (var i = 0; i < _rowCount; i++)
            {
                for (var j = 0; j < _columnCount; j++)
                {
                    _values[(j * _rowCount) + i] = array[i, j];
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseKeyedMatrix"/> class, copying
        /// the values from the given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to copy.</param>
        public DenseKeyedMatrix(KeyedMatrix<double> matrix)
            : this(matrix.RowCount, matrix.ColumnCount)
        {
            matrix.Storage.CopyToUnchecked(Storage, skipClearing: true);
        }

        /// <summary>
        /// Create a new dense matrix with values sampled from the provided random distribution.
        /// </summary>
        public static DenseKeyedMatrix CreateRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            var storage = new DenseKeyedColumnMajorMatrixStorage<double>(rows, columns);
            for (var i = 0; i < storage.Data.Length; i++)
            {
                storage.Data[i] = distribution.Sample();
            }
            return new DenseKeyedMatrix(storage);
        }

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        [Obsolete("Use Values instead. Will be removed in future versions.")]
        public double[] Data
        {
            get { return _values; }
        }

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        public double[] Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Creates a <c>DenseKeyedMatrix</c> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        /// <param name="fullyMutable">True if all fields must be mutable (e.g. not a diagonal matrix).</param>
        /// <returns>
        /// A <c>DenseKeyedMatrix</c> with the given dimensions.
        /// </returns>
        public override KeyedMatrix<double> CreateMatrix(int numberOfRows, int numberOfColumns, bool fullyMutable = false)
        {
            return new DenseKeyedMatrix(numberOfRows, numberOfColumns);
        }

        /// <summary>
        /// Creates a <see cref="Vector{T}"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <param name="fullyMutable">True if all fields must be mutable.</param>
        /// <returns>
        /// A <see cref="Vector{T}"/> with the given dimension.
        /// </returns>
        public override KeyedVector<double> CreateVector(int size, bool fullyMutable = false)
        {
            return new DenseKeyedVector(size);
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>        
        /// <returns>The transpose of this matrix.</returns>
        public override KeyedMatrix<double> Transpose()
        {
            var ret = new DenseKeyedMatrix(_columnCount, _rowCount);
            for (var j = 0; j < _columnCount; j++)
            {
                var index = j * _rowCount;
                for (var i = 0; i < _rowCount; i++)
                {
                    ret._values[(i * _columnCount) + j] = _values[index + i];
                }
            }

            return ret;
        }

        /// <summary>Calculates the L1 norm.</summary>
        /// <returns>The L1 norm of the matrix.</returns>
        public override double L1Norm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.OneNorm, _rowCount, _columnCount, _values);
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public override double FrobeniusNorm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, _rowCount, _columnCount, _values);
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>  
        public override double InfinityNorm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, _rowCount, _columnCount, _values);
        }

        #region Static constructors for special matrices.

        /// <summary>
        /// Initializes a square <see cref="DenseKeyedMatrix"/> with all zero's except for ones on the diagonal.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <returns>A dense identity matrix.</returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public static DenseKeyedMatrix Identity(int order)
        {
            var m = new DenseKeyedMatrix(order);
            for (var i = 0; i < order; i++)
            {
                m._values[(i * order) + i] = 1.0;
            }

            return m;
        }

        #endregion

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of add</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.AddArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected override void DoSubtract(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.SubtractArrays(_values, denseOther._values, denseResult._values);
            }
        }
    
        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(double scalar, KeyedMatrix<double> result)
        {
            var denseResult = result as DenseKeyedMatrix;
            if (denseResult == null)
            {
                base.DoMultiply(scalar, result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(scalar, _values, denseResult._values);
            }
        }
     
        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(KeyedVector<double> rightSide, KeyedVector<double> result)
        {
            var denseRight = rightSide as DenseKeyedVector;
            var denseResult = result as DenseKeyedVector;

            if (denseRight == null || denseResult == null)
            {
                base.DoMultiply(rightSide, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    _values,
                    _rowCount,
                    _columnCount,
                    denseRight.Values,
                    denseRight.Count,
                    1,
                    0.0,
                    denseResult.Values);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    _values,
                    _rowCount,
                    _columnCount,
                    denseOther._values,
                    denseOther._rowCount,
                    denseOther._columnCount,
                    0.0,
                    denseResult._values);
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
             var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoTransposeAndMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    Algorithms.LinearAlgebra.Transpose.Transpose,
                    1.0,
                    _values,
                    _rowCount,
                    _columnCount,
                    denseOther._values,
                    denseOther._rowCount,
                    denseOther._columnCount,
                    0.0,
                    denseResult._values);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(KeyedVector<double> rightSide, KeyedVector<double> result)
        {
            var denseRight = rightSide as DenseKeyedVector;
            var denseResult = result as DenseKeyedVector;

            if (denseRight == null || denseResult == null)
            {
                base.DoTransposeThisAndMultiply(rightSide, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.Transpose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    _values,
                    _rowCount,
                    _columnCount,
                    denseRight.Values,
                    denseRight.Count,
                    1,
                    0.0,
                    denseResult.Values);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoTransposeThisAndMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.Transpose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    _values,
                    _rowCount,
                    _columnCount,
                    denseOther._values,
                    denseOther._rowCount,
                    denseOther._columnCount,
                    0.0,
                    denseResult._values);
            }
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(KeyedMatrix<double> result)
        {
            var denseResult = result as DenseKeyedMatrix;

            if (denseResult == null)
            {
                base.DoNegate(result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(-1, _values, denseResult._values);
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseMultiplyArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(KeyedMatrix<double> other, KeyedMatrix<double> result)
        {
            var denseOther = other as DenseKeyedMatrix;
            var denseResult = result as DenseKeyedMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseDivideArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoModulus(double divisor, KeyedMatrix<double> result)
        {
            var denseResult = result as DenseKeyedMatrix;

            if (denseResult == null)
            {
               base.DoModulus(divisor, result);
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(result);
                }

                CommonParallel.For(
                    0,
                    _values.Length,
                    index => denseResult._values[index] %= divisor);
            }
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public override double Trace()
        {
            if (_rowCount != _columnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            var sum = 0.0;
            for (var i = 0; i < _rowCount; i++)
            {
                sum += _values[(i * _rowCount) + i];
            }

            return sum;
        }

        /// <summary>
        /// Adds two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to add.</param>
        /// <param name="rightSide">The right matrix to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator +(DenseKeyedMatrix leftSide, DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide._rowCount != rightSide._rowCount || leftSide._columnCount != rightSide._columnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(leftSide, rightSide);
            }

            return (DenseKeyedMatrix)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Returns a <strong>Matrix</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The matrix to get the values from.</param>
        /// <returns>A matrix containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator +(DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseKeyedMatrix)rightSide.Clone();
        }

        /// <summary>
        /// Subtracts two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator -(DenseKeyedMatrix leftSide, DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide._rowCount != rightSide._rowCount || leftSide._columnCount != rightSide._columnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(leftSide, rightSide);
            }

            return (DenseKeyedMatrix)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates each element of the matrix.
        /// </summary>
        /// <param name="rightSide">The matrix to negate.</param>
        /// <returns>A matrix containing the negated values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator -(DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseKeyedMatrix)rightSide.Negate();
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator *(DenseKeyedMatrix leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseKeyedMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator *(double leftSide, DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseKeyedMatrix)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to multiply.</param>
        /// <param name="rightSide">The right matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static DenseKeyedMatrix operator *(DenseKeyedMatrix leftSide, DenseKeyedMatrix rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide._columnCount != rightSide._rowCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(leftSide, rightSide);
            }

            return (DenseKeyedMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> and a Vector.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The vector to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedVector operator *(DenseKeyedMatrix leftSide, DenseKeyedVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseKeyedVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a Vector and a <strong>Matrix</strong>.
        /// </summary>
        /// <param name="leftSide">The vector to multiply.</param>
        /// <param name="rightSide">The matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedVector operator *(DenseKeyedVector leftSide, DenseKeyedMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseKeyedVector)rightSide.LeftMultiply(leftSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseKeyedMatrix operator %(DenseKeyedMatrix leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseKeyedMatrix)leftSide.Modulus(rightSide);
        }
    }
}