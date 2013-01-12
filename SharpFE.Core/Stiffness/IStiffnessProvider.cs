﻿//-----------------------------------------------------------------------
// <copyright file="?.cs" company="Iain Sproat">
//     Copyright Iain Sproat, 2013.
// </copyright>
//-----------------------------------------------------------------------
using System;

namespace SharpFE.Stiffness
{
    public interface IStiffnessProvider
    {
        StiffnessMatrix GlobalStiffnessMatrix { get; } ////FIXME only here for testing, to be removed
        double GetGlobalStiffnessAt(FiniteElementNode rowNode, DegreeOfFreedom rowDegreeOfFreedom, FiniteElementNode columnNode, DegreeOfFreedom columnDegreeOfFreedom);
    }
}