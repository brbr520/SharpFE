﻿namespace SharpFE
{
    using System;

    using SharpFE.Elements;
    using SharpFE.Stiffness;

    /// <summary>
    /// Linear element which has constant cross section
    /// and constant material properties.
    /// Also known as a Rod element.
    /// </summary>
    public class LinearTruss : FiniteElement1D, IHasMaterial, IHasConstantCrossSection
    {
        public LinearTruss(FiniteElementNode start, FiniteElementNode end, IMaterial material, ICrossSection crossSection)
            : base(start, end)
        {
            this.CrossSection = crossSection;
            this.Material = material;
        }
        
        public ICrossSection CrossSection
        {
            get;
            private set;
        }
        
        public IMaterial Material
        {
            get;
            private set;
        }
        
        public override bool IsASupportedBoundaryConditionDegreeOfFreedom(DegreeOfFreedom degreeOfFreedom)
        {
            switch (degreeOfFreedom)
            {
                case DegreeOfFreedom.X:
                    return true;
                case DegreeOfFreedom.Y:
                case DegreeOfFreedom.Z:
                case DegreeOfFreedom.XX:
                case DegreeOfFreedom.YY:
                case DegreeOfFreedom.ZZ:
                default:
                    return false;
            }
        }
        
        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            LinearTruss other = obj as LinearTruss;
            if (other == null)
                return false;
            return base.Equals(other) && object.Equals(this.Material, other.Material) && object.Equals(this.CrossSection, other.CrossSection);
        }
        
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                hashCode += base.GetHashCode();
                hashCode += 1000000007 * this.Material.GetHashCode();
                hashCode += 1000000022 * this.CrossSection.GetHashCode();
            }
            return hashCode;
        }
        
        public static bool operator ==(LinearTruss lhs, LinearTruss rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
                return true;
            if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null))
                return false;
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(LinearTruss lhs, LinearTruss rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
    }
}
