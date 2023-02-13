using System;


namespace Clatter.Core
{
    /// <summary>
    /// A partial implementation of a non-Unity Vector3 that uses doubles. Source: https://github.com/sldsmkd/vector3d/blob/master/Vector3d.cs
    /// </summary>
    public struct Vector3d
    {
        /// <summary>
        /// The vector data.
        /// </summary>
        private readonly double[] vector;
        /// <summary>
        /// The x coordinate.
        /// </summary>
        public double X
        {
            get
            {
                return vector[0];
            }
            set
            {
                vector[0] = value;
            }
        }
        /// <summary>
        /// The y coordinate.
        /// </summary>
        public double Y
        {
            get
            {
                return vector[1];
            }
            set
            {
                vector[1] = value;
            }
        }
        /// <summary>
        /// The z coordinate.
        /// </summary>
        public double Z
        {
            get
            {
                return vector[2];
            }
            set
            {
                vector[2] = value;
            }
        }
        /// <summary>
        /// The magnitude of the vector.
        /// </summary>
        public double Magnitude
        {
            get
            {
                return Math.Sqrt(SqrMagnitude);
            }
        }
        /// <summary>
        /// The square root magnitude.
        /// </summary>
        public double SqrMagnitude
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }
        /// <summary>
        /// Vector (0, 0, 0).
        /// </summary>
        public static Vector3d Zero
        {
            get
            {
                return new Vector3d(0d, 0d, 0d);
            }
        }


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public Vector3d(double x, double y, double z)
        {
            vector = new double[3];
            X = x;
            Y = y;
            Z = z;
        }


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="vector">The vector. We assume this has exactly 3 elements.</param>
        public Vector3d(double[] vector)
        {
            this.vector = new double[3];
            Buffer.BlockCopy(vector, 0, this.vector, 0, vector.Length * 8);
        }
        
        
        /// <summary>
        /// Stringifies this vector.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + X + "; " + Y + "; " + Z + ")";
        }
        

        /// <summary>
        /// Normalize this vector.
        /// </summary>
        public void Normalize()
        {
            double magnitude = Magnitude;
            if (magnitude > 9.99999974737875E-06)
            {
                this /= magnitude;
            }
            else
            {
                this = Zero;
            }  
        }
        
        
        /// <summary>
        /// Copy another Vector3d into this one. This is faster than using the = operator to set a new value.
        /// </summary>
        /// <param name="v">The other Vector3d.</param>
        public void CopyTo(Vector3d v)
        {
            Buffer.BlockCopy(vector, 0, v.vector, 0, vector.Length * 8);
        }


        /// <summary>
        /// Equality comparison.
        /// </summary>
        /// <param name="other">The other object.</param>
        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
            {
                return false;
            }
            Vector3d vector3d = (Vector3d)other;
            return X.Equals(vector3d.X) && Y.Equals(vector3d.Y) && Z.Equals(vector3d.Z);
        }


        /// <summary>
        /// Returns the hashcode.
        /// </summary>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }
        
        
        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static double Distance(Vector3d a, Vector3d b)
        {
            return (a - b).Magnitude;
        }


        /// <summary>
        /// Vector addition.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        

        /// <summary>
        /// Vector subtraction.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        /// <summary>
        /// Vector negation.
        /// </summary>
        /// <param name="v">The vector.</param>
        public static Vector3d operator -(Vector3d v)
        {
            return new Vector3d(-v.X, -v.Y, -v.Z);
        }
        
        
        /// <summary>
        /// Vector multiplication.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="d">The scalar.</param>
        public static Vector3d operator *(Vector3d v, double d)
        {
            return new Vector3d(v.X * d, v.Y * d, v.Z * d);
        }
        
        
        /// <summary>
        /// Vector multiplication.
        /// </summary>
        /// <param name="d">The scalar.</param>
        /// <param name="v">The vector.</param>
        public static Vector3d operator *(double d, Vector3d v)
        {
            return new Vector3d(v.X * d, v.Y * d, v.Z * d);
        }


        /// <summary>
        /// Vector division.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="d">The scalar.</param>
        public static Vector3d operator /(Vector3d v, double d)
        {
            return new Vector3d(v.X / d, v.Y / d, v.Z / d);
        }


        /// <summary>
        /// Vector equality.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static bool operator ==(Vector3d a, Vector3d b)
        {
            return (a - b).SqrMagnitude < 0.0 / 1.0;
        }


        /// <summary>
        /// Vector inequality.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static bool operator !=(Vector3d a, Vector3d b)
        {
            return (a - b).SqrMagnitude >= 0.0 / 1.0;
        }
    }
}