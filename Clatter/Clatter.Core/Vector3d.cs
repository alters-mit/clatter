using System;


namespace Clatter.Core
{
    /// <summary>
    /// A non-Unity Vector3 that uses doubles. Not everything is implemented.
    /// Source: https://github.com/sldsmkd/vector3d/blob/master/Vector3d.cs
    /// </summary>
    public struct Vector3d
    {
        /// <summary>
        /// The vector data.
        /// </summary>
        private double[] _vector;
        /// <summary>
        /// The x coordinate.
        /// </summary>
        public double x
        {
            get
            {
                return _vector[0];
            }
            set
            {
                _vector[0] = value;
            }
        }
        /// <summary>
        /// The y coordinate.
        /// </summary>
        public double y
        {
            get
            {
                return _vector[1];
            }
            set
            {
                _vector[1] = value;
            }
        }
        /// <summary>
        /// The z coordinate.
        /// </summary>
        public double z
        {
            get
            {
                return _vector[2];
            }
            set
            {
                _vector[2] = value;
            }
        }
        public Vector3d Normalized
        {
            get
            {
                Vector3d vector2d = new Vector3d(_vector);
                vector2d.Normalize();
                return vector2d;
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
                return x * x + y * y + z * z;
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
            _vector = new double[3];
            this.x = x;
            this.y = y;
            this.z = z;
        }


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="vector">The vector. We assume this has exactly 3 elements.</param>
        public Vector3d(double[] vector)
        {
            _vector = new double[3];
            Buffer.BlockCopy(vector, 0, this._vector, 0, vector.Length * 8);
        }
        


        public override string ToString()
        {
            return "(" + x + "; " + y + "; " + z + ")";
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
        
        
        public void CopyTo(Vector3d a)
        {
            Buffer.BlockCopy(_vector, 0, a._vector, 0, _vector.Length * 8);
        }


        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
            {
                return false;
            }
            Vector3d vector3d = (Vector3d)other;
            return x.Equals(vector3d.x) && y.Equals(vector3d.y) && z.Equals(vector3d.z);
        }


        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }
        
        
        public static double Distance(Vector3d a, Vector3d b)
        {
            return (a - b).Magnitude;
        }


        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }


        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }


        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }


        public static Vector3d operator *(Vector3d a, double d)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }


        public static Vector3d operator *(float d, Vector3d a)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }


        public static Vector3d operator /(Vector3d a, double d)
        {
            return new Vector3d(a.x / d, a.y / d, a.z / d);
        }


        public static bool operator ==(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).SqrMagnitude < 0.0 / 1.0;
        }


        public static bool operator !=(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).SqrMagnitude >= 0.0 / 1.0;
        }
    }
}