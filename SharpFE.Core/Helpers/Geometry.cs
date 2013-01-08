﻿//-----------------------------------------------------------------------
// <copyright file="?.cs" company="Iain Sproat">
//     Copyright Iain Sproat, 2012.
// </copyright>
//-----------------------------------------------------------------------
using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace SharpFE
{
	/// <summary>
	/// Description of Geometry.
	/// </summary>
	public class Geometry
	{
		public static Vector VectorBetweenPointAndLine(Vector point, Vector pointOnLine, Vector vectorDefiningLine)
		{
			Vector<double> betweenPoints = point.Subtract(pointOnLine);
			Vector<double> lineVector = vectorDefiningLine.Normalize(2);
			double projectionDistanceOfEndPointAlongSide1 = betweenPoints.DotProduct(lineVector);
			
			Vector<double> vectorFromNode1 = lineVector.Multiply(projectionDistanceOfEndPointAlongSide1);
			Vector<double> endPointOfPerpendicularLine = pointOnLine.Add(vectorFromNode1);
			
			Vector<double> result = endPointOfPerpendicularLine.Subtract(point);
			return (Vector)result;
		}
		
		public static double AreaTriangle(FiniteElementNode node0, FiniteElementNode node1, FiniteElementNode node2)
		{
			return Geometry.AreaTriangle(node0.AsVector(), node1.AsVector(), node2.AsVector());
		}
		
		public static double AreaTriangle(Vector point0, Vector point1, Vector point2)
		{
			Vector side01 = (Vector)point1.Subtract(point0);
			Vector side02 = (Vector)point2.Subtract(point0);
			
			Vector crossProduct = side01.CrossProduct(side02);
			double quadArea = crossProduct.Norm(2);
			return quadArea / 2.0;
		}
		
		/// <summary>
		/// Nodes to be specified in clockwise order
		/// </summary>
		/// <param name="node0"></param>
		/// <param name="node1"></param>
		/// <param name="node2"></param>
		/// <param name="node3"></param>
		/// <returns></returns>
		public static double AreaQuadrilateral(FiniteElementNode node0, FiniteElementNode node1, FiniteElementNode node2, FiniteElementNode node3)
		{
			return Geometry.AreaQuadrilateral(node0.AsVector(), node1.AsVector(), node2.AsVector(), node3.AsVector());
		}
		
		/// <summary>
		/// Vectors to be provided in clockwise order
		/// </summary>
		/// <param name="point0"></param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <returns></returns>
		public static double AreaQuadrilateral(Vector point0, Vector point1, Vector point2, Vector point3)
		{
			Vector diagonal1 = (Vector)point2.Subtract(point0);
			Vector diagonal2 = (Vector)point3.Subtract(point1);
			
			double crossProduct = diagonal1[0] * diagonal2[1] - diagonal1[1] * diagonal2[0];
			return 0.5 * crossProduct;
		}
	}
}